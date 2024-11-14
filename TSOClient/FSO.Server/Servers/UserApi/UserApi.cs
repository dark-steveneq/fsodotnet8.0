using System;
using FSO.Server.Common;
using Microsoft.Owin.Hosting;
using System.Web.Http;
using Owin;
using System.Collections.Specialized;
using Ninject;
using FSO.Server.Domain;

using static FSO.Server.Common.ApiAbstract;

namespace FSO.Server.Servers.UserApi
{
    public class UserApi : AbstractServer
    {
        private IDisposable App;
        public ServerConfiguration Config;
        private IKernel Kernel;

        public event APIRequestShutdownDelegate OnRequestShutdown;
        public event APIBroadcastMessageDelegate OnBroadcastMessage;
        public event APIRequestUserDisconnectDelegate OnRequestUserDisconnect;
        public event APIRequestMailNotifyDelegate OnRequestMailNotify;

        public UserApi(ServerConfiguration config, IKernel kernel)
        {
            this.Config = config;
            this.Kernel = kernel;
        }

        public override void AttachDebugger(IServerDebugger debugger)
        {
        }

        public override void Shutdown()
        {
            APIThread?.Stop();
        }

        private IAPILifetime APIThread;

        public static Func<UserApi, string, IAPILifetime> CustomStartup;
        public override void Start()
        {
            // Start OWIN host
            if (CustomStartup != null) APIThread = CustomStartup(this, Config.Services.UserApi.Bindings[0]);
            else
            {
                App = WebApp.Start(Config.Services.UserApi.Bindings[0], x =>
                {
                    new UserApiStartup().Configuration(x, Config);
                    SetupInstance(INSTANCE);
                    ((FSO.Server.Api.Api)INSTANCE).HostPool = Kernel.Get<IGluonHostPool>();
                });
            }
        }

        public IGluonHostPool GetGluonHostPool()
        {
            return Kernel.Get<IGluonHostPool>();
        }

        public void SetupInstance(ApiAbstract api)
        {
            api.OnBroadcastMessage += (s, t, m) => { OnBroadcastMessage?.Invoke(s, t, m); };
            api.OnRequestShutdown += (t, st) => { OnRequestShutdown?.Invoke(t, st); };
            api.OnRequestUserDisconnect += (i) => { OnRequestUserDisconnect?.Invoke(i); };
            api.OnRequestMailNotify += (i, s, b, t) => { OnRequestMailNotify?.Invoke(i, s, b, t); };
            
            var config = Config;
        }
    }

    public class UserApiStartup
    {
        public void Configuration(IAppBuilder builder, ServerConfiguration config)
        {
            HttpConfiguration http = new HttpConfiguration();
            //WebApiConfig.Register(http);

            var userApiConfig = config.Services.UserApi;

            var settings = new NameValueCollection
            {
                { "maintainance", userApiConfig.Maintainance.ToString() },
                { "authTicketDuration", userApiConfig.AuthTicketDuration.ToString() },
                { "regkey", userApiConfig.Regkey },
                { "secret", config.Secret },
                { "updateUrl", userApiConfig.UpdateUrl },
                { "cdnUrl", userApiConfig.CDNUrl },
                { "connectionString", config.Database.ConnectionString },
                { "NFSdir", config.SimNFS },
                { "smtpHost", userApiConfig.SmtpHost },
                { "smtpUser", userApiConfig.SmtpUser },
                { "smtpPassword", userApiConfig.SmtpPassword },
                { "smtpPort", userApiConfig.SmtpPort.ToString() },
                { "useProxy", userApiConfig.UseProxy.ToString() }
            };

            //var api = new FSO.Server.Api.Api();
            //api.Init(settings);

            builder.UseWebApi(http);
        }
    }
}
