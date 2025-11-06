using Microsoft.Extensions.DependencyInjection;
using Services.Reports;

namespace Services.Helpers;
public static class DependencyInjection
{
    public static IServiceCollection AddServiceLayer(this IServiceCollection services)
    {
        //Helpers
        services.AddScoped<IFtpHelper, FtpHelper>();
        services.AddScoped<ICsvHelper, CsvHelper>();
        services.AddScoped<ISqlDataAccess, SqlDataAccess>();
        services.AddScoped<IFileStoreHelper, FileStoreHelper>();
        services.AddScoped<IExportHelper, ExportHelper>();
        services.AddScoped<ISQLQueriesService, SQLQueriesService>();
        //Services
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IStationService, StationService>();
        services.AddScoped<IWIMScaleService, WIMScaleService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAxleLoadService, AxleLoadService>();
        services.AddScoped<IFinePaymentService, FinePaymentService>();

        services.AddScoped<IFAQService, FAQService>();
        services.AddScoped<IStationAccessService, StationAccessService>();
        services.AddScoped<IClassStatusService, ClassStatusService>();
        services.AddScoped<ITutorialService, TutorialService>();
        services.AddScoped<IConfigurationService, ConfigurationService>();
        services.AddScoped<ICategoryService, TutorialCategoryService>();
        services.AddScoped<IUserActivityService, UserActivityService>();

        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IDCUService, DCUService>();
        services.AddScoped<IMeterService, MeterService>();

        //Reports
        services.AddScoped<IOverloadReportService, OverloadReportService>();

        return services;
    }
}
