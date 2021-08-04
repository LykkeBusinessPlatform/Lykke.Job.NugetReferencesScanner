using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Lykke.NuGetReferencesScanner.Domain;
using Lykke.NuGetReferencesScanner.Domain.Abstractions;
using Lykke.NuGetReferencesScanner.Domain.Options;
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
            services.AddSingleton(Configuration);
            services.AddMvc();

            var githubSection = Configuration.GetSection(GitHubScanner.ConfigurationSection);
            var bitBucketSection = Configuration.GetSection(BitBucketScanner.ConfigurationSection);

            if (githubSection.Exists())
            {
                services.Configure<GithubOptions>(githubSection);
                services.AddSingleton<IOrganizationScanner, GitHubScanner>();
            }

            if (bitBucketSection.Exists())
            {
                services.Configure<BitBucketOptions>(bitBucketSection);
                services.AddSingleton<IOrganizationScanner, BitBucketScanner>();
            }

            services.AddSingleton<IParserModeProvider, ParserModeProvider>();
            services.AddSingleton<IPackageWhitelist, PackageWhitelist>();
            services.AddSingleton<IReferencesScanner, GitScanner>();
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
