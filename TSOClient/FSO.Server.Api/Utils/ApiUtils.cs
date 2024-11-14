﻿using System.Linq;
using System.Net.Http;
using System.ServiceModel.Channels;
using Microsoft.AspNetCore.Http;
using Microsoft.Owin;
using Ninject.Activation;

namespace FSO.Server.Api.Utils
{
    public class ApiUtils
    {
        private const string HttpContext = "MS_HttpContext";

        private const string RemoteEndpointMessage =
            "System.ServiceModel.Channels.RemoteEndpointMessageProperty";

        private const string OwinContext = "MS_OwinContext";

        public static string GetIP(HttpRequest request)
        {
            var api = Api.INSTANCE;
            if (!api.Config.UseProxy)
            {
                /*
                // Web-hosting
                if (request.Properties.ContainsKey(HttpContext))
                {
                    var ctx = (HttpContextWrapper)request.Properties[HttpContext];
                    if (ctx != null)
                    {
                        return ctx.Request.UserHostAddress;
                    }
                }

                // Self-hosting
                if (request.Properties.ContainsKey(RemoteEndpointMessage))
                {
                    var remoteEndpoint = (RemoteEndpointMessageProperty)request.Properties[RemoteEndpointMessage];
                    if (remoteEndpoint != null)
                    {
                        return remoteEndpoint.Address;
                    }
                }

                // Self-hosting using Owin
                if (request.Properties.ContainsKey(OwinContext))
                {
                    var owinContext = (OwinContext)request.Properties[OwinContext];
                    if (owinContext != null)
                    {
                        return owinContext.Request.RemoteIpAddress;
                    }
                }

                return "127.0.0.1";
                */

                return request.Host.Host.ToString();
            }
            /*
            else
            {
                var ip = "127.0.0.1";
                if (request.Request.Headers.ContainsKey("X-Forwarded-For"))
                {
                    ip = request.Request.For("X-Forwarded-For").First();
                    ip = ip.Substring(ip.IndexOf(",") + 1);
                    var last = ip.LastIndexOf(":");
                    if (last < ip.Length - 5) ip = ip.Substring(0, last);
                }

                return ip;
            }
            */
            else
            {
                return "127.0.0.1";
            }
        }
    }
}