using FSO.Common.Utils;

namespace FSO.Server.Common
{
    public static class RewriteCache
    {
        public static TimedReferenceCache<uint, string> Rewrites;

        static RewriteCache()
        {
            Rewrites = new();
        }
    }
}
