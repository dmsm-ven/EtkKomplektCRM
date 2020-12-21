using Blazored.Toast;
using EtkBlazorApp.API.Ozon;
using EtkBlazorApp.BL;
using EtkBlazorApp.BL.Managers;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EtkBlazorApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            //Blazor default
            services.AddRazorPages();
            services.AddServerSideBlazor();

            //Мои              
            services.AddSingleton<IDatabaseProductCorrelator, SimpleDatabaseProductCorrelator>();
            services.AddSingleton<IPriceLineLoadCorrelator, SimplePriceLineLoadCorrelator>();
            services.AddSingleton<ICurrencyChecker, CurrencyCheckerCbRf>();

            services.AddSingleton<IDatabase, DapperMySql>();
            services.AddSingleton<DatabaseManager>();

            services.AddSingleton<PriceListManager>();
            services.AddSingleton<ReportManager>();          
            services.AddSingleton<NewOrdersNotificationService>();
            services.AddScoped<AuthenticationStateProvider, MyCustomAuthProvider>();
            services.AddSingleton<OzonSellerApi>();
            

            //Сторонние
            services.AddBlazoredToast();

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
