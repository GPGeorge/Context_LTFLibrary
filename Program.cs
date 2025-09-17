using LTF_Library_V1.Data;
using LTF_Library_V1.Data.Models;
using LTF_Library_V1.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor(options =>
{
    options.DetailedErrors = true;
    // Force shorter retention for Chromium browsers
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromSeconds(30);
    options.DisconnectedCircuitMaxRetained = 5;
    // Chromium-specific timeout adjustments
    options.JSInteropDefaultCallTimeout = TimeSpan.FromSeconds(20);
});

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity Configuration (SINGLE configuration only)
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

    // Sign in settings
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>,
    UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>>();
// ============================
// Environment toggle
// ============================
bool isLocal = true; // <-- set false when publishing under /LTFCatalog

string basePath = isLocal ? "/" : "/LTFCatalog";
string loginPath = isLocal ? "/Account/login" : "/LTFCatalog/Account/login";
string logoutPath = isLocal ? "/Account/logout" : "/LTFCatalog/logout";
string accessDeniedPath = isLocal ? "/Account/access-denied" : "/LTFCatalog/access-denied";

// Configure Cookie Authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Path = basePath;
    options.LoginPath = loginPath;
    options.LogoutPath = logoutPath;
    options.AccessDeniedPath = accessDeniedPath;

    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; 
    options.Cookie.SameSite = SameSiteMode.Lax;
    // Custom redirect logic
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.Redirect(loginPath);  
        return Task.CompletedTask;
    };
});

// Register your custom services
builder.Services.AddScoped<IPublicationService, PublicationService>();
builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRequestManagementService, RequestManagementService>();

// Add HttpContextAccessor for accessing current user in services
builder.Services.AddHttpContextAccessor();

// Add Authorization (SINGLE configuration only)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("StaffOrAdmin", policy => policy.RequireRole("Staff", "Admin"));
    options.AddPolicy("AdminOrPublic", policy => policy.RequireRole("Admin", "Public"));
});

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddScoped<AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

if (!isLocal)
{
    // UsePathBase only needed for subsite
    app.UsePathBase("/LTFCatalog");
}

app.UseRouting();

// Add Authentication and Authorization middleware (SINGLE occurrence only)
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapBlazorHub();
app.MapControllers();
app.MapFallbackToPage("/_Host");

