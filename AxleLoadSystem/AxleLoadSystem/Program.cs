using AxleLoadSystem.Authentication;
using AxleLoadSystem.Client.Pages;
using AxleLoadSystem.Components;
using AxleLoadSystem.Helpers;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Services.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

//Local storage
builder.Services.AddBlazoredLocalStorage();

//Add local dependencies
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddAuthentication()
    .AddScheme<AuthenticationSchemeOptions, CustomAuthenticationHandler>("CustomSchemeName", options => { });
builder.Services.AddScoped<IRHDApiHelper, RHDApiHelper>();
//Add service dependencies
builder.Services.AddServiceLayer();

//Caching
builder.Services.AddMemoryCache();

//Localization
builder.Services.AddLocalization();
builder.Services.AddControllers();

//HTTP Client
builder.Services.AddHttpClient("BRTA_API", client => client.BaseAddress = new Uri(builder.Configuration["BRTA_API"]));

builder.Services.AddCascadingAuthenticationState();

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
