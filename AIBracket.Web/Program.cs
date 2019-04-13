using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using AIBracket.Web.Managers;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIBracket.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            GameCacheManager.Start();
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args).UseKestrel(x =>
            {
                if (System.IO.File.Exists("Cert.pfx"))
                {
                    x.Listen(IPAddress.Any, 443, listenOptions =>
                    {
                        listenOptions.UseHttps("Cert.pfx", "password");
                    });
                }
                x.Listen(IPAddress.Any, 80);
            }).UseStartup<Startup>().UseContentRoot(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        }
    }
}
