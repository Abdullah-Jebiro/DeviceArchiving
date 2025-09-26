using DeviceArchiving.Data;
using DeviceArchiving.Data.Contexts;
using DeviceArchiving.Data.Dto;
using DeviceArchiving.Data.Entities;
using DeviceArchiving.Data.Enums;
using DeviceArchiving.Service;
using DeviceArchiving.Service.AccountServices;
using DeviceArchiving.Service.DeviceServices;
using DeviceArchiving.Service.OperationServices;
using DeviceArchiving.Service.OperationTypeServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
// 🔧 Configuration
var configuration = builder.Configuration;
var services = builder.Services;
// 🔌 Add Services
services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
// 🧩 Dependency Injection
services.AddScoped<IDeviceService, DeviceService>();
services.AddScoped<IOperationService, OperationService>();
services.AddScoped<IOperationTypeService, OperationTypeService>();
services.AddScoped<IAccountService, AccountService>();
// 🗄️ Database (SQL Server based on configuration)
services.AddHttpContextAccessor();
services.AddSingleton<HttpContextUserIdInterceptor>();
services.AddDbContextFactory<DeviceArchivingContext>((serviceProvider, options) =>
{
    var userIdInterceptor = serviceProvider.GetRequiredService<HttpContextUserIdInterceptor>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
        throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
    options.UseSqlServer(connectionString);
    options.AddInterceptors(userIdInterceptor);
});
// Bind JwtSettings
services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JWTSettings"));
// JWT Setup
var jwtSettings = builder.Configuration.GetSection("JWTSettings").Get<JwtSettings>();
// 🔐 JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings!.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
        };
    });
// Read allowed origins from configuration
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins!)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .WithExposedHeaders("x-pagination", "Authorization");
    });
});
var app = builder.Build();

app.UseMiddleware<DbExceptionMiddleware>();

// ⚙️ Apply Migrations in Production
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DeviceArchivingContext>();
    try
    {
        // التحقق من الاتصال بقاعدة البيانات
        bool canConnect = dbContext.Database.CanConnect();
        if (!canConnect)
        {
            app.Logger.LogError("Unable to connect to the database.");
            throw new InvalidOperationException("Unable to connect to the database.");
        }

        // Apply any pending migrations
        //dbContext.Database.Migrate();
        //app.Logger.LogInformation("Database migrations applied successfully.");

        // Seed admin user
        await SeedAdminUser(dbContext);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while applying database migrations.");
        throw; // توقف التطبيق إذا فشلت الـ migrations
    }
}

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/UI/");
        return;
    }

    await next();
});


app.UseSwagger();
app.UseSwaggerUI();

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/UI") &&
        !Path.HasExtension(context.Request.Path.Value))
    {
        context.Request.Path = "/UI/index.html";
    }
    await next();
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UI")),
    RequestPath = "/UI"
});




app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Method to seed the admin user
 static async Task SeedAdminUser(DeviceArchivingContext dbContext)
{
    var userName = AppSession.Admin;
    var adminUser = await dbContext.Users.FirstOrDefaultAsync(u => u.UserName == userName);
    if (adminUser == null)
    {
        adminUser = new User
        {
            UserName = userName,
            Password = BCrypt.Net.BCrypt.HashPassword(AppSession.AdminPassword),
            Role = UserRole.Admin

        };

        dbContext.Users.Add(adminUser);
        await dbContext.SaveChangesAsync();
    }
}