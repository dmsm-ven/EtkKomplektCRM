using Blazored.Toast;
using EtkBlazorApp.Integration.Ozon;
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
using EtkBlazorApp.BL.Interfaces;
using EtkBlazorApp.BL.Correlators;

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
            services.AddServerSideBlazor().AddCircuitOptions(options => { options.DetailedErrors = true; });

            //Blazor additional
            services.AddHttpContextAccessor();

            //Мои              
            services.AddSingleton<IDatabaseProductCorrelator, SimpleDatabaseProductCorrelator>();
            services.AddSingleton<IPriceLineLoadCorrelator, SimplePriceLineLoadCorrelator>();
            services.AddSingleton<ICurrencyChecker, CurrencyCheckerCbRf>();

            services.AddSingleton<IDatabaseAccess, EtkDatabaseDapperAccess>();
            services.AddSingleton<IProductStorage, ProductStorage>();
            services.AddSingleton<IPriceListTemplateStorage, PriceListTemplateStorage>();
            services.AddSingleton<IOrderStorage, OrderStorage>();
            services.AddSingleton<IManufacturerStorage, ManufacturerStorage>();
            services.AddSingleton<ILogStorage, LogStorage>();
            services.AddSingleton<ISettingStorage, SettingStorage>();
            services.AddSingleton<IAuthStateProcessor, MyAuthStateProcessor>();
            
            services.AddSingleton<NewOrdersNotificationService>();
            services.AddSingleton<UpdateManager>();
            services.AddSingleton<OzonSellerApi>();
            services.AddSingleton<PriceListManager>();
            services.AddSingleton<ScheduleTaskManager>();

            services.AddScoped<MyDbLogger>();                 
            services.AddScoped<AuthenticationStateProvider, MyCustomAuthProvider>();
            services.AddScoped<ReportManager>();      

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

            app.ApplicationServices.GetService<ScheduleTaskManager>();
        }
    }
}