Console.WriteLine("=== BEFORE SEEDING BLOCK ===");
// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    Console.WriteLine("=== INSIDE SEEDING SCOPE ===");
    var services = scope.ServiceProvider;
    try
    {
        Console.WriteLine("=== GETTING SERVICES ===");
        var context = services.GetRequiredService<ApplicationDbContext>();
        Console.WriteLine("=== GOT CONTEXT ===");
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        Console.WriteLine("=== GOT USER MANAGER ===");
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        Console.WriteLine("=== GOT ROLE MANAGER ===");

        // Force create Identity tables only (keeping your working approach)
        Console.WriteLine("=== CREATING IDENTITY TABLES ===");
        try
        {
            await context.Database.ExecuteSqlRawAsync(@"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetRoles' AND xtype='U')
            CREATE TABLE [AspNetRoles] (
                [Id] nvarchar(450) NOT NULL,
                [Name] nvarchar(256) NULL,
                [NormalizedName] nvarchar(256) NULL,
                [ConcurrencyStamp] nvarchar(max) NULL,
                CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
            );
            
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUsers' AND xtype='U')
            CREATE TABLE [AspNetUsers] (
                [Id] nvarchar(450) NOT NULL,
                [UserName] nvarchar(256) NULL,
                [NormalizedUserName] nvarchar(256) NULL,
                [Email] nvarchar(256) NULL,
                [NormalizedEmail] nvarchar(256) NULL,
                [EmailConfirmed] bit NOT NULL,
                [PasswordHash] nvarchar(max) NULL,
                [SecurityStamp] nvarchar(max) NULL,
                [ConcurrencyStamp] nvarchar(max) NULL,
                [PhoneNumber] nvarchar(max) NULL,
                [PhoneNumberConfirmed] bit NOT NULL,
                [TwoFactorEnabled] bit NOT NULL,
                [LockoutEnd] datetimeoffset(7) NULL,
                [LockoutEnabled] bit NOT NULL,
                [AccessFailedCount] int NOT NULL,
                [FirstName] nvarchar(100) NULL,
                [LastName] nvarchar(100) NULL,
                [CreatedDate] datetime2 NOT NULL,
                [LastLoginDate] datetime2 NULL,
                [IsActive] bit NOT NULL,
                CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
            );
            
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUserRoles' AND xtype='U')
            CREATE TABLE [AspNetUserRoles] (
                [UserId] nvarchar(450) NOT NULL,
                [RoleId] nvarchar(450) NOT NULL,
                CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
                CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
                CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
            );
            
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUserClaims' AND xtype='U')
            CREATE TABLE [AspNetUserClaims] (
                [Id] int IDENTITY(1,1) NOT NULL,
                [UserId] nvarchar(450) NOT NULL,
                [ClaimType] nvarchar(max) NULL,
                [ClaimValue] nvarchar(max) NULL,
                CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
                CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
            );
            
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUserLogins' AND xtype='U')
            CREATE TABLE [AspNetUserLogins] (
                [LoginProvider] nvarchar(450) NOT NULL,
                [ProviderKey] nvarchar(450) NOT NULL,
                [ProviderDisplayName] nvarchar(max) NULL,
                [UserId] nvarchar(450) NOT NULL,
                CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
                CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
            );
            
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUserTokens' AND xtype='U')
            CREATE TABLE [AspNetUserTokens] (
                [UserId] nvarchar(450) NOT NULL,
                [LoginProvider] nvarchar(450) NOT NULL,
                [Name] nvarchar(450) NOT NULL,
                [Value] nvarchar(max) NULL,
                CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
                CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
            );
            
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetRoleClaims' AND xtype='U')
            CREATE TABLE [AspNetRoleClaims] (
                [Id] int IDENTITY(1,1) NOT NULL,
                [RoleId] nvarchar(450) NOT NULL,
                [ClaimType] nvarchar(max) NULL,
                [ClaimValue] nvarchar(max) NULL,
                CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
                CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
            );");
            Console.WriteLine("=== IDENTITY TABLES CREATED ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"=== TABLE CREATION WARNING: {ex.Message} (tables may already exist) ===");
        }

        Console.WriteLine("=== Starting database seeding ===");
        await SeedDataAsync(userManager, roleManager);
        Console.WriteLine("=== Seeding completed ===");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"=== SEEDING ERROR: {ex.Message} ===");
        Console.WriteLine($"=== STACK TRACE: {ex.StackTrace} ===");
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}
Console.WriteLine("=== AFTER SEEDING BLOCK ===");

// Method to seed initial data
static async Task SeedDataAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
{
    // Ensure roles exist
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    if (!await roleManager.RoleExistsAsync("Staff"))
    {
        await roleManager.CreateAsync(new IdentityRole("Staff"));
    }

    if (!await roleManager.RoleExistsAsync("Public"))
    {
        await roleManager.CreateAsync(new IdentityRole("Public"));
    }

    // Create default admin user if it doesn't exist
    var adminEmail = "admin@landertrailfoundation.org";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = "admin",
            Email = adminEmail,
            FirstName = "System",
            LastName = "Administrator",
            EmailConfirmed = true,
            IsActive = true,
            CreatedDate = DateTime.Now
        };

        var result = await userManager.CreateAsync(adminUser, "X");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            Console.WriteLine("=== ADMIN USER CREATED SUCCESSFULLY ===");
        }
        else
        {
            Console.WriteLine($"=== FAILED TO CREATE ADMIN USER: {string.Join(", ", result.Errors.Select(e => e.Description))} ===");
        }
    }
    else
    {
        Console.WriteLine("=== ADMIN USER ALREADY EXISTS ===");
    }
}
// Temporary debug endpoint
app.MapGet("/debug-user", async (UserManager<ApplicationUser> userManager) =>
{
    var user = await userManager.FindByNameAsync("admin");
    if (user == null)
    {
        return "User not found by username";
    }
    var roles = await userManager.GetRolesAsync(user);
    return $"User: {user.UserName}, Email: {user.Email}, IsActive: {user.IsActive}, EmailConfirmed: {user.EmailConfirmed}, Roles: {string.Join(",", roles)}";
});
app.Run();

