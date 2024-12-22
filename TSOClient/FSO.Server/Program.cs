using FSO.Server.Database;
using FSO.Server.DataService;
using FSO.Server.Utils;
using Ninject;
using Ninject.Parameters;
using System;
using CommandLine;

namespace FSO.Server
{
    public class Program
    {
        /// <summary>
        /// Entrypoint of the server, decides which tool to run
        /// </summary>
        /// <param name="args">Command-line arguments</param>
        /// <returns></returns>
        public static int Main(string[] args)
        {
            Type toolType = null;
            object toolOptions = null;

            string[] a2 = args;
            if (args.Length == 0)
                a2 = ["run"];

            var parser = new Parser(with =>
            {
                with.EnableDashDash = true;
                with.HelpWriter = Console.Out;
            });
            var result = parser.ParseArguments<RunServerOptions, DatabaseInitOptions, ImportNhoodOptions, RestoreLotsOptions>(a2)
                .WithParsed<RunServerOptions>(options =>
                {
                    toolType = typeof(ToolRunServer);
                    toolOptions = options;
                })
                .WithParsed<DatabaseInitOptions>(options =>
                {
                    toolType = typeof(DatabaseInitOptions);
                    toolOptions = options;
                })
                .WithParsed<ImportNhoodOptions>(options =>
                {
                    toolType = typeof(ImportNhoodOptions);
                    toolOptions = options;
                })
                .WithParsed<RestoreLotsOptions>(options =>
                {
                    toolType = typeof(RestoreLotsOptions);
                    toolOptions = options;
                });


            if (toolType == null)
                Environment.Exit(1);

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
