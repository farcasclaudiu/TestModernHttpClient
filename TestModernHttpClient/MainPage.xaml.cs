using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ModernHttpClient;
using Xamarin.Forms;

namespace TestModernHttpClient
{
    public partial class MainPage : ContentPage
    {
        public const string TEST_URL = "https://mockhttpcalls.azurewebsites.net/api/values";

        public MainPage()
        {
            InitializeComponent();
        }

        async void Handle_Clicked(object sender, System.EventArgs e)
        {
            //native
            var response = await GetAsync(true, new Uri(TEST_URL), new CancellationToken());
            var onlyStr = (response > 1 ? "" : " ONLY ");
            lblResult1.Text = $"Got { onlyStr } {response} cookies back";
            //default httpclient
            response = await GetAsync(false, new Uri(TEST_URL), new CancellationToken());
            onlyStr = (response > 1 ? "" : " ONLY ");
            lblResult2.Text = $"Got { onlyStr } {response} cookies back";
        }


        private HttpClient CreateClient(bool useNative)
        {
            var cookieHandler = new NativeCookieHandler();
            var client =
                useNative ? new HttpClient(
                new NativeMessageHandler(false, false, cookieHandler)
                {
                    UseCookies = false,
                    DisableCaching = true,
                }
            ) :
                new HttpClient(
                        new HttpClientHandler
                        {
                            UseCookies = false
                        })
            ;
            client.MaxResponseContentBufferSize = 5000000000;
            client.Timeout = TimeSpan.FromSeconds(5);

            return client;
        }


        public async Task<int> GetAsync(bool useNative, Uri requestUri, CancellationToken cancellationToken)
        {
            using (var client = CreateClient(useNative))
            {
                client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
                using (var response = await client.GetAsync(requestUri, cancellationToken).ConfigureAwait(false))
                {
                    return HandleResponse(response);
                }
            }
        }

        protected int HandleResponse(HttpResponseMessage response)
        {
            if (response.Headers.Contains("Set-Cookie"))
            {
                return response.Headers.GetValues("Set-Cookie").Count();
            }
            return 0;
        }
    }
}
