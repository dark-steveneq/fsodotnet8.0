using FSO.Server.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZstdSharp.Unsafe;

namespace FSO.Server.Api
{
    public class Startup : IAPILifetime
    {
        public IApplicationLifetime AppLifetime;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var par = services.AddCors(options => {
               options.AddDefaultPolicy(
                   builder => {
                       builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().WithExposedHeaders("content-disposition");
                   });

                options.AddPolicy("AdminAppPolicy",
                    builder => {
                        builder.WithOrigins("https://freeso.org", "http://localhost:8080").AllowAnyMethod().AllowAnyHeader().AllowCredentials().WithExposedHeaders("content-disposition");
                    });
            }).AddMvc().AddMvcOptions((MvcOptions opts) => {
                opts.EnableEndpointRouting = true;
            }).AddRinMvcSupport();

            services.AddRin();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseRin();
                app.UseRinMvcSupport();
                app.UseDeveloperExceptionPage();
                app.UseRinDiagnosticsHandler();
            }
            else
            {
                app.UseHsts();
            }
            app.UseCors();
            //app.UseHttpsRedirection();
            AppLifetime = appLifetime;
        }

        public void Stop()
        {
            AppLifetime.StopApplication();
        }
    }
}
