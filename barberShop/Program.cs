using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using barberShop;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null)));

builder.Services.AddIdentity<Felhasznalo, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
});

builder.Services.Configure<EmailBeallitasok>(
    builder.Configuration.GetSection("Email"));

builder.Services.AddScoped<IEmailKuldo, SmtpEmailKuldo>();

builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();

        await context.Database.MigrateAsync();

        SeedAdatok.Initialize(context);

        var userManager = services.GetRequiredService<UserManager<Felhasznalo>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        const string adminRoleName = "Admin";

        if (!await roleManager.RoleExistsAsync(adminRoleName))
        {
            await roleManager.CreateAsync(new IdentityRole(adminRoleName));
        }

        const string fodraszRole = "Fodrasz";
        if (!await roleManager.RoleExistsAsync(fodraszRole))
        {
            await roleManager.CreateAsync(new IdentityRole(fodraszRole));
        }

        const string felhasznaloRole = "Mugli";
        if (!await roleManager.RoleExistsAsync(felhasznaloRole))
        {
            await roleManager.CreateAsync(new IdentityRole(felhasznaloRole));
        }

        var adminEmail = "kerberosz@kerberosz.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new Felhasznalo
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(adminUser, "%20kerberosz02%");

            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, adminRoleName);
            }
            else
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError("Nem sikerült az admin felhasználó létrehozása: {Errors}",
                    string.Join("; ", createResult.Errors.Select(e => e.Description)));
            }
        }

        var fodraszEmail = "szaszak@gmail.com";
        var fodraszUser = await userManager.FindByEmailAsync(fodraszEmail);
        if (fodraszUser == null)
        {

        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Hiba Seed adatok inicializálásánál!");
    }
}

app.Run();