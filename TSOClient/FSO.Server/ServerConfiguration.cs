using FSO.Server.Database;
using FSO.Server.Discord;
using FSO.Server.Servers.Api.JsonWebToken;
using FSO.Server.Servers.City;
using FSO.Server.Servers.Lot;
using FSO.Server.Servers.Tasks;
using FSO.Server.Servers.UserApi;
using Microsoft.Win32;
using Ninject.Activation;
using Ninject.Modules;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

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
        private static Logger LOG = LogManager.GetCurrentClassLogger();

        private ServerConfiguration GetConfiguration(IContext context)
        {
            //TODO: Allow config path to be overriden in a switch
            var configPath = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "config.json");
            if (!File.Exists(configPath))
            {
                if (!File.Exists(Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "config.sample.json")))
                    LOG.Fatal("File config.json doesn't exist, rename config.sample.json to config.json and edit it!");
                else
                    LOG.Fatal("File config.json doesn't exist, refer to documentation!");
                Environment.Exit(1);
            }

            var data = File.ReadAllText(configPath);

            try
            {
                var parsed = Newtonsoft.Json.JsonConvert.DeserializeObject<ServerConfiguration>(data);

                var parentPath = Path.GetDirectoryName(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));
                if (File.Exists(Path.Combine(parentPath, "game", "tuning.dat")))
                {
                    parsed.GameLocation = Path.Combine(parentPath, "game");
                    LOG.Info("Found a valid TSO instalation in {0}, using it instead!", parsed.GameLocation);
                }
                else if (File.Exists(Path.Combine(parentPath, "The Sims Online", "TSOClient", "tuning.dat")))
                {
                    parsed.GameLocation = Path.Combine(parentPath, "The Sims Online", "TSOClient");
                    LOG.Info("Found a valid TSO instalation in {0}, using it instead!", parsed.GameLocation);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                    {
                        RegistryKey softwareKey = hklm.OpenSubKey("SOFTWARE");
                        if (Array.Exists(softwareKey.GetSubKeyNames(), delegate (string s) { return s.Equals("Maxis", StringComparison.InvariantCultureIgnoreCase); }))
                        {
                            RegistryKey maxisKey = softwareKey.OpenSubKey("Maxis");
                            if (Array.Exists(maxisKey.GetSubKeyNames(), delegate (string s) { return s.Equals("The Sims Online", StringComparison.InvariantCultureIgnoreCase); }))
                            {
                                parsed.GameLocation = Path.Combine((string)maxisKey.OpenSubKey("The Sims Online").GetValue("InstallDir"), "TSOClient");
                                LOG.Info("Found a valid TSO instalation in {0}, using it instead!", parsed.GameLocation);
                            }
                        }
                    }

                return parsed;
            }
            catch (Exception ex)
            {
                LOG.Fatal(ex, "Couldn't read config.json! Check config.json for formatting issues!");
                Environment.Exit(1);
                return null;
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
