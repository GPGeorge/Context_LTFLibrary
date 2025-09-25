using LTF_Library_V1.Data;
using LTF_Library_V1.Data.Models;
using LTF_Library_V1.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
//X
Console.WriteLine("=== PROGRAM.CS STARTING ===");
var builder = WebApplication.CreateBuilder(args);
//X
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

builder.Services.AddControllersWithViews();
builder.Services.AddSession();

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity Configuration (SINGLE configuration only)
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

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
bool isLocal = builder.Environment.IsDevelopment();

string basePath = isLocal ? "/" : "/LTFCatalog";
string loginPath = "/login";
string logoutPath =  "/logout";
string accessDeniedPath = "/access-denied";
// ============================
// End environment toggle
// ============================
//Debugging
Console.WriteLine($"=== PROGRAM.CS DEBUG ===");
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"IsDevelopment: {builder.Environment.IsDevelopment()}");
Console.WriteLine($"isLocal: {isLocal}");
Console.WriteLine($"loginPath: '{loginPath}'");
Console.WriteLine($"basePath: '{basePath}'");
Console.WriteLine($"========================");

// Configure Cookie Authentication
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie(options =>
//    {
//        options.Cookie.Name = "LTFCatalog.Auth";
     
//        options.LoginPath = loginPath;
//        options.LogoutPath = logoutPath;
//        options.AccessDeniedPath = accessDeniedPath;
//        options.Cookie.HttpOnly = true;

//        // This adapts automatically:
//        // - On localhost (HTTP): allows non-secure cookies
//        // - On production (HTTPS): marks cookie as Secure
//        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
//        options.ExpireTimeSpan = TimeSpan.FromDays(30);
//        options.SlidingExpiration = true;


//        // Safe default that works in both cases
//        // - Localhost: accepted by Chrome/Edge/Brave
//        // - Production: behaves like normal auth cookies
//        options.Cookie.SecurePolicy = isLocal
//        ? CookieSecurePolicy.None       // allow HTTP on localhost
//        : CookieSecurePolicy.Always;    // require HTTPS in production
//        options.Cookie.SameSite = isLocal
//            ? SameSiteMode.Lax               // relaxed on localhost
//            : SameSiteMode.Lax;              // standard in production
//                                             // Optional: custom redirect logging
//        options.Events.OnRedirectToLogin = context =>
//        {
//            Console.WriteLine($"Redirect to login: {context.RedirectUri}");
//            context.Response.Redirect(context.RedirectUri);
//            return Task.CompletedTask;
//        };


//    });





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
    Console.WriteLine("=== APPLYING PATHBASE /LTFCatalog ===");
    app.UsePathBase("/LTFCatalog");
}
else
{
    Console.WriteLine("=== NOT APPLYING PATHBASE (localhost) ===");
}

app.UseRouting();
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
        //User Password sanitized for Public repo
        var result = await userManager.CreateAsync(adminUser, "XXXXXX!");

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

