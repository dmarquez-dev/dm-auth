using System.Text.Json.Serialization;
using DMAuth.Application;
using DMAuth.Infrastructure;
using DMAuth.Web.Common.CurrentUser;
using DMAuth.Web.Common.Middleware;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.OpenApi;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, loggerConfig) =>
	loggerConfig.ReadFrom.Configuration(context.Configuration));

// Add layer services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Configure cookie authentication
builder.Services
	.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(options =>
	{
		options.Cookie.HttpOnly = true;
		options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
		options.Cookie.SameSite = SameSiteMode.Strict;
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
