using System;
using System.Net.Http;
using System.Net.Http.Headers;


namespace Tavis.Auth.Tests
{
    public class FooCredentials : HttpCredentials
    {
        public string Password { get; set; }

        public FooCredentials(Uri originServer) :  base("fooauth",originServer)
        {
        }

        public override AuthenticationHeaderValue CreateAuthHeader(HttpRequestMessage request)
        {
            var password = LastChallengeParameters.Split('=')[1];
            return new AuthenticationHeaderValue(AuthScheme, password);
        }
    }
}