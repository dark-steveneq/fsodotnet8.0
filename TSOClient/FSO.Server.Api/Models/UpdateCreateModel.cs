﻿namespace FSO.Server.Api.Models
{
    public class UpdateCreateModel
    {
        public int branchID;
        public uint scheduledEpoch;
        public string catalog;

        public bool contentOnly;
        public bool includeMonogameDelta;
        public bool disableIncremental;
        public bool minorVersion;
    }
}