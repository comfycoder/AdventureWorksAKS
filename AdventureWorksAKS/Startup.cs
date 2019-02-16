using System;
using System.Data.SqlClient;
using System.Net.Http;
using AdventureWorksAKS.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;

namespace AdventureWorksAKS
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
            // Adds services required for using options.
            services.AddOptions();

            // Add logging services
            services.AddLogging();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(options =>
                {
                    // Format the JSON string returned from the service.
                    options.SerializerSettings.Formatting = Formatting.Indented;
                });

            var sqlConnection = GetSqlConnectionAsync();

            services.AddDbContext<AdventureWorksContext>(options =>
                options.UseSqlServer(sqlConnection,
                sqlServerOptionsAction: sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                    sqlServerOptions.UseRowNumberForPaging();
                })
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            string basePath = Configuration["AspNetCorePathBase"];

            if (!string.IsNullOrEmpty(basePath))
            {
                app.Use(async (context, next) =>
                {
                    context.Request.PathBase = basePath;
                    await next.Invoke();
                });
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        public SqlConnection GetSqlConnectionAsync()
        {
            string tenantId = Configuration["TenantId"];
            string clientId = Configuration["ClientId"];
            string clientSecret = Configuration["ClientSecret"];
            string dbServer = Configuration["DbServerName"];
            string dbName = Configuration["DbName"];

            var authority = string.Format("https://login.windows.net/{0}", tenantId);
            var resource = "https://database.windows.net/";
            var scope = "";

            var builder = new SqlConnectionStringBuilder();
            builder["Data Source"] = $"{dbServer}.database.windows.net";
            builder["Initial Catalog"] = dbName;
            builder["Connect Timeout"] = 30;
            builder["Persist Security Info"] = false;
            builder["TrustServerCertificate"] = false;
            builder["Encrypt"] = true;
            builder["MultipleActiveResultSets"] = false;

            var con = new SqlConnection(builder.ToString());

            var token = GetAccessToken(clientId, clientSecret, authority, resource, scope);

            con.AccessToken = token;

            return con;
        }

        public static string GetAccessToken(string clientId, string clientSecret, string authority, string resource, string scope)
        {

            var authContext = new AuthenticationContext(authority, TokenCache.DefaultShared);

            var clientCred = new ClientCredential(clientId, clientSecret);

            var result = authContext.AcquireTokenAsync(resource, clientCred).GetAwaiter().GetResult();

            if (result == null)
            {
                throw new InvalidOperationException("Could not get token");
            }

            return result.AccessToken;
        }
    }

}
