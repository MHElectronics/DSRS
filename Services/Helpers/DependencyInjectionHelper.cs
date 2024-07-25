using Microsoft.Extensions.DependencyInjection;

namespace Services.Helpers;
public static class DependencyInjection
{
    public static IServiceCollection AddServiceLayer(this IServiceCollection services)
    {
        //Helpers
        services.AddScoped<IFtpHelper, FtpHelper>();
        services.AddScoped<ICsvHelper, CsvHelper>();
        services.AddScoped<ISqlDataAccess, SqlDataAccess>();
        
        //Services
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IStationService, StationService>();
        services.AddScoped<IWIMScaleService, WIMScaleService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAxleLoadService, AxleLoadService>();
        services.AddScoped<IFinePaymentService, FinePaymentService>();

        services.AddScoped<IFAQService, FAQService>();

        return services;
    }
}
