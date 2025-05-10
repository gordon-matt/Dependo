# Dependo

## Info

Dependo is a library that let's you resolve dependencies from anywhere via a singleton call to `EngineContext.Current`. This is mostly useful for resolving dependencies in static classes, including extensions methods.

Additionally, there is an `IDependencyRegistrar` interface to let you register dependencies from anywhere, such as class libraries.

Check the demo project for how to use it.

## NuGet Packages
> Dependo: https://www.nuget.org/packages/Dependo/

> Dependo.Autofac: https://www.nuget.org/packages/Dependo.Autofac/

> Dependo.DryIoc: coming soon.

> Dependo.Lamar: coming soon.

## Credits

I based the original version of this project on some code found in an old version of nopCommerce. They only used Autofac; I wanted it to be more generic for use in projects where the IOC container may need to be swapped out for another implementation.