using Castle.Core.Resource;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Lofn.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    #if !DEBUG
                    webBuilder.UseKestrel(options =>
                    {
                        options.ConfigureHttpsDefaults(httpsOptions =>
                        {
                            var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("MonexUp.API.monexup.com.pfx");
                            using (MemoryStream ms = new MemoryStream())
                            {
                                s.CopyTo(ms);
                                httpsOptions.ServerCertificate = new X509Certificate2(ms.ToArray(), "pikpro6");
                            }
                        });
                    });
                    #endif
                    webBuilder.UseStartup<Startup>();
                });
    }
}
