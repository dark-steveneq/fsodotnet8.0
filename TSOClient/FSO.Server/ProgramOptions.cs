using CommandLine;
using CommandLine.Text;

namespace FSO.Server
{
    [Verb("run", HelpText = "Run the servers configured in config.json")]
    public class RunServerOptions
    {
        [Option('d', "debug", Default = false, HelpText = "Launches a network debug interface")]
        public bool Debug { get; set; }
    }

    [Verb("db-init", HelpText = "Initialize the database.")]
    public class DatabaseInitOptions
    {}

    [Verb("import-nhood", HelpText = "Import the neighborhood stored in the given JSON file to the specified shard.")]
    public class ImportNhoodOptions
    {
        [Value(0, HelpText = "Shard/City to affect")]
        public int ShardId { get; set; }
        [Value(1, HelpText = "Path to json file you want to apply")]
        public string JSON { get; set; }
    }

    [Verb("restore-lots", HelpText = "Create lots in the database from FSOV saves in the specified folder. (with specified shard)")]
    public class RestoreLotsOptions
    {
        [Value(0, HelpText = "Shard/City you want to import to")]
        public int ShardId { get; set; }
        [Value(1, HelpText = "Folder with the lot to restaore")]
        public string RestoreFolder { get; set; }

        [Option('l', "location", Default = 0u, HelpText = "Override location to place the property.")]
        public uint Location { get; set; }

        [Option('t', "owner", Default = 0u, HelpText = "Override avatar id to own the property.")]
        public uint Owner { get; set; }

        [Option('c', "category", Default = -1, HelpText = "Override property category.")]
        public int Category { get; set; }

        [Option('r', "report", Default = false, HelpText = "Report changes that would be made restoring the lot, " +
            "eg. add/remove/reown of objects, lot positon (and if we can restore it) ")]
        public bool Report { get; set; }

        [Option('o', "objects", Default = false, HelpText = "Create new database entries for objects when they are still owned. " +
            "If 'safe' is enabled, then database entries will be created for objects on other lots, otherwise they will be created for all.")]
        public bool Objects { get; set; }

        [Option('d', "donate", Default = false, HelpText = "Convert all objects to donated so they don't have to belong to roommates.")]
        public bool Donate { get; set; }

        [Option('s', "safe", Default = false, HelpText = "Do not return objects that have been placed, only ones in inventories.")]
        public bool Safe { get; set; }
    }
}
