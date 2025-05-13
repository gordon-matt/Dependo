namespace Dependo;

public abstract class BaseDependoContainer : IDependoContainer
{
    public abstract bool IsRegistered(Type serviceType);

    public abstract T Resolve<T>() where T : class;

    public abstract T Resolve<T>(IDictionary<string, object> ctorArgs) where T : class;

    public abstract object Resolve(Type type);

    public abstract IEnumerable<T> ResolveAll<T>();

    public abstract IEnumerable<T> ResolveAllNamed<T>(string name);

    public abstract T ResolveNamed<T>(string name) where T : class;

    public virtual T? ResolveUnregistered<T>() where T : class =>
        ResolveUnregistered(typeof(T)) as T;

    public virtual object ResolveUnregistered(Type type)
    {
        Exception? innerException = null;
        foreach (var constructor in type.GetConstructors())
        {
            try
            {
                // Try to resolve constructor parameters
                var parameters = constructor.GetParameters().Select(parameter =>
                {
                    object service = Resolve(parameter.ParameterType);
                    return service ?? throw new ApplicationException("Unable to resolve dependency");
                });

                // Create instance with resolved parameters
                return Activator.CreateInstance(type, parameters.ToArray())!;
            }
            catch (Exception ex)
            {
                innerException = ex;
            }
        }
        throw new ApplicationException("No constructor was found that had all the dependencies satisfied.", innerException);
    }

    public abstract bool TryResolve<T>(out T? instance) where T : class;

    public abstract bool TryResolve(Type serviceType, out object? instance);
}