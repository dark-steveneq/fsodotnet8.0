using FSO.Server.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace FSO.Server.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {}

        public static IAPILifetime StartUserApi(string[] urls)
        {
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.WebHost.UseUrls(urls);
            var app = builder.Build();
            
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseCors();
            app.UseRouting();
            app.MapControllers();

            var lifetime = new APIControl((IApplicationLifetime)app.Services.GetService(typeof(IApplicationLifetime)));
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
