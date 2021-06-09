using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Lykke.NuGetReferencesScanner.Domain.Abstractions;
using Lykke.NuGetReferencesScanner.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lykke.NuGetReferencesScanner
{
    public class Startup
    {
        private const string ReferencePrefixesEnvVarKey = "ReferencePrefixes";
        private const string AreReferencePrefixesIncludeEnvVarKey = "AreReferencePrfixesIncluded";
        private const string GitHubApiEnvVarKey = "GitHubApiKey";
        private const string GitHubOrganizationEnvVarKey = "GitHubOrganization";
        private const string BitBucketEnvVarKey = "BitBucketKey";
        private const string BitBucketSecretEnvVarKey = "BitBucketSecret";
        private const string BitBucketAccountEnvVarKey = "BitBucketAccount";

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        [UsedImplicitly]
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            try
            {
                var projFileParser = new ProjectFileParser(
                    Configuration[ReferencePrefixesEnvVarKey].Split(',').Select(s => s.Trim()),
                    bool.Parse(Configuration[AreReferencePrefixesIncludeEnvVarKey]));
                var scanners = new List<IOrganizationScanner>();

                var ghKey = Configuration[GitHubOrganizationEnvVarKey];
                if (!string.IsNullOrWhiteSpace(ghKey))
                    scanners.Add(
                        new GitHubScanner(
                            projFileParser,
                            Configuration[GitHubOrganizationEnvVarKey],
                            Configuration[GitHubApiEnvVarKey]));

                var bbKey = Configuration[BitBucketAccountEnvVarKey];
                if (!string.IsNullOrWhiteSpace(bbKey))
                    scanners.Add(
                        new BitBucketScanner(
                            projFileParser,
                            Configuration[BitBucketAccountEnvVarKey],
                            Configuration[BitBucketEnvVarKey],
                            Configuration[BitBucketSecretEnvVarKey]));

                services.AddSingleton<IReferencesScanner>(new GitScanner(scanners));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        [UsedImplicitly]
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            IHostApplicationLifetime appLifetime)
        {
            app.UseDeveloperExceptionPage();
            //app.UseBrowserLink();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            appLifetime.ApplicationStarted.Register(() =>
            {
                try
                {
                    var scanner = app.ApplicationServices.GetService<IReferencesScanner>();
                    scanner.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
        }
    }
}

