﻿using System;
using Arbor.AspNetCore.Mvc.Formatting.HtmlForms.Core;
using Arbor.SyslogServer.Areas.Syslog;
using Arbor.SyslogServer.Time;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog.AspNetCore;
using IApplicationLifetime = Microsoft.AspNetCore.Hosting.IApplicationLifetime;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Arbor.SyslogServer.AspNetCore
{
    public class Startup
    {
        private readonly ILifetimeScope _webHostScope;
        private readonly Serilog.ILogger _logger;
        private ILifetimeScope _aspNetScope;

        public Startup([NotNull] ILifetimeScope webHostScope, [NotNull] Serilog.ILogger logger)
        {
            _webHostScope = webHostScope ?? throw new ArgumentNullException(nameof(webHostScope));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [UsedImplicitly]
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<IHostedService, SyslogBackgroundService>();

            services
                .AddMvc(options =>
                {
                    options.InputFormatters.Insert(0,
                        new XWwwFormUrlEncodedFormatter(new SerilogLoggerFactory(_logger)
                            .CreateLogger<XWwwFormUrlEncodedFormatter>()));
                })
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.Converters.Add(new DateConverter());
                    options.SerializerSettings.Formatting = Formatting.Indented;
                });

            services.AddSingleton<IServerAddressesFeature, ServerAddressesFeature>();

            _aspNetScope = _webHostScope.BeginLifetimeScope(builder => builder.Populate(services));

            return new AutofacServiceProvider(_aspNetScope);
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseDeveloperExceptionPage();

            app.UseMvc();

            app.UseStaticFiles();

            appLifetime.ApplicationStopped.Register(() => _aspNetScope.Dispose());
        }
    }
}
