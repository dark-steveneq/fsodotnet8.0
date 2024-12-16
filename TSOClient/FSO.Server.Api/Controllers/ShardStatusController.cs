using FSO.Common.Utils;
using FSO.Server.Api.Utils;
using FSO.Server.Protocol.CitySelector;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;

namespace FSO.Server.Api.Controllers
{
    [EnableCors]
    [Route("cityselector/shard-status.jsp")]
    [ApiController]
    public class ShardStatusController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {

            var api = Api.INSTANCE;

            var result = new XMLList<ShardStatusItem>("Shard-Status-List");
            var shards = api.Shards.All;
            var host = Request.Host.Host;
            if (host == "localhost")
                host = "127.0.0.1";
            foreach (var shard in shards)
            {
                if (api.Config.SingleNode)
                    shard.PublicHost = host + shard.PublicHost[shard.PublicHost.IndexOf(":")..];
                else if (api.Config.Rewrites != null && api.Config.Rewrites.ContainsKey(host))
                {
                    foreach(KeyValuePair<string, string> rule in api.Config.Rewrites[host])
                    {
                        if (shard.PublicHost.Contains(rule.Key))
                        {
                            shard.PublicHost = shard.PublicHost.Replace(rule.Key, rule.Value);
                            break;
                        }
                    }
                }
                result.Add(shard);
            }
            return ApiResponse.Xml(HttpStatusCode.OK, result);
        }
    }
}
