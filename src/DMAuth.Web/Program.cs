using System.Security.Cryptography;
using System.Text.Json.Serialization;
using Azure.Identity;
using DMAuth.Application;
using DMAuth.Application.Common.Settings;
using DMAuth.Infrastructure;
using DMAuth.Web.Common.CurrentUser;
using DMAuth.Web.Common.Middleware;
using DMAuth.Web.Common.SigningKey;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Bootstrap Azure Key Vault configuration before any service registration reads from IConfiguration.
// DefaultAzureCredential uses az login in dev and Managed Identity in production automatically.
var vaultUri = builder.Configuration.GetConnectionString("KeyVault");
if (string.IsNullOrEmpty(vaultUri))
{
	throw new InvalidOperationException("Key Vault connection string cannot be empty.");
}

builder.Configuration.AddAzureKeyVault(
	new Uri(vaultUri),
	new DefaultAzureCredential());

// Configure Serilog
builder.Host.UseSerilog((context, loggerConfig) =>
	loggerConfig.ReadFrom.Configuration(context.Configuration));

// Add layer services
builder.Services.AddApplication();
builder.Services.AddApplicationInsightsTelemetry(options =>
	options.ConnectionString = builder.Configuration.GetConnectionString("ApplicationInsights"));
builder.Services.AddInfrastructure(builder.Configuration);

// Configure authentication: cookies for the dashboard session, JWT Bearer for OAuth endpoints
builder.Services
	.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddJwtBearer()
	.AddCookie(options =>
	{
		options.Cookie.HttpOnly = true;
		options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
		options.Cookie.SameSite = SameSiteMode.None;
		options.Cookie.Name = "dm_auth_session";
		options.SlidingExpiration = true;
		options.ExpireTimeSpan = TimeSpan.FromHours(24);

		// Return 401/403 instead of redirecting — this is an API, not a web app
		options.Events.OnRedirectToLogin = context =>
		{
			context.Response.StatusCode = StatusCodes.Status401Unauthorized;
			return Task.CompletedTask;
		};
		options.Events.OnRedirectToAccessDenied = context =>
		{
			context.Response.StatusCode = StatusCodes.Status403Forbidden;
			return Task.CompletedTask;
		};
	});

// Add controllers
builder.Services.AddControllers()
	.AddJsonOptions(options =>
		options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Register Web-layer services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddSingleton<ISigningKeyProvider, SigningKeyProvider>();

// Configure JWT Bearer using ISigningKeyProvider so the RSA key is imported only once.
// Reconstructs a public-only RsaSecurityKey from the Base64Url-encoded parameters
// that SigningKeyProvider already computed at startup.
builder.Services
	.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
	.Configure<ISigningKeyProvider, JwtSettings>((options, signingKeyProvider, jwtSettings) =>
	{
		// Preserve JWT claim names as-is (e.g. "sub" stays "sub", not ClaimTypes.NameIdentifier).
		options.MapInboundClaims = false;

		var rsaParams = new RSAParameters
		{
			Modulus  = Base64UrlEncoder.DecodeBytes(signingKeyProvider.Modulus),
			Exponent = Base64UrlEncoder.DecodeBytes(signingKeyProvider.Exponent),
		};
		var publicKey = new RsaSecurityKey(RSA.Create(rsaParams))
		{
			KeyId = signingKeyProvider.KeyId,
		};

		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidIssuer      = jwtSettings.Issuer,
			ValidAudience    = jwtSettings.Audience,
			IssuerSigningKey = publicKey,
			ValidateLifetime = true,
			ClockSkew        = TimeSpan.Zero,
		};
	});

// Configure CORS for React SPA
builder.Services.AddCors(options =>
	options.AddPolicy(
		"AllowSpa",
		policy =>
		{
			var allowedOrigins = builder.Configuration
				.GetSection("Cors:AllowedOrigins")
				.Get<string[]>() ?? [];

			policy
				.WithOrigins(allowedOrigins)
				.AllowAnyHeader()
				.AllowAnyMethod()
				.AllowCredentials();
		}));

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc(
		"v1",
		new OpenApiInfo
		{
			Title = "DM Auth API",
			Version = "v1",
			Description = "OAuth 2.0 + OpenID Connect Authorization Server"
		});

	options.AddSecurityDefinition(
		"Bearer",
		new OpenApiSecurityScheme
		{
			Name = "Authorization",
			Type = SecuritySchemeType.Http,
			Scheme = "bearer",
			BearerFormat = "JWT",
			In = ParameterLocation.Header,
			Description = "Enter your JWT access token"
		});

	options.AddSecurityRequirement(document =>
		new OpenApiSecurityRequirement
		{
			{
				new OpenApiSecuritySchemeReference(
					"Bearer",
					document),
				new List<string>()
			}
		});
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors("AllowSpa");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
