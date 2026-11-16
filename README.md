<img src="https://github.com/gordon-matt/Dependo/raw/master/_Misc/Logo.png" alt="Logo" width="250" />

## Intro

Dependo is a library that let's you resolve dependencies from anywhere via a singleton call to `DependoResolver.Instance`. This is mostly useful for resolving dependencies in static classes, including extensions methods.

Additionally, there is an `IDependencyRegistrar` interface to let you register dependencies from anywhere, such as class libraries. If you need to take advantage of implementation-specific registration features, you can make use of the more specialized dependency registrar, such as `IAutofacDependencyRegistrar`, `IDryIocDependencyRegistrar`, etc.

## Compile-time discovery

Finding those registrars used to reflect over every assembly already loaded in the process. Referencing a Dependo container package now generates that list while the project compiles.

The application project (the one with an entry point) writes a module initializer that loads each participating assembly and seals `DependoCatalog`. Library projects that contain an `IDependencyRegistrar`, or a concrete type exported with `DependoExports`, write their own initializer. After `DependoCatalog` is sealed, `ITypeFinder` answers those queries from the catalog. Published container packages carry the generator. A project reference does not, so attach `Dependo.Generator` with `OutputItemType` set to `Analyzer`.

`IDependencyRegistrar` and the container-specific registrar interfaces are always catalogued. For anything else a host discovers by scanning, mark the service once:

```csharp
[assembly: Dependo.DependoExports(typeof(IStartupTask), typeof(ITask))]
```

Put that attribute on an assembly that implementors reference, or on each assembly that contains implementations. The generator only sees a project that references Dependo directly, because source generators do not flow through transitive packages. Types discovered this way still need a public or internal parameterless constructor when the host activates them itself.

Assemblies loaded after startup, such as plugins, are still reflected once, and only those assemblies. Framework assemblies that match the type finder's skip pattern are not. Hosts that load plugins themselves should turn off the extra pass that loads every DLL from the output folder:

```csharp
DependoCatalog.ProbeBinFolder = false;
```

Set that before the container is built. Leave the default when plugins are discovered by probing the bin folder.

To force the mode, set `DependoCatalog` to `root`, `local`, or `off`. `off` keeps the reflection scan.

Check the demo project for a clearer idea on how to use Dependo. Or for a more advanced example, see it being used in [MantleCMS](https://github.com/gordon-matt/MantleCMS)

## NuGet Packages

| Package                | Version                                                                                                                                                           | Downloads                                                                                                                                                         |
|------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Dependo                | [![NuGet](https://img.shields.io/nuget/v/Dependo?style=flat-square&logo=nuget&label=Version)](https://www.nuget.org/packages/Dependo)                             | [![NuGet](https://img.shields.io/nuget/dt/Dependo?style=flat-square&logo=nuget&label=Downloads)](https://www.nuget.org/packages/Dependo)                             |
| Dependo.Autofac        | [![NuGet](https://img.shields.io/nuget/v/Dependo.Autofac?style=flat-square&logo=nuget&label=Version)](https://www.nuget.org/packages/Dependo.Autofac)             | [![NuGet](https://img.shields.io/nuget/dt/Dependo.Autofac?style=flat-square&logo=nuget&label=Downloads)](https://www.nuget.org/packages/Dependo.Autofac)             |
| Dependo.DotNetDefault  | [![NuGet](https://img.shields.io/nuget/v/Dependo.DotNetDefault?style=flat-square&logo=nuget&label=Version)](https://www.nuget.org/packages/Dependo.DotNetDefault) | [![NuGet](https://img.shields.io/nuget/dt/Dependo.DotNetDefault?style=flat-square&logo=nuget&label=Downloads)](https://www.nuget.org/packages/Dependo.DotNetDefault) |
| Dependo.DryIoc         | [![NuGet](https://img.shields.io/nuget/v/Dependo.DryIoc?style=flat-square&logo=nuget&label=Version)](https://www.nuget.org/packages/Dependo.DryIoc)               | [![NuGet](https://img.shields.io/nuget/dt/Dependo.DryIoc?style=flat-square&logo=nuget&label=Downloads)](https://www.nuget.org/packages/Dependo.DryIoc)               |
| Dependo.Lamar          | [![NuGet](https://img.shields.io/nuget/v/Dependo.Lamar?style=flat-square&logo=nuget&label=Version)](https://www.nuget.org/packages/Dependo.Lamar)                 | [![NuGet](https://img.shields.io/nuget/dt/Dependo.Lamar?style=flat-square&logo=nuget&label=Downloads)](https://www.nuget.org/packages/Dependo.Lamar)                 |
| Dependo.LightInject    | [![NuGet](https://img.shields.io/nuget/v/Dependo.LightInject?style=flat-square&logo=nuget&label=Version)](https://www.nuget.org/packages/Dependo.LightInject)     | [![NuGet](https://img.shields.io/nuget/dt/Dependo.LightInject?style=flat-square&logo=nuget&label=Downloads)](https://www.nuget.org/packages/Dependo.LightInject)     |

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

## Support

If you find this project useful, consider supporting its development.

<p align="center">
  <a href="https://buymeacoffee.com/vnmatt">
    <img src="https://github.com/gordon-matt/MyResume/blob/gh-pages/assets/images/BuyMeACoffee_Logo.png"
         alt="Buy Me a Coffee"
         height="40">
  </a>&nbsp;&nbsp;&nbsp;
  <a href="https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=gordon_matt%40live%2ecom&lc=AU&currency_code=AUD&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted">
    <img src="https://github.com/gordon-matt/MyResume/blob/gh-pages/assets/images/PayPal_Logo.png"
         alt="Donate with PayPal"
         height="40">
  </a>
</p>

<p align="center">
  Prefer using your phone? Scan the QR code:
</p>

<p align="center">
  <a href="https://buymeacoffee.com/vnmatt">
    <img src="https://github.com/gordon-matt/MyResume/blob/gh-pages/assets/images/BuyMeACoffee_QR.png"
         alt="Buy Me a Coffee QR Code"
         width="180">
  </a>
</p>

<img src="https://komarev.com/ghpvc/?username=gordon-matt&label=GitHub%20Hits%20Since%202025-06-01%3A%20&color=ff0000&style=flat" alt="gordon-matt" />