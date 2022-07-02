using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moduit.Interview.Api.AspNetCore.Extensions;
using Moduit.Interview.Common.Commands;
using Newtonsoft.Json;
using QSI.ORM.Config;
using QSI.ORM.NHibernate.Extension;
using QSI.Swagger.Common;
using QSI.Swagger.Extensions;
using QSI.Web.Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Moduit.Interview.Engine.Docker.Linux
{
    /// <summary>
    /// Startup class that configure Apps DI, etc
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Load needed apps configuration on constructor
        /// </summary>
        /// <param name="env"></param>
        public Startup(IHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddYamlFile("configuration.yml", optional: false, reloadOnChange: true)
                .AddYamlFile($"configuration.{env.EnvironmentName}.yml", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        /// <summary>
        /// Apps configuration file
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Autofac container
        /// </summary>
        public ILifetimeScope AutofacContainer { get; private set; }

        /// <summary>
        ///  This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            #region Response Compression & Caching
            services.AddResponseCaching();
            services.AddMvc(options =>
            {
                options.CacheProfiles.Add("Default",
                    new CacheProfile()
                    {
                        NoStore = true,
                        Location = ResponseCacheLocation.None,
                        Duration = 0
                    });
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            })
                .AddControllersAsServices()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.Configure<GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Optimal);
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "image/svg+xml" });
            });
            #endregion
            
            #region ORM Setup
            var setting = new DatabaseConfiguration();
            Configuration.Bind("orm", setting);
            services.AddConnection(setting);
            #endregion

            #region Swagger UI
            services.AddSwaggerUI(Configuration);
            #endregion

            #region Extensions
            ModuitConfiguration moduitConfiguration = new ModuitConfiguration();
            Configuration.Bind("moduit", moduitConfiguration);
            services.AddModuitExtension(moduitConfiguration);
            #endregion
        }

        /// <summary>
        /// Configure Dependency Injection from container builder
        /// </summary>
        /// <param name="builder"></param>
        public void ConfigureContainer(ContainerBuilder builder)
        {
            // Register your own things directly with Autofac here. Don't
            // call builder.Populate(), that happens in AutofacServiceProviderFactory
            // for you.
            builder.RegisterModule(new AutofacModule(Configuration));
        }

        /// <summary>
        ///  This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="appLifetime"></param>
        /// <param name="swaggerConfiguration"></param>
        /// <param name="provider"></param>
        public void Configure(IApplicationBuilder app, IHostEnvironment env, ILoggerFactory loggerFactory, IHostApplicationLifetime appLifetime,
            SwaggerConfiguration swaggerConfiguration, IApiVersionDescriptionProvider provider)
        {
            loggerFactory.AddLog4Net(Configuration.GetValue<string>("Log4NetConfigFile:Name"));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // Forcing HTTPS Redirect
                // app.UseHsts();

                #region Response Sharper
                ResponseActionFactory factory = new ResponseActionFactory();
                factory.AddCase(async (e, c) =>
                {
                    var ce = e as QSI.Common.Exception.Validation.ValidationException;
                    if (ce != null)
                    {
                        c.Response.ContentType = "application/json";
                        c.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        var ex = new QSI.Web.Middleware.JsonException()
                        {
                            ErrorCode = ce.ErrorCode,
                            Message = ce.Message,
                            MessageKey = ce.MessageKey,
                            Exception = e.GetType().FullName,
                            Data = new Dictionary<string, object>
                            {
                                { "Failures", ce.Failures }
                            }
                        };

                        await c.Response.WriteAsync(JsonConvert.SerializeObject(ex));
                        return true;
                    }
                    return false;
                });
                app.UseMiddleware<MiddlewareExceptionShaper>(factory);
                #endregion
            }

            #region Swagger
            app.UseSwaggerUI(swaggerConfiguration, provider);
            #endregion

            app.UseResponseCaching();
            app.Use(async (context, next) =>
            {
                context.Response.GetTypedHeaders().CacheControl =
                    new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                    {
                        NoCache = true,
                        NoStore = true,
                        MaxAge = TimeSpan.Zero
                    };

                await next();
            });
            app.UseResponseCompression();
            app.UseAuthentication();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });


            this.AutofacContainer = app.ApplicationServices.GetAutofacRoot();
            appLifetime.ApplicationStopped.Register(() =>
            {
                if (this.AutofacContainer != null) this.AutofacContainer.Dispose();
            });
        }
    }
}
