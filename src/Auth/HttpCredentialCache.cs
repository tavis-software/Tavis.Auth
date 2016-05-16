using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace Tavis.Auth
{
    public class HttpCredentialCache : IEnumerable<HttpCredentials>
    {
        private readonly Dictionary<string,HttpCredentials> _Credentials = new Dictionary<string,HttpCredentials>(StringComparer.CurrentCultureIgnoreCase);
        private Regex _RealmParser = new Regex("Realm=\"(?<Realm>.*)\"",RegexOptions.IgnoreCase);
        public void Add(HttpCredentials info)
        {
            var key = MakeKey(info.OriginServer, info.Realm, info.AuthScheme);
            _Credentials.Add(key,info);    
        }

        private static string MakeKey(Uri originServer, string realm, string authScheme)
        {
            return originServer.AbsoluteUri + "|" + realm + "|" + authScheme;
        }

        public HttpCredentials GetMatchingCredentials(Uri originServer, IEnumerable<AuthenticationHeaderValue> challenges)
        {
            var matchingCredentials = from c in challenges
                                      let key = MakeKey(originServer, ParseRealm(c.Parameter), c.Scheme)
                                      let credentials = _Credentials.ContainsKey(key) ? _Credentials[key] : null
                                      where credentials != null
                                      orderby credentials.Priority
                                      select new { creds = credentials, challenge = c };

            var match = matchingCredentials.FirstOrDefault();
            if (match == null) return null;
            match.creds.LastChallengeParameters = match.challenge.Parameter;
            return match.creds;
        }

        public IEnumerable<HttpCredentials> GetGatewayCredentials(Uri requestUri)
        {
            var matching = from c in _Credentials.Values
                           where !String.IsNullOrEmpty(c.GatewayId) 
                                    && requestUri.AbsoluteUri.StartsWith(c.OriginServer.AbsoluteUri) 
                           select c;

            return matching;
        }

        private string ParseRealm(string parameters)
        {
            if (parameters == null) return "";
            var match = _RealmParser.Match(parameters);
            return match.Groups["Realm"].Value ?? "";
            
        }


        public IEnumerator<HttpCredentials> GetEnumerator()
        {
            return _Credentials.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}