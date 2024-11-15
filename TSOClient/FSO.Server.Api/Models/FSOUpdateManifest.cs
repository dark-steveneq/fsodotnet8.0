using FSO.Files.Utils;
using System.Collections.Generic;

namespace FSO.Server.Api.Models
{
    public class FSOUpdateManifest {
        public string Version;
        public List<FileDiff> Diffs;
    }
}
