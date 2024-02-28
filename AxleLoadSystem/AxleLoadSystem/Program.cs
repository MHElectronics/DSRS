using AxleLoadSystem.Client.Pages;
using AxleLoadSystem.Components;
using Services;
using Services.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddScoped<IFtpHelper, FtpHelper>();
builder.Services.AddScoped<ISqlDataAccess, SqlDataAccess>();
builder.Services.AddScoped<IALCSFilesService, ALCSFilesService>();
builder.Services.AddScoped<IUserService, UserService>();

//Localization
//builder.Services.AddLocalization(options => options.ResourcesPath = "Locales");
builder.Services.AddLocalization();
builder.Services.AddControllers();


var app = builder.Build();

//Localization
var cultures = builder.Configuration.GetSection("Cultures")
    .GetChildren().ToDictionary(x => x.Key, x => x.Value);
string[] supportedCultures = cultures.Keys.ToArray();
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("en-US")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizationOptions);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Counter).Assembly);

app.Run();
