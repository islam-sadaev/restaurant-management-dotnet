using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Models;
using AutoMapper;
using Restaurant.Data.UnitOfWork;
using Restaurant.Configuration.MailService;
using Restaurant.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Array met alle gewenste rollen (Idempotente Rol Seeding)
string[] desiredRoles = new[] {
    "Eigenaar",
    "Klant",
    "Ober",
    "Kok",
    "Zaalverantwoordelijke"

};

// ====================================================================
// CONFIGURATIE VAN SERVICES
// ====================================================================

// Registreert de DbContext en koppelt deze aan de verbindingsstring uit appsettings.json.
// Dit is de standaard methode om de database te verbinden in .NET Core.
builder.Services.AddDbContext<RestaurantContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LocalDBConnection")));

// Registreert services voor Session management (H11)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Stelt de timeout van de sessie in
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Registratie van andere services
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddControllersWithViews();
builder.Services.AddHostedService<ReservationMailScheduler>();

// IDENTITY SETUP (H6)
// Registreert de Identity-services met CustomUser (ons model) en IdentityRole (standaard rollen).
builder.Services.AddIdentity<CustomUser, IdentityRole>()
    .AddEntityFrameworkStores<RestaurantContext>() // Koppelt Identity aan onze DbContext
    .AddDefaultTokenProviders();

// Identity Configuratie (Stelt eisen aan wachtwoorden en lockout, H6)
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
});

// Cookie paden voor Identity (H11)
// Bepaalt waar de gebruiker naartoe gestuurd wordt bij inloggen of weigering.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Gebruiker/Login";
    options.AccessDeniedPath = "/Home/Index";
});

// ANDERE BELANGRIJKE SERVICES
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>(); // Registreert het UnitOfWork patroon (H4)
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies()); // Registreert alle AutoMapper Profiles (H5)

// ====================================================================
// APP AANMAKEN EN MIDDLEWARE 
// ====================================================================

var app = builder.Build();
app.UseSession(); // Activeert de sessie middleware

// VEILIGE ROL SEEDING (Idempotent: Maakt rollen aan bij startup als ze ontbreken, H7)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    // Zorgt ervoor dat alle rollen die nodig zijn, bestaan in de database.
    // Dit garandeert dat Rollen (Eigenaar, Klant, etc.) beschikbaar zijn voor toewijzing.
    foreach (var roleName in desiredRoles)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}
// EINDE ROL SEEDING

// Configuratie voor foutafhandeling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication(); // Authenticatie (Wie ben je? - H6)
app.UseAuthorization();  // Authorisatie (Wat mag je? - H7)

// Standaard routering (H9)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();