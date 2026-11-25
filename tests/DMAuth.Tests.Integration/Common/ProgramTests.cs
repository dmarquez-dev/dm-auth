using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DMAuth.Tests.Integration.Common;

/// <summary>
///		Integration tests for the startup configuration in Program.cs.
///		These tests build the host through <see cref="WebApplicationFactory{TEntryPoint}"/>
///		directly, not <see cref="IntegrationTestFactory"/>, so the Key Vault switch is never
///		turned off by the test host.
/// </summary>
public class ProgramTests
	: IDisposable
{
	/// <summary>
	///		An empty directory used as a content root that holds no appsettings files.
	/// </summary>
	private readonly DirectoryInfo _emptyContentRoot = Directory.CreateTempSubdirectory();

	[Fact]
	public void Startup_WithEmptyKeyVaultConnectionStringAndSwitchUnset_ThrowsInvalidOperationException()
	{
		// Arrange
		using var baseFactory = new WebApplicationFactory<Program>();
		using var factory = baseFactory.WithWebHostBuilder(builder =>
			builder.UseSetting("ConnectionStrings:KeyVault", string.Empty));

		// Act
		var act = () =>
			factory.CreateClient();

		// Assert
		act.Should()
			.Throw<InvalidOperationException>()
			.WithMessage("*Key Vault connection string cannot be empty.*");
	}

	[Fact]
	public void Startup_WithNoAppSettingsFiles_ThrowsInvalidOperationException()
	{
		// Arrange
		// With no appsettings files the Key Vault switch and connection string are both absent,
		// so startup depends on the default Program.cs applies to the switch.
		using var baseFactory = new WebApplicationFactory<Program>();
		using var factory = baseFactory.WithWebHostBuilder(builder =>
			builder.UseContentRoot(_emptyContentRoot.FullName));

		// Act
		var act = () =>
			factory.CreateClient();

		// Assert
		act.Should()
			.Throw<InvalidOperationException>()
			.WithMessage("*Key Vault connection string cannot be empty.*");
	}

	/// <summary>
	///		Deletes the empty content root created for this test instance.
	/// </summary>
	public void Dispose()
	{
		_emptyContentRoot.Delete(recursive: true);
	}
}
