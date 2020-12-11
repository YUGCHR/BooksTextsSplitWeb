using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using BooksTextsSplit.Services;
using StackExchange.Redis;
using CachingFramework.Redis;
using System.Diagnostics.Contracts;
using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using BooksTextsSplit.Helpers;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Authentication.Cookies;
using CachingFramework.Redis.Contracts.Providers;
using Microsoft.Extensions.Logging;

namespace BooksTextsSplit
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddLocalization(opts => { opts.ResourcesPath = "Resources"; });
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            // configure basic authentication with BasicAuthenticationHandler - 
            //services.AddAuthentication("BasicAuthentication")
            //    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // configure DI for application services            
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IResultDataService, ResultDataService>();
            services.AddScoped<BooksTextsSplit.Models.UserData>(); // ?

            // Consuming a scoped service in a background task
            //services.AddSingleton<MonitorLoop>();
            services.AddHostedService<QueuedHostedService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddScoped<IBackgroungTasksService, BackgroungTasksService>();

            //CookieAuthenticationOptions
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
            });

            services.AddControllersWithViews();
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });
            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            try
            {
                services.AddSingleton<ICosmosDbService>( sp =>  InitializeCosmosClientInstanceAsync(sp, Configuration.GetSection("CosmosDb")).GetAwaiter().GetResult());                
            }
            catch (Exception ex)
            {
                string Message = ex.Message;
                Console.WriteLine("\n\n CosmosDbService was not initialized: \n\n" + Message + "\n\n");
                throw;
            }

            try
            {
                ConnectionMultiplexer muxer = ConnectionMultiplexer.Connect("localhost");
                //muxer.GetDatabase().StringGet
                //services.AddSingleton<RedisContext>(new RedisContext(muxer));
                services.AddSingleton<ICacheProviderAsync>(new RedisContext(muxer).Cache);
            }
            catch (Exception ex)
            {
                string Message = ex.Message;
                Console.WriteLine("\n\n Redis server did not start: \n\n" + Message + "\n\n");
                throw;
            }

            //services.AddLocalization(op ;
            //services.AddSingleton<IDatabase>(muxer.GetDatabase());
            //services.AddSingleton<CachingFramework.Redis.Contracts.Providers.ICacheProvider>(muxer);
            services.AddScoped<IAccessCacheData, AccessCacheData>();
            services.AddScoped<IControllerDataManager, ControllerDataManager>();
            services.AddScoped<IControllerCacheManager, ControllerCacheManager>();
            services.AddScoped<IControllerDbManager, ControllerDbManager>();
            services.AddScoped<IControllerQueryManager, ControllerQueryManager>();

        }

        /// <summary>
        /// Creates a Cosmos DB database and a container with the specified partition key. 
        /// </summary>
        /// <returns></returns>
        private static async Task<CosmosDbService> InitializeCosmosClientInstanceAsync(IServiceProvider sp, IConfigurationSection configurationSection)
        {
            string databaseName = configurationSection.GetSection("DatabaseName").Value;
            string containerName = configurationSection.GetSection("ContainerName").Value;
            string userContainerName = configurationSection.GetSection("UserContainerName").Value;
            string account = configurationSection.GetSection("Account").Value;
            string key = configurationSection.GetSection("Key").Value;            
            CosmosClientBuilder clientBuilder = new CosmosClientBuilder(account, key);
            CosmosClient client = clientBuilder
                                .WithConnectionModeDirect()
                                .Build();
            CosmosDbService cosmosDbService = new CosmosDbService(client, databaseName, containerName, userContainerName, sp.GetService<ILogger<CosmosDbService>>());            
            DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
            await database.Database.CreateContainerIfNotExistsAsync(containerName, "/id");
            await database.Database.CreateContainerIfNotExistsAsync(userContainerName, "/userId");

            return cosmosDbService;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            var supportedCultures = new[]
            {
                new CultureInfo("en-US")
            };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }


    }
}
