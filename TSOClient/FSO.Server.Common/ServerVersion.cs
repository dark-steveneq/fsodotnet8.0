using System.IO;
using System.Reflection;

namespace FSO.Server.Common
{
    public class ServerVersion
    {
        public string Name;
        public string Number;
        public int? UpdateID;

        public static ServerVersion Get()
        {
            var result = new ServerVersion()
            {
                Name = "unknown",
                Number = "0"
            };

            var versionPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "version.txt");
            if (File.Exists(versionPath))
            {
                using (StreamReader Reader = new StreamReader(File.Open(versionPath, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    var str = Reader.ReadLine();
                    var split = str.LastIndexOf('-');

                    result.Name = str;
                    if (split != -1)
                    {
                        result.Name = str.Substring(0, split);
                        result.Number = str.Substring(split + 1);
                    }
                }
            }

            if (File.Exists("updateID.txt"))
            {
                var stringID = File.ReadAllText("updateID.txt");
                int id;
                if (int.TryParse(stringID, out id))
                {
                    result.UpdateID = id;
                }
            }

            return result;
        }
    }
}
