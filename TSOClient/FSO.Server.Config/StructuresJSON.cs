namespace FSO.Server.Config
{
    internal class JSONConfig
    {
        public string gameLocation;
        public string secret;
        public string simNFS;
        public JSONDatabase database;
        public JSONServices services;
    }

    internal class JSONDatabase
    {
        public string connectionString;

        public string ConvertDatabaseConfig()
        {
            if (!connectionString.EndsWith(";"))
                connectionString += ";";
            string uid = "";
            string pwd = "";
            string server = "";
            string database = "";
            foreach(string argument in connectionString.Split(";"))
            {
                int equals = argument.IndexOf("=");
                if (argument == "")
                    break;
                else if (equals < argument.Length)
                {
                    string value = argument[(equals + 1)..];
                    switch (argument[..equals])
                    {
                    case "server":
                        server = value;
                        break;
                    case "uid":
                        uid = value;
                        break;
                    case "pwd":
                        pwd = value;
                        break;
                    case "database":
                        database = value;
                        break;
                    }
                }
                else if (equals == -1)
                    throw new System.Exception("Malformed legacy database configuration");
            }
            string output = "mysql://";
            if (uid != "" || pwd != "")
                output += uid + ":" + pwd + "@";
            return output + server + "/" + database;
        }
    }

    internal class JSONServices
    {
        public JSONServicesTasks tasks;
        public JSONServicesUserApi userApi;
        public JSONServicesCities[] cities;
        public JSONServicesLots[] lots;
    }


    // Services/Tasks
    internal class JSONServicesTasks
    {
        public bool enabled;
        public string call_sign;
        public string binding;
        public string internal_host;
        public string public_host;
        public JSONServicesTasksTask[] schedule;
    }

    internal class JSONServicesTasksTask
    {
        public string cron;
        public string task;
        public int timeout;
    }


    // Services/UserAPI
    internal class JSONServicesUserApi
    {
        public bool enabled;
        public string[] bindings;
        public string updateUrl;
    }


    // Services/Cities
    internal class JSONServicesCities
    {
        public string call_sign;
        public int id;
        public string binding;
        public string internal_host;
        public string public_host;
        public JSONServicesCitiesMaintenance maintenance;
    }

    internal class JSONServicesCitiesMaintenance
    {
        public string cron;
        public int timeout;
        public int visits_retention_period;
        public int top100_average_period;
    }


    // Services/Lots
    internal class JSONServicesLots
    {
        public string call_sign;
        public string binding;
        public string internal_host;
        public string public_host;
        public int max_lots;

        public JSONServicesLotsCity[] cities;
    }

    internal class JSONServicesLotsCity
    {
        public int id;
        public string host;
    }
}