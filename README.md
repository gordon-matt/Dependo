<img src="https://github.com/gordon-matt/Dependo/blob/master/_Misc/Logo.png" alt="Logo" width="250" />

# Dependo

## Info

Dependo is a library that let's you resolve dependencies from anywhere via a singleton call to `DependoResolver.Instance`. This is mostly useful for resolving dependencies in static classes, including extensions methods.

Additionally, there is an `IDependencyRegistrar` interface to let you register dependencies from anywhere, such as class libraries. If you need to take advantage of implementation-specific registration features, you can make use of the more specialized dependency registrar, such as `IAutofacDependencyRegistrar`, `IDryIocDependencyRegistrar`, etc.

Check the demo project for a clearer idea on how to use Dependo. Or for a more advanced example, see it being used in [MantleCMS](https://github.com/gordon-matt/MantleCMS)

## NuGet Packages
> Dependo: https://www.nuget.org/packages/Dependo/

> Dependo.Autofac: https://www.nuget.org/packages/Dependo.Autofac/

> Dependo.DotNetDefault: https://www.nuget.org/packages/Dependo.DotNetDefault/

> Dependo.DryIoc: https://www.nuget.org/packages/Dependo.DryIoc/

> Dependo.Lamar: https://www.nuget.org/packages/Dependo.Lamar/

> Dependo.LightInject: https://www.nuget.org/packages/Dependo.LightInject/

## Feature Support

The default .NET implementation suits most needs. You could start with that and then if more advanced features are required, simply swap out that implementation for another. Use the following table to help decide which is most suitable for your project.

| Method                                                                  | Default .NET DI | Autofac | DryIoc | Lamar | LightInject |
|-------------------------------------------------------------------------|------------------|---------|--------|--------|-----------|
| `IsRegistered(Type serviceType)`                                        | ✅               | ✅      | ✅     | ✅     | ✅        |
| `Resolve<T>()`                                                          | ✅               | ✅      | ✅     | ✅     | ✅        |
| `Resolve<T>(IDictionary<string, object> ctorArgs)`                      | ❌               | ✅      | ✅     | ❌     | ❌        |
| `object Resolve(Type type)`                                             | ✅               | ✅      | ✅     | ✅     | ✅        |
| `T ResolveKeyed<T>(object key)`                                         | ✅               | ✅      | ✅     | ✅     | ✅        |
| `T ResolveKeyed<T>(object key, IDictionary<string, object> ctorArgs)`   | ❌               | ✅      | ✅     | ❌     | ❌        |
| `IEnumerable<T> ResolveAllKeyed<T>(object key)`                         | ✅               | ✅      | ❌     | ❌     | ❌        |
| `IEnumerable<T> ResolveAll<T>()`                                        | ✅               | ✅      | ✅     | ✅     | ✅        |
| `IEnumerable<T> ResolveAllNamed<T>`                                     | ✅               | ✅      | ❌     | ❌     | ❌        |
| `T ResolveNamed<T>(string name)`                                        | ✅               | ✅      | ✅     | ✅     | ✅        |
| `T? ResolveUnregistered<T>()`                                           | ✅               | ✅      | ✅     | ✅     | ✅        |
| `object ResolveUnregistered(Type type)`                                 | ✅               | ✅      | ✅     | ✅     | ✅        |
| `bool TryResolve<T>(out T? instance)`                                   | ✅               | ✅      | ✅     | ✅     | ✅        |
| `bool TryResolve(Type serviceType, out object? instance)`               | ✅               | ✅      | ✅     | ✅     | ✅        |

## Credits

I based the original version of this project on some code found in an old version of nopCommerce. They only used Autofac; I wanted it to be more generic for use in projects where the IOC container may need to be swapped out for another implementation.