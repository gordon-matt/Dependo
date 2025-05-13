namespace Dependo;

public abstract class BaseDependoContainer : IDependoContainer
{
    /// <inheritdoc />
    public abstract bool IsRegistered(Type serviceType);

    /// <inheritdoc />
    public abstract T Resolve<T>() where T : class;

    /// <inheritdoc />
    public abstract T Resolve<T>(IDictionary<string, object> ctorArgs) where T : class;

    /// <inheritdoc />
    public abstract object Resolve(Type type);

    /// <inheritdoc />
    public abstract T ResolveKeyed<T>(object key) where T : class;

    /// <inheritdoc />
    public abstract T ResolveKeyed<T>(object key, IDictionary<string, object> ctorArgs) where T : class;

    /// <inheritdoc />
    public abstract IEnumerable<T> ResolveAllKeyed<T>(object key);

    /// <inheritdoc />
    public abstract IEnumerable<T> ResolveAll<T>();

    /// <inheritdoc />
    public abstract IEnumerable<T> ResolveAllNamed<T>(string name);

    /// <inheritdoc />
    public abstract T ResolveNamed<T>(string name) where T : class;

    /// <inheritdoc />
    public virtual T? ResolveUnregistered<T>() where T : class =>
        ResolveUnregistered(typeof(T)) as T;

    /// <inheritdoc />
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

    /// <inheritdoc />
    public abstract bool TryResolve<T>(out T? instance) where T : class;

    /// <inheritdoc />
    public abstract bool TryResolve(Type serviceType, out object? instance);

    public abstract void Dispose();

    protected static string StringifyKey(object key) => key switch
    {
        string s => s,
        int i => i.ToString(),
        Enum e => e.ToString(),
        Guid g => g.ToString(),
        null => throw new ArgumentNullException(nameof(key)),
        _ => $"{key.GetType().FullName}:{key.GetHashCode()}"
    };
}