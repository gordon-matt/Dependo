using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dependable.Autofac
{
    public class AutofacEngine : IEngine, IDisposable
    {
        #region Private Members

        private AutofacContainerManager containerManager;
        private bool disposed = false;

        #endregion Private Members

        #region Properties

        /// <summary>
        /// Gets or sets service provider
        /// </summary>
        public virtual IServiceProvider ServiceProvider { get; private set; }

        #endregion Properties

        #region IEngine Members

        /// <summary>
        /// Add and configure services
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration root of the application</param>
        /// <returns>Service provider</returns>
        public virtual IServiceProvider ConfigureServices(ContainerBuilder containerBuilder, IConfigurationRoot configuration)
        {
            //find startup configurations provided by other assemblies
            var typeFinder = new WebAppTypeFinder();

            //register dependencies
            RegisterDependencies(containerBuilder, typeFinder);

            //resolve assemblies here. otherwise, plugins can throw an exception when rendering views
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            return ServiceProvider;
        }

        public virtual T Resolve<T>()
            where T : class
        {
            return containerManager.Resolve<T>();
        }

        public T Resolve<T>(IDictionary<string, object> ctorArgs)
            where T : class
        {
            return containerManager.Resolve<T>(ctorArgs);
        }

        public virtual object Resolve(Type type) => containerManager.Resolve(type);

        public T ResolveNamed<T>(string name)
            where T : class
        {
            return containerManager.ResolveNamed<T>(name);
        }

        public virtual IEnumerable<T> ResolveAll<T>() => containerManager.ResolveAll<T>();

        public IEnumerable<T> ResolveAllNamed<T>(string name) => containerManager.ResolveAllNamed<T>(name);

        public virtual object ResolveUnregistered(Type type) => containerManager.ResolveUnregistered(type);

        public bool TryResolve<T>(out T instance)
            where T : class
        {
            return containerManager.TryResolve<T>(out instance);
        }

        public bool TryResolve(Type serviceType, out object instance) => containerManager.TryResolve(serviceType, out instance);

        #endregion IEngine Members

        #region Non-Public Methods

        /// <summary>
        /// Get IServiceProvider
        /// </summary>
        /// <returns>IServiceProvider</returns>
        protected virtual IServiceProvider GetServiceProvider()
        {
            var httpContextAccessor = ServiceProvider.GetService<IHttpContextAccessor>();
            var context = httpContextAccessor.HttpContext;
            return context != null ? context.RequestServices : ServiceProvider;
        }

        /// <summary>
        /// Register dependencies using Autofac
        /// </summary>
        /// <param name="containerBuilder">Container Builder</param>
        /// <param name="typeFinder">Type finder</param>
        protected virtual IServiceProvider RegisterDependencies(ContainerBuilder containerBuilder, ITypeFinder typeFinder)
        {
            //register engine
            containerBuilder.RegisterInstance(this).As<IEngine>().SingleInstance();

            //register type finder
            containerBuilder.RegisterInstance(typeFinder).As<ITypeFinder>().SingleInstance();

            //find dependency registrars provided by other assemblies
            var dependencyRegistrars = typeFinder.FindClassesOfType<IDependencyRegistrar>();

            //create and sort instances of dependency registrars
            var instances = dependencyRegistrars
                .Select(x => (IDependencyRegistrar)Activator.CreateInstance(x))
                .OrderBy(x => x.Order);

            //register all provided dependencies
            foreach (var dependencyRegistrar in instances)
            {
                dependencyRegistrar.Register(containerBuilder, typeFinder);
            }

            //create service provider
            var container = containerBuilder.Build();
            ServiceProvider = new AutofacServiceProvider(container);
            containerManager = new AutofacContainerManager(container);
            return ServiceProvider;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            //check for assembly already loaded
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
            if (assembly != null)
            {
                return assembly;
            }

            //get assembly from TypeFinder
            var typeFinder = Resolve<ITypeFinder>();
            return typeFinder.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
        }

        #endregion Non-Public Methods

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                containerManager.Dispose();
                // Free any other managed objects here.
            }

            // Free any unmanaged objects here.
            disposed = true;
        }

        #endregion IDisposable Members
    }
}