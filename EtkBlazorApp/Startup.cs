using Blazored.Toast;
using EtkBlazorApp.BL;
using EtkBlazorApp.BL.Templates.PriceListTemplates;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.Integration.Ozon;
using EtkBlazorApp.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Globalization;

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
            //Blazor стандартные
            services.AddRazorPages();
            services.AddServerSideBlazor().AddCircuitOptions(options => { options.DetailedErrors = true; });

            //Blazor дополнительные
            services.AddHttpContextAccessor();

            //Приложение              
            services.AddTransient<IDatabaseProductCorrelator, HardOrdereSkuModelProductCorrelator>();
            services.AddTransient<IPriceLineLoadCorrelator, SimplePriceLineLoadCorrelator>();
            services.AddTransient<IOzonProductCorrelator, SimpleOzonProductCorrelator>();       
            services.AddTransient<IDatabaseAccess, EtkDatabaseDapperAccess>();
            services.AddTransient<IProductStorage, ProductStorage>();
            services.AddTransient<ITemplateStorage, TemplateStorage>();
            services.AddTransient<IOrderStorage, OrderStorage>();
            services.AddTransient<IManufacturerStorage, ManufacturerStorage>();
            services.AddTransient<ILogStorage, LogStorage>();
            services.AddTransient<ISettingStorage, SettingStorage>();
            services.AddTransient<IAuthenticationDataStorage, AuthenticationDataStorage>();
            services.AddTransient<RemoteTemplateFileLoaderFactory>();

            services.AddSingleton<ICurrencyChecker, CurrencyCheckerCbRf>();
            services.AddSingleton<SystemEventsLogger>();
            services.AddSingleton<NewOrdersNotificationService>();
            services.AddSingleton<UpdateManager>();
            services.AddSingleton<OzonSellerManager>();
            services.AddSingleton<PriceListManager>();
            services.AddSingleton<CronTaskService>();
            
            services.AddScoped<AuthenticationStateProvider, MyCustomAuthProvider>();
            services.AddScoped<UserLogger>();
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

            app.ApplicationServices.GetService<CronTaskService>();

            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("ru-RU");
        }
    }
}
