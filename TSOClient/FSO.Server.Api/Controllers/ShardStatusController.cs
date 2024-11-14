﻿using FSO.Common.Utils;
using FSO.Server.Api.Utils;
using FSO.Server.Protocol.CitySelector;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;

namespace FSO.Server.Api.Controllers
{
    public class ShardStatusController : ControllerBase
    {
        public HttpResponseMessage Get()
        {
            var api = Api.INSTANCE;

            var result = new XMLList<ShardStatusItem>("Shard-Status-List");
            var shards = api.Shards.All;
            foreach (var shard in shards)
            {
                result.Add(shard);
            }
            return ApiResponse.Xml(HttpStatusCode.OK, result);
        }
    }
}