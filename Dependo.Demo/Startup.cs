using Autofac;
using DryIoc;
using LightInject;

namespace Dependo.Demo;

public class Startup
{
    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddRouting((routeOptions) =>
        {
            routeOptions.AppendTrailingSlash = true;
            routeOptions.LowercaseUrls = true;
        });

        services.AddControllersWithViews();
        services.AddRazorPages();
        services.AddHttpContextAccessor();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            //app.UseBrowserLink();
        }
        else
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseStaticFiles();
        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "areaRoute",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            endpoints.MapRazorPages();
        });
    }

    //// For Autofac
    //public void ConfigureContainer(ContainerBuilder builder)
    //{
    //    // Add extra registrations here, if needed...
    //    //  But it's better to use IDependencyRegistrar
    //}

    //// For DryIoC
    //public void ConfigureContainer(Container builder)
    //{
    //    // Add extra registrations here, if needed...
    //    //  But it's better to use IDependencyRegistrar
    //}

    //// For Lamar
    //public void ConfigureContainer(ServiceRegistry builder)
    //{
    //    // Add extra registrations here, if needed...
    //    //  But it's better to use IDependencyRegistrar
    //}

    // For LightInject
    public void ConfigureContainer(ServiceContainer builder)
    {
        // Add extra registrations here, if needed...
        //  But it's better to use IDependencyRegistrar
    }
}