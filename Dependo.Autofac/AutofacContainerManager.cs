using Autofac;

namespace Dependo.Autofac
{
    internal class AutofacContainerManager : IDisposable
    {
        private bool isDisposed = false;

        public AutofacContainerManager(IContainer container)
        {
            Container = container;
        }

        public IContainer Container { get; }

        public bool IsRegistered(Type serviceType) => Container.IsRegistered(serviceType);

        public T Resolve<T>(string key = "") where T : class
        {
            if (string.IsNullOrEmpty(key))
            {
                return Container.Resolve<T>();
            }
            return Container.ResolveKeyed<T>(key);
        }

        public T Resolve<T>(IDictionary<string, object> ctorArgs, string key = "") where T : class
        {
            var ctorParams = ctorArgs.Select(x => new NamedParameter(x.Key, x.Value)).ToArray();

            if (string.IsNullOrEmpty(key))
            {
                return Container.Resolve<T>(ctorParams);
            }
            return Container.ResolveKeyed<T>(key, ctorParams);
        }

        public object Resolve(Type type) => Container.Resolve(type);

        public IEnumerable<T> ResolveAll<T>(string key = "")
        {
            if (string.IsNullOrEmpty(key))
            {
                return Container.Resolve<IEnumerable<T>>().ToArray();
            }
            return Container.ResolveKeyed<IEnumerable<T>>(key).ToArray();
        }

        public IEnumerable<T> ResolveAllNamed<T>(string name) => Container.ResolveKeyed<IEnumerable<T>>(name).ToArray();

        public T ResolveNamed<T>(string name)
            where T : class
        {
            return Container.ResolveNamed<T>(name);
        }

        public object ResolveOptional(Type serviceType) => Container.ResolveOptional(serviceType);

        public T ResolveUnregistered<T>()
            where T : class
        {
            return ResolveUnregistered(typeof(T)) as T;
        }

        public object ResolveUnregistered(Type type)
        {
            Exception innerException = null;
            foreach (var constructor in type.GetConstructors())
            {
                try
                {
                    //try to resolve constructor parameters
                    var parameters = constructor.GetParameters().Select(parameter =>
                    {
                        var service = Resolve(parameter.ParameterType);
                        if (service == null)
                        {
                            throw new ApplicationException("Unknown dependency");
                        }
                        return service;
                    });

                    //all is ok, so create instance
                    return Activator.CreateInstance(type, parameters.ToArray());
                }
                catch (Exception ex)
                {
                    innerException = ex;
                }
            }
            throw new ApplicationException("No constructor was found that had all the dependencies satisfied.", innerException);
        }

        public bool TryResolve<T>(out T instance)
            where T : class
        {
            return Container.TryResolve<T>(out instance);
        }

        public bool TryResolve(Type serviceType, out object instance) => Container.TryResolve(serviceType, out instance);

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
            {
                return;
            }

            if (disposing)
            {
                Container.Dispose();
                // Free any other managed objects here.
            }

            // Free any unmanaged objects here.
            isDisposed = true;
        }

        #endregion IDisposable Members
    }
}