using CommandLine;
using FSO.Server.Database;
using FSO.Server.DataService;
using FSO.Server.Utils;
using Ninject;
using Ninject.Parameters;
using System;
using System.Reflection;

namespace FSO.Server
{
    public class Program
    {
        public static int Main(string[] args)
        {
            Type toolType = null;
            object toolOptions = null;

            string[] a2 = args;
            if (args.Length == 0) a2 = ["--run"];
            switch (a2[0])
            {
                case "--run":
                    toolType = typeof(ToolRunServer);
                    //toolOptions = subOptions;
                    break;
                case "--db-init":
                    toolType = typeof(ToolInitDatabase);
                    //toolOptions = subOptions;
                    break;
                case "--import-nhood":
                    toolType = typeof(ToolImportNhood);
                    //toolOptions = subOptions;
                    break;
                case "--restore-lots":
                    toolType = typeof(ToolRestoreLots);
                    //toolOptions = subOptions;
                    break;
                default:
                    Console.WriteLine("Commands:");
                    Console.WriteLine("  --run");
                    Console.WriteLine("  --db-init");
                    Console.WriteLine("  --import-nhood");
                    Console.WriteLine("  --restore-lots");
                    Environment.Exit(1);
                    break;
            }

            /*
            var result = CommandLine.Parser.Default.ParseArguments<ProgramOptions>(a2)
                .WithParsed(options => {
                    switch (options.ToString())
                    {
                        case "run":
                            toolType = typeof(ToolRunServer);
                            //toolOptions = subOptions;
                            break;
                        case "db-init":
                            toolType = typeof(ToolInitDatabase);
                            //toolOptions = subOptions;
                            break;
                        case "import-nhood":
                            toolType = typeof(ToolImportNhood);
                            //toolOptions = subOptions;
                            break;
                        case "restore-lots":
                            toolType = typeof(ToolRestoreLots);
                            //toolOptions = subOptions;
                            break;
                        default:
                            Console.WriteLine("Use --help");
                            Environment.Exit(1);
                            break;
                    }
                })
                .WithNotParsed(action => {
                    Environment.Exit(1);
                });
            */
 

            if (toolType == null)
            {
                Environment.Exit(1);
            }

            var kernel = new StandardKernel(
                new ServerConfigurationModule(),
                new DatabaseModule(),
                new GlobalDataServiceModule(),
                new GluonHostPoolModule()
            );

            //If db init, allow @ variables in the query itself. We could always enable this but for added security
            //we are conditionally adding it only for db migrations
            if (toolType == typeof(ToolInitDatabase))
            {
                var config = kernel.Get<ServerConfiguration>();
                if (!config.Database.ConnectionString.EndsWith(";")){
                    config.Database.ConnectionString += ";";
                }
                config.Database.ConnectionString += "Allow User Variables=True";
            }

            var tool = (ITool)kernel.Get(toolType, new ConstructorArgument("options", toolOptions));
            return tool.Run();

        }
    }
}
