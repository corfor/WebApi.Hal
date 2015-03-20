﻿using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Newtonsoft.Json;
using WebApi.Hal.Web.App_Start;
using WebApi.Hal.Web.Data;

namespace WebApi.Hal.Web
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        IContainer container;
        string connectionString;

        protected void Application_Start()
        {
            connectionString = ConfigurationManager.ConnectionStrings["BeerDatabase"].ConnectionString;

            RouteConfig.RegisterRoutes(GlobalConfiguration.Configuration.Routes);

            GlobalConfiguration.Configuration.Formatters.Add(new JsonHalMediaTypeFormatter());
            GlobalConfiguration.Configuration.Formatters.Add(new XmlHalMediaTypeFormatter());
            var xmlFormatter =
                GlobalConfiguration.Configuration.Formatters.FirstOrDefault(f => f.SupportedMediaTypes.Any(m => string.Equals(m.MediaType, "application/xml", StringComparison.OrdinalIgnoreCase)));
            if (xmlFormatter != null) GlobalConfiguration.Configuration.Formatters.Remove(xmlFormatter);

            var containerBuilder = new ContainerBuilder();

            ConfigureContainer(containerBuilder);

            Database.SetInitializer(new DbUpDatabaseInitializer(connectionString));

            container = containerBuilder.Build();
            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
        }

        private void ConfigureContainer(ContainerBuilder containerBuilder)
        {
            // Register API controllers using assembly scanning.
            containerBuilder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            containerBuilder
                .Register(c=> new BeerDbContext(connectionString))
                .As<IBeerDbContext>()
                .InstancePerRequest();

            containerBuilder
                .RegisterType<BeerRepository>()
                .As<IRepository>()
                .InstancePerRequest();
        }
    }
}