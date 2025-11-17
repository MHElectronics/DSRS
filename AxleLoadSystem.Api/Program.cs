using DSRSystem.Api.Extensions;
using Microsoft.OpenApi.Models;
using NLog.Extensions.Logging;
using Services.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Configure NLog
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.SetMinimumLevel(LogLevel.Trace);
});

// Add NLog as the logger provider
builder.Services.AddSingleton<ILoggerProvider, NLogLoggerProvider>();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Axle Load API", Version = "v1" });
    //c.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme
    //{
    //    Type = SecuritySchemeType.Http,
    //    Scheme = "bearer",
    //    BearerFormat = "JWT",
    //    Description = "JWT Authorization header using the Bearer scheme."
    //});
    //c.AddSecurityRequirement(new OpenApiSecurityRequirement
    //{
    //    {
    //        new OpenApiSecurityScheme
    //        {
    //            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "bearerAuth" }
    //        },
    //        new string[] {}
    //    }
    //});
});

//Add service dependencies
builder.Services.AddServiceLayer();

//Caching
builder.Services.AddMemoryCache();
//builder.Services.AddHostedService<BackgroudCacheRefresh>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DefaultModelsExpandDepth(-1);
    });
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DefaultModelsExpandDepth(-1);
        options.SupportedSubmitMethods(new Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod[] { });
    });
}
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
