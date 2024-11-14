using CommandLine;
using CommandLine.Text;

namespace FSO.Server
{
    public class ProgramOptions
    {
        [Option("run", HelpText = "Run the servers configured in config.json")]
        public RunServerOptions RunServerVerb { get; set; }

        [Option("db-init", HelpText = "Initialize the database.")]
        public DatabaseInitOptions DatabaseMaintenanceVerb { get; set; }

        [Option("import-nhood",
            HelpText = "Import the neighborhood stored in the given JSON file to the specified shard.")]
        public ImportNhoodOptions ImportNhoodVerb { get; set; }

        [Option("restore-lots",
            HelpText = "Create lots in the database from FSOV saves in the specified folder. (with specified shard)")]
        public RestoreLotsOptions RestoreLotsVerb { get; set; }
    }

    public class DatabaseInitOptions
    {
    }
    

    public class RunServerOptions
    {
        [Option('d', "debug", Default = false, HelpText = "Launches a network debug interface")]
        public bool Debug { get; set; }
    }

    public class ImportNhoodOptions
    {
        [Value(0)]
        public int ShardId { get; set; }
        [Value(1)]
        public string JSON { get; set; }
    }

    public class RestoreLotsOptions
    {
        [Value(0)]
        public int ShardId { get; set; }
        [Value(1)]
        public string RestoreFolder { get; set; }

        [Option('r', "report", Default = false, HelpText = "Report changes that would be made restoring the lot, " +
            "eg. add/remove/reown of objects, lot positon (and if we can restore it) ")]
        public bool Report { get; set; }

        [Option('o', "objects", Default = false, HelpText = "Create new database entries for objects when they are still owned. " +
            "If 'safe' is enabled, then database entries will be created for objects on other lots, otherwise they will be created for all.")]
        public bool Objects { get; set; }

        [Option('s', "safe", Default = false, HelpText = "Do not return objects that have been placed, only ones in inventories.")]
        public bool Safe { get; set; }
    }
}
