namespace FSO.Server.Config
{
    internal class YamlConfig
    {
        public string version;
        public string TSOPath;
        public string secret;
        public string database;
        public string userContentPath;
        public string updateURL;
        public YamlServices services;
    }

    internal class YamlServices
    {
        public YamlServicesTasks tasks;
        public YamlServicesUserAPI userAPI;
        public YamlServicesCities[] cities;
        public YamlServicesLots[] lots;
    }


    // Services/Tasks
    internal class YamlServicesTasks
    {
        public bool enabled;
        public string callSign;
        public string binding;
        public string hostInternal;
        public string hostPublic;
        public YamlServicesTasksTask[] tasks;
    }

    internal class YamlServicesTasksTask
    {
        public string cron;
        public string task;
        public int timeout;
    }


    // Services/UserAPI
    internal class YamlServicesUserAPI
    {
        public bool enabled;
        public string[] bindings;
    }


    // Services/Cities
    internal class YamlServicesCities
    {
        public string callSign;
        public int id;
        public string binding;
        public string hostInternal;
        public string hostPublic;
        public YamlServicesCitiesMaintenance maintenance;
    }

    internal class YamlServicesCitiesMaintenance
    {
        public string cron;
        public int timeout;
        public int visitsRetentionPeriod;
        public int top100AveragePeriod;
    }


    // Services/Lots
    internal class YamlServicesLots
    {
        public string callSign;
        public string binding;
        public string hostInternal;
        public string hostPublic;
        public int maxLots;

        public YamlServicesLotsCity[] cities;
    }

    internal class YamlServicesLotsCity
    {
        public int id;
        public string host;
    }
}