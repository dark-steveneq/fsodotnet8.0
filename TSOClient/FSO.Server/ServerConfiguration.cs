using FSO.Server.Database;
using FSO.Server.Discord;
using FSO.Server.Servers.Api.JsonWebToken;
using FSO.Server.Servers.City;
using FSO.Server.Servers.Lot;
using FSO.Server.Servers.Tasks;
using FSO.Server.Servers.UserApi;
using Ninject.Activation;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace FSO.Server
{
    /// <summary>
    /// Server Configuration Parsed from config.json
    /// </summary>
    public class ServerConfiguration
    {
        /// <summary>
        /// Location of The Sims Online Assets
        /// </summary>
        public string GameLocation;
        /// <summary>
        /// Location of User-Generated Content (Lots, Objects)
        /// </summary>
        public string SimNFS;
        /// <summary>
        /// Version branch
        /// </summary>
        public string UpdateBranch;

        /// <summary>
        /// Database configuration
        /// </summary>
        public DatabaseConfiguration Database;
        /// <summary>
        /// Service configuration
        /// </summary>
        public ServerConfigurationservices Services;
        /// <summary>
        /// Discord RPC configuration
        /// </summary>
        public DiscordConfiguration Discord;

        /// <summary>
        /// Secret string used as a key for signing JWT tokens for the admin system
        /// </summary>
        public string Secret;

        /// <summary>
        /// Update ID this server is running on. All shards that we host will report needing this version, and this is reported with our host information.
        /// Loaded from updateID.txt if present.
        /// </summary>
        public int? UpdateID;
    }


    public class ServerConfigurationservices
    {
        public ApiServerConfiguration UserApi;
        public TaskServerConfiguration Tasks;
        public List<CityServerConfiguration> Cities;
        public List<LotServerConfiguration> Lots;
    }

    
    /// <summary>
    /// Ninject module which parses config.json
    /// </summary>
    public class ServerConfigurationModule : NinjectModule
    {
        private ServerConfiguration GetConfiguration(IContext context)
        {
            //TODO: Allow config path to be overriden in a switch
            var configPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config.json");
            if (!File.Exists(configPath))
            {
                Console.WriteLine("Configuration file path: " + configPath);
                throw new Exception("Configuration file, config.json, missing");
            }

            var data = File.ReadAllText(configPath);

            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<ServerConfiguration>(data);
            }catch(Exception ex)
            {
                throw new Exception("Could not deserialize config.json", ex);
            }
        }

        private class DatabaseConfigurationProvider : IProvider<DatabaseConfiguration>
        {
            private ServerConfiguration Config;

            public DatabaseConfigurationProvider(ServerConfiguration config)
            {
                this.Config = config;    
            }


            public Type Type
            {
                get
                {
                    return typeof(DatabaseConfiguration);
                }
            }

            public object Create(IContext context)
            {
                return this.Config.Database;
            }
        }


        private class JWTConfigurationProvider : IProvider<JWTConfiguration>
        {
            private ServerConfiguration Config;

            public JWTConfigurationProvider(ServerConfiguration config)
            {
                this.Config = config;
            }


            public Type Type
            {
                get
                {
                    return typeof(JWTConfiguration);
                }
            }

            public object Create(IContext context)
            {
                return new JWTConfiguration() {
                    Key = System.Text.UTF8Encoding.UTF8.GetBytes(Config.Secret)
                };
            }
        }

        public override void Load()
        {
            this.Bind<ServerConfiguration>().ToMethod(new Func<Ninject.Activation.IContext, ServerConfiguration>(GetConfiguration)).InSingletonScope();
            this.Bind<DatabaseConfiguration>().ToProvider<DatabaseConfigurationProvider>().InSingletonScope();
            this.Bind<JWTConfiguration>().ToProvider<JWTConfigurationProvider>().InSingletonScope();
        }
    }
}
