using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Tavis.Auth
{
    public abstract class HttpCredentials     
    {
        public Uri OriginServer { get; private set; }

        public string GatewayId { get; set; }

        public int Priority { get; set; }
        public string AuthScheme { get; private set; }
        public string Realm { get; set; }
        public string LastChallengeParameters { get; set; }

        virtual public AuthenticationHeaderValue CreateAuthHeader(HttpRequestMessage request) {
            return null;
        }

        virtual public string CreateGatewayHeaderValue(HttpRequestMessage request)
        {
            return null;
        }
        protected HttpCredentials(string authScheme, Uri originServer)
        {
            AuthScheme = authScheme;
            OriginServer = originServer;
        }
    }
}