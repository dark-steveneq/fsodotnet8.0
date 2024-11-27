using FSO.Server.Common;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace FSO.Server.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            StartUserApi(["http://localhost:9000"], true);
        }

        public static IAPILifetime StartUserApi(string[] urls)
        {
            return StartUserApi(urls, false);
        }

        public static IAPILifetime StartUserApi(string[] urls, bool sync)
        {
            var app = WebHost.CreateDefaultBuilder()
                .UseUrls(urls)
                .ConfigureLogging(conf => {
                    conf.SetMinimumLevel(LogLevel.None);
                })
                .UseKestrel(options =>
                {
                    options.Limits.MaxRequestBodySize = 500000000;
                })
                .SuppressStatusMessages(true)
                .UseStartup<Startup>().Build();

            var lifetime = new APIControl((IApplicationLifetime)app.Services.GetService(typeof(IApplicationLifetime)));
            if (sync)
                app.Run();
            else
                app.RunAsync();
            return lifetime;
        }
    }

    public class APIControl : IAPILifetime
    {
        private IApplicationLifetime Lifetime;
        
        public APIControl(IApplicationLifetime lifetime)
        {
            Lifetime = lifetime;
        }

        public void Stop()
        {
            Lifetime.StopApplication();
        }
    }
}
