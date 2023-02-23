using Blazored.Toast;
using EtkBlazorApp.BL;
using EtkBlazorApp.BL.Managers;
using EtkBlazorApp.BL.Templates.PriceListTemplates;
using EtkBlazorApp.CdekApi;
using EtkBlazorApp.Core.Interfaces;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Repositories;
using EtkBlazorApp.DellinApi;
using EtkBlazorApp.Model.Chart;
using EtkBlazorApp.Services;
using EtkBlazorApp.TelegramBotLib;
using EtkBlazorAppi.DadataApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Globalization;
using System.Net;
using System.Net.Http;

namespace EtkBlazorApp;

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

            endpoints.MapControllers();
        });

        app.ApplicationServices.GetService<CronTaskService>();
        app.ApplicationServices.GetService<NewOrdersNotificationService>().RefreshInterval = TimeSpan.FromSeconds(5);

        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("ru-RU");
    }

    public void ConfigureServices(IServiceCollection services)
    {
        //Сторонние
        services.AddBlazoredToast();
        services.AddAutoMapper(this.GetType().Assembly);
        services.AddMemoryCache();
        services.AddHttpsRedirection(options =>
        {
            options.RedirectStatusCode = (int)HttpStatusCode.TemporaryRedirect;
            options.HttpsPort = 5001;
        });

        //Blazor стандартные
        services.AddRazorPages();
        services.AddServerSideBlazor().AddCircuitOptions(options => { options.DetailedErrors = true; });
        services.AddHttpContextAccessor();
        services.AddScoped<ProtectedLocalStorage>();

        //Приложение       
        services.AddTransient<IUserInfoChecker, UserInfoChecker>();
        services.AddTransient<ICompressedFileExtractor, SharpCompressFileExtractor>();


        ConfigureCorrelators(services);
        ConfigureDatabaseServices(services);
        ConfigureExteralApiClients(services);
        ConfigureNotifiers(services);

        services.AddSingleton<ICurrencyChecker, CurrencyCheckerCbRf>();
        services.AddSingleton<CashPlusPlusLinkGenerator>();
        services.AddSingleton<RemoteTemplateFileLoaderFactory>();
        services.AddSingleton<SystemEventsLogger>();
        services.AddSingleton<NewOrdersNotificationService>();
        services.AddSingleton<PriceListPriceHistoryManager>();
        services.AddSingleton<ProductsPriceAndStockUpdateManager>();
        services.AddSingleton<PriceListManager>();
        services.AddSingleton<CronTaskService>();

        services.AddTransient<IUserService, BCryptUserService>();
        services.AddScoped<AuthenticationStateProvider, CustomAuthProvider>();
        services.AddScoped<UserLogger>();
        services.AddScoped<ReportManager>();
        services.AddScoped<ChartDataExtractor>();
    }

    private void ConfigureNotifiers(IServiceCollection services)
    {
        services.AddSingleton<IEtkUpdatesNotifierMessageFormatter, TelegramNotifierMessageFormatter>();

        services.AddSingleton<IEtkUpdatesNotifier, EtkTelegramBotNotifier>((x) =>
        {
            var section = Configuration.GetSection("TelegramBotNotifierConfiguration");
            IEtkUpdatesNotifierMessageFormatter formatter = x.GetService<IEtkUpdatesNotifierMessageFormatter>();
            ISettingStorageReader settings = x.GetService<ISettingStorageReader>();
            return new EtkTelegramBotNotifier(formatter, settings, section["Token"], long.Parse(section["ChannelId"]));
        });
    }

    private void ConfigureExteralApiClients(IServiceCollection services)
    {
        services.AddSingleton<ICompanyInfoChecker>(x =>
        {
            var section = Configuration.GetSection("DadataConfiguration");
            return new DadataApiClient(section["Token"]);
        });

        services.AddSingleton<ITransportCompanyApi, CdekApiMemoryCachedClient>(x =>
        {
            var section = Configuration.GetSection("DeliveryCompanies:CdekConfiguration");
            var client = new HttpClient()
            {
                BaseAddress = new Uri("https://api.cdek.ru")
            };
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
            IMemoryCache cache = services.BuildServiceProvider().GetRequiredService<IMemoryCache>();


            return new CdekApiMemoryCachedClient(section["Account"], section["SecurePassword"], cache, client);
        });

        services.AddSingleton<ITransportCompanyApi, DellinApiClient>(x =>
        {
            var section = Configuration.GetSection("DeliveryCompanies:DellinConfiguration");
            var client = new HttpClient()
            {
                BaseAddress = new Uri("https://api.dellin.ru")
            };
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");

            return new DellinApiClient(section["API_KEY"], client);
        });

        services.AddSingleton<DeliveryServiceApiManager>();
    }

    private void ConfigureDatabaseServices(IServiceCollection services)
    {
        services.AddTransient<IDatabaseAccess, EtkDatabaseDapperAccess>();
        services.AddTransient<IPriceListUpdateHistoryRepository, PriceListUpdateHistoryRepository>();
        services.AddTransient<IPartnersInformationService, PartnersInformationService>();
        services.AddTransient<IProductDiscountStorage, ProductDiscountStorage>();
        services.AddTransient<IMarketplaceExportService, MarketplaceExportService>();
        services.AddTransient<IProductStorage, ProductStorage>();
        services.AddTransient<ICategoryStorage, CategoryStorage>();
        services.AddTransient<IProductUpdateService, ProductUpdateService>();
        services.AddTransient<IPriceListTemplateAdditionalTabsStorage, PriceListTemplateAdditionalTabsStorage>();
        services.AddTransient<IPriceListTemplateStorage, PriceListTemplateStorage>();
        services.AddTransient<IPrikatTemplateStorage, PrikatTemplateStorage>();
        services.AddTransient<IOrderStorage, OrderStorage>();
        services.AddTransient<IOrderUpdateService, OrderUpdateService>();
        services.AddTransient<IManufacturerStorage, ManufacturerStorage>();
        services.AddTransient<IStockStorage, StockStorage>();
        services.AddTransient<IMonobrandStorage, MonobrandStorage>();
        services.AddTransient<ILogStorage, LogStorage>();
        services.AddTransient<ISettingStorageReader, SettingStorageReader>();
        services.AddTransient<ISettingStorageWriter, SettingStorageWriter>();
        services.AddTransient<ICronTaskStorage, CronTaskStorage>();
        services.AddTransient<IWebsiteCurrencyService, WebsiteCurrencyService>();
    }

    private void ConfigureCorrelators(IServiceCollection services)
    {
        services.AddTransient<IDatabaseProductCorrelator, DictionaryCompareProductCorrelator>();
        services.AddTransient<IPriceLineLoadCorrelator, SimplePriceLineLoadCorrelator>();
        services.AddTransient<IOzonProductCorrelator, SimpleOzonProductCorrelator>();
    }
}

