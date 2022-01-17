# Dependo

## Info

Dependo is a library that let's you resolve dependencies from anywhere via a singleton call to `EngineContext.Current`. This is mostly useful for resolving dependencies in static classes, including extensions methods.

Additionally, the `Autofac` implementation (the only implementation thus far) provides an `IDependencyRegistrar` interface to let you register dependencies from anywhere, such as class libraries.

Check the demo project for how to use it.

## NuGet Packages
> Dependo: https://www.nuget.org/packages/Dependo/

> Dependo.Autofac: https://www.nuget.org/packages/Dependo.Autofac/

## Credits

Much of the code comes from an old version of nopCommerce