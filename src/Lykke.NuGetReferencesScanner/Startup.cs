using System;
using JetBrains.Annotations;
using Lykke.NuGetReferencesScanner.Domain;
using Lykke.NuGetReferencesScanner.Domain.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.NuGetReferencesScanner
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

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
            services.AddSingleton<IReferencesScanner, GitScanner>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        [UsedImplicitly]
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            app.UseDeveloperExceptionPage();
            app.UseBrowserLink();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
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
