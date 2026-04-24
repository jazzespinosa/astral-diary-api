using System.Text;
using AstralDiaryApi.Data;
using AstralDiaryApi.env;
using AstralDiaryApi.Middlewares;
using AstralDiaryApi.Services.Implementations;
using AstralDiaryApi.Services.Interfaces;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Oci.Common.Auth;
using Oci.ObjectstorageService;
using Oci.SecretsService;
using Oci.SecretsService.Requests;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    var loggerFactory = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<Program>();

    try
    {
        loggerFactory.LogInformation("Loading secrets from OCI Vault...");

        var ociRegion = builder.Configuration["OCI:Region"] ?? "ap-singapore-1";
        var connectionStringSecretId = builder.Configuration["OCI:ConnectionStringSecretId"];
        var googleAdcSecretId = builder.Configuration["OCI:GoogleAdcSecretId"];
        var pepperSecretId = builder.Configuration["OCI:PepperSecretId"];
        var configSecretId = builder.Configuration["OCI:ConfigSecretId"];

        if (
            string.IsNullOrEmpty(connectionStringSecretId)
            || string.IsNullOrEmpty(googleAdcSecretId)
            || string.IsNullOrEmpty(pepperSecretId)
            || string.IsNullOrEmpty(configSecretId)
        )
        {
            throw new InvalidOperationException("OCI Secret IDs are not properly configured!");
        }

        var provider = new InstancePrincipalsAuthenticationDetailsProvider();
        using var secretsClient = new SecretsClient(provider);
        secretsClient.SetRegion(ociRegion);

        loggerFactory.LogInformation("Fetching connection string from OCI...");
        var connString = await GetSecretValueAsync(
            secretsClient,
            connectionStringSecretId,
            loggerFactory
        );

        loggerFactory.LogInformation("Fetching Google ADC from OCI...");
        var googleAdcJson = await GetSecretValueAsync(
            secretsClient,
            googleAdcSecretId,
            loggerFactory
        );

        loggerFactory.LogInformation("Fetching pepper secret from OCI...");
        var pepperSecret = await GetSecretValueAsync(secretsClient, pepperSecretId, loggerFactory);

        loggerFactory.LogInformation("Fetching config secret from OCI...");
        var configSecretJson = await GetSecretValueAsync(
            secretsClient,
            configSecretId,
            loggerFactory
        );

        var secretsConfig = new Dictionary<string, string>
        {
            ["ConnectionStrings:DefaultConnection"] = connString,
            ["Crypto:ServerPepperSecret"] = pepperSecret,
            ["GoogleAdcJson"] = googleAdcJson,
        };

        builder.Configuration.AddInMemoryCollection(secretsConfig!);

        using var configStream = new MemoryStream(Encoding.UTF8.GetBytes(configSecretJson));
        builder.Configuration.AddJsonStream(configStream);

        loggerFactory.LogInformation("All secrets loaded successfully from OCI Vault.");
    }
    catch (Exception ex)
    {
        loggerFactory.LogError(ex, "FATAL: Failed to load secrets from OCI Vault");
        throw;
    }
}

if (builder.Environment.IsProduction())
{
    var googleAdcJson = builder.Configuration["GoogleAdcJson"];
    Console.WriteLine(
        $"ADC Found. Length: {googleAdcJson.Length}. Starts with: {googleAdcJson.Substring(0, 10)}..."
    );
    if (!string.IsNullOrEmpty(googleAdcJson))
    {
        FirebaseApp.Create(
            new AppOptions()
            {
                Credential = CredentialFactory
                    .FromJson<ServiceAccountCredential>(googleAdcJson)
                    .ToGoogleCredential(),
            }
        );
    }
}
else
{
    FirebaseApp.Create(
        new AppOptions()
        {
            Credential = GoogleCredential.GetApplicationDefault(),
            ProjectId = "astral-diary",
        }
    );
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("Connection string is not set.");
    }

    options.UseMySql(
        connectionString,
        new MariaDbServerVersion(new Version(10, 5)),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null
            );
        }
    );
});

// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEntryService, EntryService>();
builder.Services.AddScoped<IDraftService, DraftService>();
builder.Services.AddScoped<ObjectStorageClient>(provider =>
{
    var env = provider.GetRequiredService<IHostEnvironment>();
    var config = provider.GetRequiredService<IConfiguration>();
    var logger = provider.GetRequiredService<ILogger<ObjectStorageClient>>();

    IBasicAuthenticationDetailsProvider authProvider;

    if (env.IsProduction())
    {
        logger.LogInformation("Using Instance Principal authentication for OCI");
        authProvider = new InstancePrincipalsAuthenticationDetailsProvider();
    }
    else
    {
        logger.LogInformation("Using Config File authentication for OCI");
        var configFilePath = config["OciStorage:ConfigFilePath"] ?? "~/.oci/config";
        var profile = config["OciStorage:Profile"] ?? "DEFAULT";
        authProvider = new ConfigFileAuthenticationDetailsProvider(configFilePath, profile);
    }

    return new ObjectStorageClient(authProvider);
});
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IUtilityService, UtilityService>();

// Environment
builder.Services.AddSingleton<MyEnvironment>();

// Controllers
builder.Services.AddControllers();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins(["https://astral-diary.web.app", "https://astral-diary.firebaseapp.com"])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("Location");
        ;
    });
    options.AddPolicy(
        "AllowDevelopmentOrigins",
        policy =>
        {
            policy
                .WithOrigins(["http://localhost:4200"])
                .AllowAnyHeader()
                .AllowAnyMethod()
                .WithExposedHeaders("Location");
            ;
        }
    );
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT Authentication for Firebase
builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://securetoken.google.com/astral-diary";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://securetoken.google.com/astral-diary",
            ValidateAudience = true,
            ValidAudience = "astral-diary",
            ValidateLifetime = true,
        };
    });
builder.Services.AddAuthorization();

// Lowercase URLs
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
});

// Build
var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Custom logger middleware
app.UseMiddleware<RequestResponseLoggingMiddleware>();

// MySQL logger
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<AppDbContext>();

    try
    {
        // Send request to MySQL to verify the link
        if (await dbContext.Database.CanConnectAsync())
        {
            logger.LogInformation("Database connection verified successfully.");
        }
        else
        {
            logger.LogWarning("Database configuration is valid, but the server is unreachable.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while connecting to the database.");
    }
}

// Auto-migrate
if (app.Environment.IsProduction())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            if (context.Database.GetPendingMigrations().Any())
            {
                logger.LogInformation("Applying migrations...");
                context.Database.Migrate();
                logger.LogInformation("Database migration successful.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database.");
            throw;
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowDevelopmentOrigins");
}
else
{
    app.UseCors();
}

app.UseExceptionHandler("/error");
app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();
app.Run();

// Helper method
async Task<string> GetSecretValueAsync(SecretsClient client, string secretId, ILogger logger)
{
    try
    {
        var request = new GetSecretBundleRequest { SecretId = secretId };
        var response = await client.GetSecretBundle(request);

        var base64Content = (
            (Oci.SecretsService.Models.Base64SecretBundleContentDetails)
                response.SecretBundle.SecretBundleContent
        ).Content;

        var bytes = Convert.FromBase64String(base64Content);
        return Encoding.UTF8.GetString(bytes);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to fetch secret: {SecretId}", secretId);
        throw;
    }
}
