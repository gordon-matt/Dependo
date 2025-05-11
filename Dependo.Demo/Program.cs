using Autofac;
using Dependo.Autofac;
using Dependo.DryIoc;
using Dependo.Lamar;
using Dependo.LightInject;
using DryIoc;
using Lamar;
using LightInject;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddRouting(routeOptions =>
{
    routeOptions.AppendTrailingSlash = true;
    routeOptions.LowercaseUrls = true;
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

// Configure LightInject
builder.Host
    // Configure Dependo for whichever container you want to use
    .UseServiceProviderFactory(new DependoAutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>(container =>
    {
        // Add your Autofac-specific registrations here or use an IAutofacDependencyRegistrar
    });
    //.UseServiceProviderFactory(new DependoDryIocServiceProviderFactory())
    //.ConfigureContainer<DryIoc.Container>(container =>
    // {
    //     // Add your DryIoc-specific registrations here or use an IDryIocDependencyRegistrar
    // });
    //.UseServiceProviderFactory(new DependoLamarServiceProviderFactory())
    //.ConfigureContainer<ServiceRegistry>(container =>
    // {
    //     // Add your Lamar-specific registrations here or use an ILamarDependencyRegistrar
    // });
    //.UseServiceProviderFactory(new DependoLightInjectServiceProviderFactory())
    //.ConfigureContainer<ServiceContainer>(container =>
    //{
    //    // Add your LightInject-specific registrations here or use an ILightInjectDependencyRegistrar
    //});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areaRoute",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();