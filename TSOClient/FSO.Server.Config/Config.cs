using System;
using System.IO;
using System.Linq;
using System.Reflection;
using SharpYaml.Serialization;

namespace FSO.Server.Config
{
    public static class Config
    {
        public static string Version;
        public static string TSOPath;
        public static string Secret;
        public static string Database;
        public static string UserContentPath;
        public static string UpdateURL;
        public static Services Services;

        public static bool LoadConfig()
        {
            var configPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config.yml");
            if (!File.Exists(configPath))
                configPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config.yaml");
            if (File.Exists(configPath))
            {
                try
                {
                    var config = new Serializer().Deserialize<YamlConfig>(File.ReadAllText(configPath));
                    Version = config.version;
                    TSOPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), config.TSOPath);
                    Secret = config.secret;
                    Database = config.database;
                    UserContentPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), config.userContentPath);
                    UpdateURL = config.updateURL;
                    Services = new Services()
                    {
                        Tasks = {
                            Enabled = config.services.tasks.enabled,
                            CallSign = config.services.tasks.callSign,
                            Binding = config.services.tasks.binding,
                            HostInternal = config.services.tasks.hostInternal,
                            HostPublic = config.services.tasks.hostPublic,
                            Tasks = []
                        },
                        UserAPI = {
                            Enabled = config.services.userAPI.enabled,
                            Bindings = config.services.userAPI.bindings
                        },
                        Cities = [],
                        Lots = []
                    };
                    foreach (YamlServicesTasksTask task in config.services.tasks.tasks)
                    {
                        Services.Tasks.Tasks.Append(new(){
                            Cron = task.cron,
                            Task = task.task,
                            Timeout = task.timeout
                        });
                    }
                    foreach (YamlServicesCities city in config.services.cities)
                    {
                        Services.Cities.Append(new(){
                            CallSign = city.callSign,
                            ID = city.id,
                            Binding = city.binding,
                            HostInternal = city.hostInternal,
                            HostPublic = city.hostPublic,
                            Maintenance = {
                                Cron = city.maintenance.cron,
                                Timeout = city.maintenance.timeout,
                                Top100AveragePeriod = city.maintenance.top100AveragePeriod,
                                VisitsRetentionPeriod = city.maintenance.visitsRetentionPeriod
                            }
                        });
                    }
                    foreach (YamlServicesLots lot in config.services.lots)
                    {
                        Services.Lots.Append(new(){
                            CallSign = lot.callSign,
                            Binding = lot.binding,
                            HostInternal = lot.hostInternal,
                            HostPublic = lot.hostPublic,
                            MaxLots = lot.maxLots,
                            Cities = []
                        });
                        foreach (YamlServicesLotsCity city in lot.cities)
                        {
                            Services.Lots.Last().Cities.Append(new(){
                                ID = city.id,
                                Host = city.host
                            });
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to deserialize config.yml", ex);
                }
            }
            else
                configPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config.json");
            if (File.Exists(configPath))
            {
                try
                {
                    var old = Newtonsoft.Json.JsonConvert.DeserializeObject<JSONConfig>(File.ReadAllText(configPath));
                    var config = new YamlConfig()
                    {
                        version = "dev-0",
                        TSOPath = old.gameLocation,
                        secret = old.secret,
                        database = old.database.ConvertDatabaseConfig(),
                        userContentPath = old.simNFS,
                        updateURL = old.services.userApi.updateUrl,
                        services = {
                            tasks = {
                                enabled = old.services.tasks.enabled,
                                callSign = old.services.tasks.call_sign,
                                binding = old.services.tasks.binding,
                                hostInternal = old.services.tasks.internal_host,
                                hostPublic = old.services.tasks.public_host,
                                tasks = []
                            },
                            userAPI = {
                                enabled = old.services.userApi.enabled,
                                bindings = old.services.userApi.bindings
                            },
                            cities = [],
                            lots = []
                        }
                    };
                    foreach (JSONServicesTasksTask task in old.services.tasks.schedule)
                    {
                        config.services.tasks.tasks.Append(new(){
                            cron = task.cron,
                            task = task.task,
                            timeout = task.timeout
                        });
                    }
                    foreach (JSONServicesCities city in old.services.cities)
                    {
                        config.services.cities.Append(new(){
                            callSign = city.call_sign,
                            id = city.id,
                            binding = city.binding,
                            hostInternal = city.internal_host,
                            hostPublic = city.public_host,
                            maintenance = {
                                cron = city.maintenance.cron,
                                timeout = city.maintenance.timeout,
                                top100AveragePeriod = city.maintenance.top100_average_period,
                                visitsRetentionPeriod = city.maintenance.visits_retention_period
                            }
                        });
                    }
                    foreach (JSONServicesLots lot in old.services.lots)
                    {
                        config.services.lots.Append(new(){
                            callSign = lot.call_sign,
                            binding = lot.binding,
                            hostInternal = lot.internal_host,
                            hostPublic = lot.public_host,
                            maxLots = lot.max_lots,
                            cities = []
                        });
                        foreach (JSONServicesLotsCity city in lot.cities)
                        {
                            config.services.lots.Last().cities.Append(new(){
                                id = city.id,
                                host = city.host
                            });
                        }
                    }
                    try
                    {
                        File.WriteAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config.yml"), new Serializer().Serialize(config));
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Failed to write migrated config.yml", ex);
                    }
                    return LoadConfig();
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to deserialize config.json", ex);
                }
            }
            return false;
        }
    }
}
