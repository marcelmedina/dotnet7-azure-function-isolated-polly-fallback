using System.Net;
using consumer.TypedHttpClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        var currentDirectory = hostingContext.HostingEnvironment.ContentRootPath;

        config.SetBasePath(currentDirectory)
            .AddJsonFile("settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
        config.Build();
    })
    .ConfigureServices((services) =>
    {
        var fallbackPolicy = Policy
            .HandleResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode)
            .FallbackAsync(_ =>
            {
                Console.Out.WriteLine("### Fallback executed");
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.FailedDependency));
            });

        services.AddHttpClient<StateCounterHttpClient>()
            .AddPolicyHandler(fallbackPolicy);
    })
    .Build();

host.Run();
