//using Microsoft.Extensions.DependencyInjection;

//namespace Dependo.LightInject;

///// <summary>
///// Extension methods for adding LightInject to IServiceCollection
///// </summary>
//public static class LightInjectServiceCollectionExtensions
//{
//    /// <summary>
//    /// Adds Dependo LightInject to the service collection
//    /// </summary>
//    /// <param name="services">Service collection</param>
//    /// <returns>Service collection</returns>
//    public static IServiceCollection AddDependoLightInject(this IServiceCollection services) =>
//        services.AddSingleton<IDependoContainer, LightInjectDependoContainer>();

//    /// <summary>
//    /// Creates a Dependo LightInject service provider factory
//    /// </summary>
//    /// <param name="services">Service collection</param>
//    /// <param name="configureContainer">Action to configure the container</param>
//    /// <returns>Service collection</returns>
//    public static IServiceCollection AddDependoLightInject(
//        this IServiceCollection services,
//        Action<IServiceContainer>? configureContainer = null) =>
//        services.AddDependoLightInject()
//            .AddSingleton<IServiceProviderFactory<IServiceContainer>>(
//                new DependableLightInjectServiceProviderFactory(configureContainer));
//} 