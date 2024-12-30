using FSO.Server.Common;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace FSO.Server.Servers.Api.JsonWebToken
{
    public class JWTInstance
    {
        public string Token;
        public int ExpiresIn;
    }

    public class JWTFactory
    {
        private JWTConfiguration Config;

        public JWTFactory(JWTConfiguration config)
        {
            this.Config = config;
        }

        public JWTUser DecodeToken(string token)
        {
            try
            {
                //var payload = JWT.JsonWebToken.Decode(token, Config.Key, true);
                var payload = JWT.Builder.JwtBuilder.Create()
                    .WithAlgorithm(new JWT.Algorithms.HMACSHA384Algorithm())
                    .WithSecret(Config.Key)
                    .MustVerifySignature()
                    .Decode(token);
                Dictionary<string, string> payloadParsed = JsonConvert.DeserializeObject<Dictionary<string, string>>(payload);
                return JsonConvert.DeserializeObject<JWTUser>(payloadParsed["data"]);     
            }
            catch
            {
                return default;
            }
        }

        public JWTInstance CreateToken(JWTUser data)
        {
            var tokenData = JsonConvert.SerializeObject(data);
            return CreateToken(tokenData, Config.TokenDuration);
        }

        private JWTInstance CreateToken(string data, int expiresIn)
        {
            var expires = Epoch.Now + expiresIn;
            /*
            var payload = new Dictionary<string, object>()
            {
                { "exp", expires },
                { "data", data }
            };
            var token = JWT.JsonWebToken.Encode(payload, Config.Key, JWT.JwtHashAlgorithm.HS384);
            */
            var token = JWT.Builder.JwtBuilder.Create()
                .WithAlgorithm(new JWT.Algorithms.HMACSHA384Algorithm())
                .WithSecret(Config.Key)
                .AddClaim("exp", expires)
                .AddClaim("data", data)
                .Encode();
            return new JWTInstance { Token = token, ExpiresIn = expiresIn };
        }
    }
}
