using System;
using FSO.Server.Common;
using FSO.Server.Api;
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

        public override void Start()
        {
            var config = this.Config;
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
                { "useProxy", userApiConfig.UseProxy.ToString() },
                { "updateID", config.UpdateID?.ToString() ?? "" },
                { "branchName", config.UpdateBranch },
                { "siteName", config.Services.UserApi.SiteName }
            };
            

            var api = new FSO.Server.Api.Api();
            api.Init(settings);
            //if (userApiConfig.AwsConfig != null) api.UpdateUploader = new AWSUpdateUploader(userApiConfig.AwsConfig);
            //if (userApiConfig.GithubConfig != null) api.UpdateUploaderClient = new GithubUpdateUploader(userApiConfig.GithubConfig);
            //else api.UpdateUploaderClient = api.UpdateUploader;
            api.Github = userApiConfig.GithubConfig;
            api.HostPool = GetGluonHostPool();
            
            APIThread = FSO.Server.Api.Program.StartUserApi(Config.Services.UserApi.Bindings.ToArray());
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
}
