using Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using QSI.ORM.NHibernate.Extension;
using System;
using System.Reflection;

namespace Moduit.Interview.Engine.Docker.Linux
{
    /// <summary>
    /// Autofac module class, you can configure any additional builder container setup here
    /// </summary>
    public class AutofacModule : Autofac.Module
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        public AutofacModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// 
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <exception cref="ArgumentException"></exception>
        protected override void Load(ContainerBuilder builder)
        {
            #region Transactional
            Assembly[] assemblies = new Assembly[]
            {
                Assembly.Load("Moduit.Interview.Service"),
                Assembly.Load("Moduit.Interview.Repository.NHibernate")
            };
            builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>();
            builder.RegisterTransactionalAssemblies(assemblies, e =>
            {
                var context = e.Resolve<IHttpContextAccessor>();
                if (context.HttpContext == null)
                    return "Unauthenticated";
                return context.HttpContext.User.Identity.Name;
            });
            builder.RegisterCriteriaCondition();
            builder.RegisterGeneralHelperDao();
            #endregion

            base.Load(builder);
        }
    }
}
