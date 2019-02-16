using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AdventureWorksAKS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
           WebHost.CreateDefaultBuilder(args)
              .ConfigureAppConfiguration((context, config) =>
              {
                  var env = context.HostingEnvironment;

                  // Call other providers here and call AddKeyPerFile last.
                  config.SetBasePath(env.ContentRootPath)
                      .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                      .AddEnvironmentVariables();

                  // Call other providers here and call AddKeyPerFile last.
                  if (env.IsDevelopment())
                  {
                      // Use the following config builder method to test local file secrets
                      // You must create a folder called '/kvmnt' at the root of the web project
                      // Create a file for each of your secrest, using key name as the secret file name (no file suffix)
                      // Store the value of the scecret as a single line of text in the secret file
                      // NOTE: You must comment-out the config.AddUserSecrets<Startup>(); statement above

                      // config.AddKeyPerFile($"{env.ContentRootPath}/kvmnt", false);
                   
                      config.AddJsonFile("localsecrets.json", optional: false, reloadOnChange: true);
                  }
                  else
                  {
                      // config.AddKeyPerFile("/kvmnt", false);
                  }
              })
              .UseStartup<Startup>();
    }
}
