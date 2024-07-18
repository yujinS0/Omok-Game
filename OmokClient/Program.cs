using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using OmokClient;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddMudServices();
builder.Services.AddAntDesign();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazoredSessionStorage();

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient ���
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5284") });

// ���� API �ּҸ� ���� HttpClient ���
builder.Services.AddHttpClient("GameAPI", client =>
{
    client.BaseAddress = new Uri("http://localhost:5105");
});

// CustomAuthenticationStateProvider ���
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<CustomAuthenticationStateProvider>());

// ���� �� ���� �ο� ���� ���
builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();