using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Builder;
using Project.Models;

namespace Project
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ================================================
            // 1. DATABASE CONFIGURATION (Current - Required)
            // ================================================
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // ================================================
            // 2. IDENTITY CONFIGURATION (Current + Future)
            // ================================================
            // Current: Basic Identity (as per project template)
            builder.Services.AddDefaultIdentity<AppUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;   // Change to true in production
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // FUTURE (uncomment after Authentication lectures):
            // builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => { ... })
            //     .AddEntityFrameworkStores<ApplicationDbContext>()
            //     .AddDefaultTokenProviders();

            // This enables [Authorize(Roles = "Admin")] for Admin Area
            // builder.Services.AddDefaultIdentity<ApplicationUser>()
            //     .AddRoles<IdentityRole>()           // ← Add this line when you create Admin role
            //     .AddEntityFrameworkStores<ApplicationDbContext>();

            // ================================================
            // 3. REPOSITORIES (Current - Required)
            // ================================================
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // ================================================
            // 4. MVC + AREAS (Current - Required)
            // ================================================
            builder.Services.AddControllersWithViews();

            // FUTURE: Add this if you want to use Areas routing explicitly
            // builder.Services.AddRazorPages(); // Only if you later add Razor Pages

            // ================================================
            // 5. CART STORAGE (Future - Choose one)
            // ================================================
            // Option A: Session-based cart (recommended for now - simple & matches project doc)
            builder.Services.AddDistributedMemoryCache();                    // Required for session
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Option B: DB-based cart (uncomment later when you create Cart tables)
            // builder.Services.AddScoped<ICartRepository, CartRepository>();

            // ================================================
            // 6. AUTHORIZATION POLICIES (Future - Uncomment after lectures)
            // ================================================
            // builder.Services.AddAuthorization(options =>
            // {
            //     options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            // });

            // ================================================
            // BUILD THE APP
            // ================================================
            var app = builder.Build();

            // ================================================
            // PIPELINE CONFIGURATION (Current)
            // ================================================
            if (app.Environment.IsDevelopment())
            {
                //app.UseMigrationsEndPoint();           // Shows detailed errors + migration page
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Enable authentication and authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession();   // ← Required for Session-based Cart (uncomment when you start CartController)

            // ================================================
            // ROUTING (Current + Future)
            // ================================================
            // Admin Area route (MUST come before default route!)
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            // Custom routes (uncomment when needed)
            app.MapControllerRoute(
                name: "catalog",
                pattern: "catalog/{action=Index}/{id?}",
                defaults: new { controller = "Catalog" });

            // Default route (MUST be last - least specific)
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // ================================================
            // SEEDING (Future - Uncomment after Identity lectures)
            // ================================================
            // using (var scope = app.Services.CreateScope())
            // {
            //     var services = scope.ServiceProvider;
            //     var context = services.GetRequiredService<ApplicationDbContext>();
            //     var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            //     var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            //
            //     await SeedData.Initialize(services);   // Create your own SeedData class later
            // }

            app.Run();
        }
    }
}
