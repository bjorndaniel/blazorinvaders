using BlazorInvaders.Api;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<HighScoreStore>();
    })
    .Build();

host.Run();
