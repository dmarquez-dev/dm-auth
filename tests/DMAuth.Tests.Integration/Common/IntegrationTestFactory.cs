using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using DMAuth.Application.Common.Settings;
using DMAuth.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace DMAuth.Tests.Integration.Common;

/// <summary>
///		Configures a test host backed by an in-memory database for integration testing.
///		Each factory instance uses an isolated database, so tests within a class fixture
///		do not interfere with one another as long as they use unique user data.
/// </summary>
public class IntegrationTestFactory : WebApplicationFactory<Program>
{
	private readonly string _databaseName = Guid.NewGuid().ToString();

	/// <summary>
	///		A self-signed RSA private key generated once per test session for JWT signing.
	///		Shared across all factory instances so the key does not have to be re-generated
	///		for every test class fixture.
	/// </summary>
	private static readonly string TestRsaPrivateKeyPem = CreateTestRsaKey();

	private static string CreateTestRsaKey()
	{
		using var rsa = RSA.Create(2048);
		return rsa.ExportRSAPrivateKeyPem();
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		// Prevent the Key Vault bootstrap in Program.cs from running during tests.
		// An empty VaultUri means the AddAzureKeyVault call is skipped.
		// ConfigureAppConfiguration runs after default sources so this in-memory entry
		// overrides the value set in appsettings.Development.json.
		builder.ConfigureAppConfiguration(config =>
			config.AddInMemoryCollection(new Dictionary<string, string?>
			{
				["KeyVault:VaultUri"] = string.Empty,
			}));

		// ConfigureTestServices runs after the app's own ConfigureServices, ensuring
		// our overrides take effect after SQL Server is registered by the app.
		builder.ConfigureTestServices(services =>
		{
			// Replace SQL Server DbContext with in-memory database.
			// EF Core 10 tracks the configured provider through IDbContextOptionsConfiguration<T>.
			// Leaving SQL Server's entry alongside InMemory triggers the "two providers" error,
			// so both it and the typed options must be removed before re-registering with InMemory.
			services.RemoveAll<DbContextOptions<DmAuthDbContext>>();
			services.RemoveAll<IDbContextOptionsConfiguration<DmAuthDbContext>>();

			services.AddDbContext<DmAuthDbContext>(options =>
				options.UseInMemoryDatabase(_databaseName));

			// Allow the session cookie to be sent over HTTP in the test server
			services.Configure<CookieAuthenticationOptions>(
				CookieAuthenticationDefaults.AuthenticationScheme,
				options => options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest);

			// Replace the production JwtSettings singleton with a test-specific configuration
			// that has a valid RSA key so TokenService can sign JWTs during token endpoint tests.
			services.RemoveAll<JwtSettings>();
			services.AddSingleton(new JwtSettings
			{
				RsaPrivateKeyPem = TestRsaPrivateKeyPem,
				Issuer = "https://test.dmauth.local",
				Audience = "https://test.dmauth.local",
				AccessTokenExpiryMinutes = 15,
				IdTokenExpiryMinutes = 60
			});
		});

		// Suppress logging noise during test runs
		builder.ConfigureLogging(logging => logging.ClearProviders());
	}

	/// <summary>
	///		Creates a JWT access token signed with the test RSA key but with an expiry one hour
	///		in the past. Use this to verify that protected endpoints correctly reject expired tokens.
	/// </summary>
	public string CreateExpiredAccessToken(Guid userId, string scope = "openid")
	{
		var rsa = RSA.Create();
		rsa.ImportFromPem(TestRsaPrivateKeyPem);

		var credentials = new SigningCredentials(
			new RsaSecurityKey(rsa),
			SecurityAlgorithms.RsaSha256);

		var token = new JwtSecurityToken(
			issuer: "https://test.dmauth.local",
			audience: "https://test.dmauth.local",
			claims:
			[
				new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
				new Claim("scope", scope),
			],
			notBefore: DateTime.UtcNow.AddHours(-2),
			expires: DateTime.UtcNow.AddHours(-1),
			signingCredentials: credentials);

		var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
		rsa.Dispose();
		return tokenString;
	}

	/// <summary>
	///		Registers a new user and logs in, returning an <see cref="HttpClient"/>
	///		whose cookie container holds the active session.
	/// </summary>
	public async Task<HttpClient> CreateAuthenticatedClientAsync(
		string email,
		string username,
		string password = "Secure1!",
		string displayName = "Test User",
		CancellationToken cancellationToken = default)
	{
		var client = CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		await client.PostAsJsonAsync(
			"/api/users/register",
			new { email, username, password, displayName },
			cancellationToken);

		await client.PostAsJsonAsync(
			"/api/users/login",
			new { email, password },
			cancellationToken);

		return client;
	}
}
