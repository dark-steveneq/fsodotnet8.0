namespace FSO.Server.Config
{
    public class Services
    {
        public ServicesTasks Tasks;
        public ServicesUserAPI UserAPI;
        public ServicesCities[] Cities;
        public ServicesLots[] Lots;
    }


    // Services/Tasks
    public class ServicesTasks
    {
        public bool Enabled;
        public string CallSign;
        public string Binding;
        public string HostInternal;
        public string HostPublic;
        public ServicesTasksTask[] Tasks;
    }

    public class ServicesTasksTask
    {
        public string Cron;
        public string Task;
        public int Timeout;
    }


    // Services/UserAPI
    public class ServicesUserAPI
    {
        public bool Enabled;
        public string[] Bindings;
    }


    // Services/Cities
    public class ServicesCities
    {
        public string CallSign;
        public int ID;
        public string Binding;
        public string HostInternal;
        public string HostPublic;
        public ServicesCitiesMaintenance Maintenance;
    }

    public class ServicesCitiesMaintenance
    {
        public string Cron;
        public int Timeout;
        public int VisitsRetentionPeriod;
        public int Top100AveragePeriod;
    }


    // Services/Lots
    public class ServicesLots
    {
        public string CallSign;
        public string Binding;
        public string HostInternal;
        public string HostPublic;
        public int MaxLots;

        public ServicesLotsCity[] Cities;
    }

    public class ServicesLotsCity
    {
        public int ID;
        public string Host;
    }
}