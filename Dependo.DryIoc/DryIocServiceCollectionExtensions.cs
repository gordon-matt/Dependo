//using DryIoc;
//using Microsoft.Extensions.DependencyInjection;

//namespace Dependo.DryIoc;

///// <summary>
///// Extension methods for adding DryIoc to IServiceCollection
///// </summary>
//public static class DryIocServiceCollectionExtensions
//{
//    /// <summary>
//    /// Adds Dependo DryIoc to the service collection
//    /// </summary>
//    /// <param name="services">Service collection</param>
//    /// <returns>Service collection</returns>
//    public static IServiceCollection AddDependoDryIoc(this IServiceCollection services) => services.AddSingleton<IEngine, DryIocEngine>();

//    /// <summary>
//    /// Creates a Dependo DryIoc service provider factory
//    /// </summary>
//    /// <param name="services">Service collection</param>
//    /// <param name="configureContainer">Action to configure the container</param>
//    /// <returns>Service collection</returns>
//    public static IServiceCollection AddDependoDryIoc(
//        this IServiceCollection services,
//        Action<IContainer>? configureContainer = null) => services.AddDependoDryIoc()
//            .AddSingleton<IServiceProviderFactory<IContainer>>(
//                new DependableDryIocServiceProviderFactory(configureContainer));
//}