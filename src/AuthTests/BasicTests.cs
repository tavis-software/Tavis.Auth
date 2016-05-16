using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tavis.Auth.Tests
{
    public class BasicTests
    {
        [Fact]
        public void Add_basic_credential_to_cache()
        {
            var cache = new HttpCredentialCache
            {
                new BasicCredentials(new Uri("http://example.org"), username: "", password: "")
            };

            Assert.Equal(1, cache.Count());
        }


        [Fact]
        public void Retreive_credentials_from_cache()
        {
            var basicCredentials = new BasicCredentials(new Uri("http://example.org"), username: "", password: "");
            var cache = new HttpCredentialCache
            {
                new FooCredentials(new Uri("http://example.org")),
                basicCredentials,
                new BasicCredentials(new Uri("http://example.net"), username: "", password: ""),
                new BasicCredentials(new Uri("http://example.org"), username: "", password: "") {Realm = "foo"},
            };


            var creds = cache.GetMatchingCredentials(new Uri("http://example.org"), new[] { new AuthenticationHeaderValue("basic","foo") });

            Assert.Same(basicCredentials, creds);
            
        }

        [Fact]
        public void Retreive_credentials_from_cache_with_trailing_slash()
        {
            var cache = new HttpCredentialCache
            {
                new FooCredentials(new Uri("http://example.org/"))
            };


            var creds = cache.GetMatchingCredentials(new Uri("http://example.org/"), new[] { new AuthenticationHeaderValue("fooauth", "asdasdasd") });

            Assert.NotNull(creds);

        }
        [Fact]
        public void Fail_to_find_creds()
        {
            var cache = new HttpCredentialCache
            {
                new FooCredentials(new Uri("http://example.org/"))
            };


            var creds = cache.GetMatchingCredentials(new Uri("http://example.org/"), new[] { new AuthenticationHeaderValue("someotherauth", "asdasdasd") });

            Assert.Null(creds);

        }

        [Fact]
        public void Fail_to_find_creds_because_no_challeng()
        {
            var cache = new HttpCredentialCache
            {
                new FooCredentials(new Uri("http://example.org/"))
            };


            var creds = cache.GetMatchingCredentials(new Uri("http://example.org/"), new AuthenticationHeaderValue[] { });

            Assert.Null(creds);

        }

        [Fact]
        public void Retreive_credentials_from_cache_using_realm()
        {
            var basicCredentials = new BasicCredentials(new Uri("http://example.org"), username: "", password: "")
            {
                Realm = "foo"   
            };
            var cache = new HttpCredentialCache
            {
                basicCredentials,
            };


            var creds = cache.GetMatchingCredentials(new Uri("http://example.org"), new[] { new AuthenticationHeaderValue("basic", "Realm=\"foo\"") });

            Assert.Same(basicCredentials, creds);
            
        }



        [Fact]
        public void Reuse_last_used_credentials()
        {
            // Arrange
            var cache = new HttpCredentialCache
            {
                new BasicCredentials(new Uri("http://example.org"), username: "", password: "")
            };

            var authService = new CredentialService(cache);
            var request = new HttpRequestMessage(){RequestUri = new Uri("http://example.org")};
            authService.CreateAuthenticationHeaderFromChallenge(request, new[] { new AuthenticationHeaderValue("basic", "") });

            // Act
            var header = authService.CreateAuthenticationHeaderFromRequest(request);

            // Assert
            Assert.Equal("basic", header.Scheme);

        }

        [Fact]
        public async Task Gateway_key()
        {
            // Arrange
            var cache = new HttpCredentialCache
            {
                new GatewayKey(new Uri("http://example.org"), "gateway-key","key value goes here"),
                new GatewayKey(new Uri("http://example.net"), "gateway-key2","key value goes here")
            };

            var authService = new CredentialService(cache);
            var request = new HttpRequestMessage() { RequestUri = new Uri("http://example.org/foo/bar") };
            var invoker = new HttpMessageInvoker(new AuthMessageHandler(new DummyHttpHandler(), authService));

            // Act
            var response = await invoker.SendAsync(request,new System.Threading.CancellationToken());

            // Assert
            
            Assert.True(request.Headers.Contains("gateway-key"));
            Assert.False(request.Headers.Contains("gateway-key2"));
            Assert.Equal("key value goes here", request.Headers.GetValues("gateway-key").First());
        }

        [Fact]
        public async Task Gateway_key_with_wrong_host()
        {
            // Arrange
            var cache = new HttpCredentialCache
            {
                new GatewayKey(new Uri("http://example.org/foo"), "gateway-key","key value goes here")
            };

            var authService = new CredentialService(cache);
            var request = new HttpRequestMessage() { RequestUri = new Uri("http://example.org") };
            var invoker = new HttpMessageInvoker(new AuthMessageHandler(new DummyHttpHandler(), authService));

            // Act
            var response = await invoker.SendAsync(request, new System.Threading.CancellationToken());

            // Assert

            Assert.False(request.Headers.Contains("gateway-key"));
            //Assert.Equal("key value goes here", request.Headers.GetValues("gateway-key").First());
        }
    }

    public class DummyHttpHandler : HttpMessageHandler
    {
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return new HttpResponseMessage() { RequestMessage = request };
        }
    }
}
