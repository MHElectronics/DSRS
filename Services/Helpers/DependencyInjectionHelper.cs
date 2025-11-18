using Microsoft.Extensions.DependencyInjection;

namespace Services.Helpers;
public static class DependencyInjection
{
    public static IServiceCollection AddServiceLayer(this IServiceCollection services)
    {
        //Helpers
        services.AddScoped<ISqlDataAccess, SqlDataAccess>();
        //Services
        services.AddScoped<IUserService, UserService>();

        services.AddScoped<IFAQService, FAQService>();
        services.AddScoped<ITutorialService, TutorialService>();
        services.AddScoped<ICategoryService, TutorialCategoryService>();
        services.AddScoped<IUserActivityService, UserActivityService>();
        //
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IDCUService, DCUService>();
        services.AddScoped<IMeterService, MeterService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ITransactionService, TransactionService>();

        return services;
    }
}
