using FSO.Common.Utils;
using FSO.Server.Api.Utils;
using FSO.Server.Protocol.CitySelector;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
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
            foreach (var shard in shards)
            {
                shard.PublicHost = host + shard.PublicHost[shard.PublicHost.IndexOf(":")..];
                result.Add(shard);
            }
            return ApiResponse.Xml(HttpStatusCode.OK, result);
        }
    }
}