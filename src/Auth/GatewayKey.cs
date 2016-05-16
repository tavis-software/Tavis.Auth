using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Tavis.Auth
{
    public class GatewayKey : HttpCredentials
    {
        private string _Key;

        public GatewayKey(Uri originServer, string gatewayid, string key ) : base("",originServer)
        {
            _Key = key;
            GatewayId = gatewayid;
        }
        
        public override string CreateGatewayHeaderValue(HttpRequestMessage request)
        {
            return _Key;
        }
    }
}
