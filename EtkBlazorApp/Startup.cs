using Blazored.Toast;
using EtkBlazorApp.BL;
using EtkBlazorApp.BL.Templates.PriceListTemplates;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Globalization;
using System.Net;

namespace EtkBlazorApp
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
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
            app.ApplicationServices.GetService<NewOrdersNotificationService>().RefreshInterval = TimeSpan.FromSeconds(5);

            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("ru-RU");
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = (int)HttpStatusCode.TemporaryRedirect;
                options.HttpsPort = 5001;
            });

            //Blazor �����������
            services.AddRazorPages();
            services.AddServerSideBlazor().AddCircuitOptions(options => { options.DetailedErrors = true; });           
            services.AddHttpContextAccessor();
            services.AddScoped<ProtectedLocalStorage>();

            //����������       
            services.AddTransient<IUserInfoChecker, UserInfoChecker>();
            services.AddTransient<ICompressedFileExtractor, SharpCompressFileExtractor>();
            ConfigureCorrelators(services);
            ConfigureDatabaseServices(services);

            services.AddMemoryCache();
            services.AddSingleton<ICurrencyChecker, CurrencyCheckerCbRf>();
            services.AddSingleton<RemoteTemplateFileLoaderFactory>();        
            services.AddSingleton<SystemEventsLogger>();
            services.AddSingleton<NewOrdersNotificationService>();
            services.AddSingleton<UpdateManager>();
            services.AddSingleton<PriceListManager>();
            services.AddSingleton<CronTaskService>();

            services.AddTransient<IUserService, BCryptUserService>();
            services.AddScoped<AuthenticationStateProvider, CustomAuthProvider>();
            services.AddScoped<UserLogger>();
            services.AddScoped<ReportManager>();
            

            //���������
            services.AddBlazoredToast();
        }

        private void ConfigureDatabaseServices(IServiceCollection services)
        {
            services.AddTransient<IDatabaseAccess, EtkDatabaseDapperAccess>();
            services.AddTransient<IPartnersInformationService, PartnersInformationService>();
            services.AddTransient<IProductDiscountStorage, ProductDiscountStorage>();
            services.AddTransient<IMarketplaceExportService, MarketplaceExportService>();
            services.AddTransient<IProductStorage, ProductStorage>();
            services.AddTransient<ICategoryStorage, CategoryStorage>();
            services.AddTransient<IProductUpdateService, ProductUpdateService>();
            services.AddTransient<IPriceListTemplateStorage, PriceListTemplateStorage>();
            services.AddTransient<IPrikatTemplateStorage, PrikatTemplateStorage>();
            services.AddTransient<IOrderStorage, OrderStorage>();
            services.AddTransient<IManufacturerStorage, ManufacturerStorage>();
            services.AddTransient<IStockStorage, StockStorage>();
            services.AddTransient<IMonobrandStorage, MonobrandStorage>();
            services.AddTransient<ILogStorage, LogStorage>();
            services.AddTransient<ISettingStorage, SettingStorage>();
            services.AddTransient<ICronTaskStorage, CronTaskStorage>();
            
            services.AddTransient<IWebsiteCurrencyService, WebsiteCurrencyService>();
        }

        private void ConfigureCorrelators(IServiceCollection services)
        {
            //��������� �������� ��� ������� Elevel ��� ~ 125000 �������
            //services.AddTransient<IDatabaseProductCorrelator, FullCompareProductCorrelator>();
            services.AddTransient<IDatabaseProductCorrelator, DictionaryCompareProductCorrelator>();

            services.AddTransient<IPriceLineLoadCorrelator, SimplePriceLineLoadCorrelator>();
            services.AddTransient<IOzonProductCorrelator, SimpleOzonProductCorrelator>();
        }       
    }
}
