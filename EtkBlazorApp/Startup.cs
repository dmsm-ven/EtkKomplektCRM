using Blazored.Toast;
using EtkBlazorApp.BL;
using EtkBlazorApp.BL.Data;
using EtkBlazorApp.BL.Loggers;
using EtkBlazorApp.BL.Managers;
using EtkBlazorApp.BL.Notifiers;
using EtkBlazorApp.BL.Templates.PriceListTemplates.RemoteFileLoaders;
using EtkBlazorApp.CdekApi;
using EtkBlazorApp.Core.Data;
using EtkBlazorApp.Core.Interfaces;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Repositories;
using EtkBlazorApp.DataAccess.Repositories.PriceList;
using EtkBlazorApp.DataAccess.Repositories.Product;
using EtkBlazorApp.DataAccess.Repositories.Wildberries;
using EtkBlazorApp.DellinApi;
using EtkBlazorApp.Model.Chart;
using EtkBlazorApp.Model.IOptionProfiles;
using EtkBlazorApp.Services;
using EtkBlazorApp.Services.CurrencyChecker;
using EtkBlazorApp.TelegramBotLib;
using EtkBlazorApp.WildberriesApi;
using EtkBlazorAppi.DadataApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using System;
using System.Globalization;
using System.Net;
using System.Net.Http;

namespace EtkBlazorApp;

public class Startup
{
    private static readonly Logger nlog = LogManager.GetCurrentClassLogger();

    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime hostApplicationLifetime)
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

        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("ru-RU");

        var sysLogger = app.ApplicationServices.GetRequiredService<SystemEventsLogger>();

        hostApplicationLifetime.ApplicationStarted.Register(async () =>
        {
            app.ApplicationServices.GetRequiredService<CronTaskService>().Start();
            app.ApplicationServices.GetRequiredService<NewOrdersNotificationService>().Start();
            app.ApplicationServices.GetRequiredService<EmailPriceListCheckingService>().Start();

            await sysLogger.WriteSystemEvent(LogEntryGroupName.Auth, "Запуск", "Запуск приложения личного кабинета");
        });
        hostApplicationLifetime.ApplicationStopping.Register(async () =>
        {
            await sysLogger.WriteSystemEvent(LogEntryGroupName.Auth, "Остановка", "Остановка приложения личного кабинета");
        });
    }

    public void ConfigureServices(IServiceCollection services)
    {

        //Сторонние
        services.AddBlazoredToast();
        services.AddAutoMapper(this.GetType().Assembly);
        services.AddMemoryCache();
        services.AddHttpClient();
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
        services.AddSingleton<EncryptHelper>((x) => new EncryptHelper(Configuration["EncryptHelper:Phrase"]));
        services.AddTransient<IUserInfoChecker, UserInfoChecker>();
        services.AddTransient<ICompressedFileExtractor, SharpCompressFileExtractor>();

        ConfigureCorrelators(services);
        ConfigureDatabaseServices(services);
        ConfigureExteralApiClients(services);
        ConfigureNotifiers(services);
        ConfigureOptions(services);

        services.AddSingleton<ICurrencyChecker, CurrencyCheckerCbRf_V2>();
        services.AddSingleton<EmailAttachmentExtractorInitializer>();
        services.AddSingleton<CashPlusPlusLinkGenerator>();
        services.AddSingleton<RemoteTemplateFileLoaderFactory>();
        services.AddSingleton<SystemEventsLogger>();
        services.AddSingleton<NewOrdersNotificationService>();
        services.AddSingleton<PriceListPriceHistoryManager>();
        services.AddSingleton<ProductsPriceAndStockUpdateManager>();
        services.AddSingleton<PriceListManager>();
        services.AddSingleton<CronTaskService>();
        services.AddSingleton<EmailPriceListCheckingService>();

        services.AddTransient<IUserService, BCryptUserService>();
        services.AddScoped<AuthenticationStateProvider, CustomAuthProvider>();
        services.AddScoped<UserLogger>();
        services.AddScoped<ReportManager>();
        services.AddScoped<ChartDataExtractor>();

        services.AddHostedService<WildberriesUpdateService>();
    }

    private void ConfigureOptions(IServiceCollection services)
    {
        services.Configure<Integration1C_Configuration>(Configuration.GetSection(nameof(Integration1C_Configuration)));
        services.Configure<CurrencyUpdaterEndpointOptions>(Configuration.GetSection(nameof(CurrencyUpdaterEndpointOptions)));
    }

    private void ConfigureNotifiers(IServiceCollection services)
    {
        services.AddTransient<ICustomerOrderNotificator, MailkitOrderEmailNotificator>();

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

        services.AddHttpClient<WildberriesApiClient>(x =>
        {
            x.DefaultRequestHeaders.Add("Accept", "application/json");
        });
        services.AddTransient<WildberriesApiClient>();
    }

    private void ConfigureDatabaseServices(IServiceCollection services)
    {
        services.AddTransient<IDatabaseAccess, EtkDatabaseDapperAccess>();
        services.AddTransient<IWildberriesProductRepository, WildberriesProductRepository>();
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

