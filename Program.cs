using AstralDiaryApi.Data;
using AstralDiaryApi.env;
using AstralDiaryApi.Services.Implementations;
using AstralDiaryApi.Services.Interfaces;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Firebase Admin SDK Initialization
FirebaseApp.Create(
    new AppOptions()
    {
        Credential = GoogleCredential.GetApplicationDefault(),
        ProjectId = "astral-diary",
    }
);

// Get connection string from config then add db context
//var connectionString = builder.Configuration["ConnectionString:DefaultConnection"];
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string is not set.");
}
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null
            );
        }
    )
);

// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEntryService, EntryService>();
builder.Services.AddScoped<IDraftService, DraftService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

builder.Services.AddMemoryCache();

// Environment
builder.Services.AddSingleton<MyEnvironment>();

// Controllers
builder.Services.AddControllers();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        //policy.WithOrigins(["https://astral-diary.web.app", "https://astral-diary.firebaseapp.com"])
        //      .AllowAnyHeader()
        //      .AllowAnyMethod();
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
//builder.Services.AddEndpointsApiExplorer();
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

app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors("AllowDevelopmentOrigins");
}
else
{
    app.UseCors();
}

app.UseExceptionHandler("/error");
app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthorization();
app.MapControllers();
app.Run();
