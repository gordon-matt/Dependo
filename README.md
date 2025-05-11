# Dependo

## Info

Dependo is a library that let's you resolve dependencies from anywhere via a singleton call to `DependoResolver.Instance`. This is mostly useful for resolving dependencies in static classes, including extensions methods.

Additionally, there is an `IDependencyRegistrar` interface to let you register dependencies from anywhere, such as class libraries. If you need to take advantage of implementation-specific registration features, you can make use of the more specialized dependency registrar, such as `IAutofacDependencyRegistrar`, `IDryIocDependencyRegistrar`, etc.

Check the demo project for a clearer idea on how to use Dependo.

## NuGet Packages
> Dependo: https://www.nuget.org/packages/Dependo/

> Dependo.Autofac: https://www.nuget.org/packages/Dependo.Autofac/

> Dependo.DotNetDefault: coming soon.

> Dependo.DryIoc: coming soon.

> Dependo.Lamar: coming soon.

> Dependo.LightInject: coming soon.

## Feature Support

The default .NET implementation suits most needs. You could start with that and then if more advanced features are required, simply swap out that implementation for another. Use the following table to help decide which is most suitable for your project.

| Method                                                             | Default .NET DI | Autofac | DryIoc | Lamar | LightInject |
|--------------------------------------------------------------------|------------------|---------|--------|--------|--------------|
| `Resolve<T>()`                                                     | ✅               | ✅      | ✅     | ✅     | ✅           |
| `Resolve<T>(IDictionary<string, object> ctorArgs)`                 | ❌               | ✅      | ✅     | ❌     | ❌           |
| `object Resolve(Type type)`                                       | ✅               | ✅      | ✅     | ✅     | ✅           |
| `T ResolveNamed<T>(string name)`                                   | ❌               | ✅      | ✅     | ✅     | ✅           |
| `IEnumerable<T> ResolveAll<T>()`                                   | ✅               | ✅      | ✅     | ✅     | ✅           |
| `IEnumerable<T> ResolveAllNamed<T>`                                | ❌               | ✅      | ❌     | ❌     | ❌           |
| `object ResolveUnregistered(Type type)`                            | ❌               | ✅      | ❌     | ✅     | ❌           |
| `bool TryResolve<T>(out T? instance)`                              | ✅               | ✅      | ✅     | ✅     | ✅           |
| `bool TryResolve(Type serviceType, out object? instance)`          | ✅               | ✅      | ✅     | ✅     | ✅           |

## Credits

I based the original version of this project on some code found in an old version of nopCommerce. They only used Autofac; I wanted it to be more generic for use in projects where the IOC container may need to be swapped out for another implementation.