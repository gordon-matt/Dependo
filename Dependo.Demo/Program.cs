using Dependo.Autofac;
using Dependo.DryIoc;

namespace Dependo.Demo;

public class Program
{
    public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseIISIntegration();
                webBuilder.UseStartup<Startup>();
            })
            //.UseServiceProviderFactory(new DependableAutofacServiceProviderFactory());
            .UseServiceProviderFactory(new DependableDryIocServiceProviderFactory());
            //.UseServiceProviderFactory(new DependableLamarServiceProviderFactory());
}