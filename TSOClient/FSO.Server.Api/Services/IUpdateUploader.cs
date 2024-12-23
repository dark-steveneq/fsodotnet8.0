﻿using System.Threading.Tasks;

namespace FSO.Server.Api.Services
{
    public interface IUpdateUploader
    {
        Task<string> UploadFile(string destPath, string fileName, string groupName);
    }
}
