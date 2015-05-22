using System.Configuration;
using System.Data.Entity;
using System.Reflection;
using System.Web;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using CacheCow.Server;
using Newtonsoft.Json;
using WebApi.Hal.Web.App_Start;
using WebApi.Hal.Web.Data;

namespace WebApi.Hal.Web
{
    public class WebApiApplication : HttpApplication
    {
        string connectionString;
        IContainer container;

        protected void Application_Start()
        {
            connectionString = ConfigurationManager.ConnectionStrings["BeerDatabase"].ConnectionString;

            RouteConfig.RegisterRoutes(GlobalConfiguration.Configuration.Routes);

            GlobalConfiguration.Configuration.Formatters.Add(new JsonHalMediaTypeFormatter());
            //GlobalConfiguration.Configuration.Formatters.Add(new XmlHalMediaTypeFormatter());

            GlobalConfiguration.Configuration.Formatters.Remove(GlobalConfiguration.Configuration.Formatters.JsonFormatter);
            GlobalConfiguration.Configuration.Formatters.Remove(GlobalConfiguration.Configuration.Formatters.XmlFormatter);

            var containerBuilder = new ContainerBuilder();

            ConfigureContainer(containerBuilder);

            Database.SetInitializer(new DbUpDatabaseInitializer(connectionString));

            container = containerBuilder.Build();
            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;

            GlobalConfiguration.Configuration.MessageHandlers.Add(new CachingHandler(GlobalConfiguration.Configuration));
        }

        void ConfigureContainer(ContainerBuilder containerBuilder)
        {
            // Register API controllers using assembly scanning.
            containerBuilder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            containerBuilder
                .Register(c => new BeerDbContext(connectionString))
                .As<IBeerDbContext>()
                .InstancePerRequest();

            containerBuilder
                .RegisterType<BeerRepository>()
                .As<IRepository>()
                .InstancePerRequest();
        }
    }
}