using System;
using System.Globalization;
using System.Net;
using System.Net.Http;

namespace NHxD.Frontend.Winforms
{
	public class StaticHttpClient : IDisposable
	{
		private readonly HttpClient client;
		private readonly WebProxy webProxy;

		public HttpClient Client => client;
		public WebProxy WebProxy => webProxy;

		public StaticHttpClient(Configuration.ConfigNetwork networkSettings)
		{
			webProxy = GetWebProxy(networkSettings);

			if (!networkSettings.Offline)
			{
				client = new HttpClient(GetHttpClientHandler(networkSettings, webProxy), true);
			}
		}

		private static WebProxy GetWebProxy(Configuration.ConfigNetwork networkSettings)
		{
			WebProxy webProxy = new WebProxy();

			if (networkSettings.Client.Proxy.IsEnabled)
			{
				webProxy.Address = new Uri(string.Format(CultureInfo.InvariantCulture, "{0}:{1}", networkSettings.Client.Proxy.Address, networkSettings.Client.Proxy.Port));
				webProxy.BypassProxyOnLocal = networkSettings.Client.Proxy.BypassProxyOnLocal;
				webProxy.BypassList = networkSettings.Client.Proxy.BypassList;

				if (networkSettings.Client.Proxy.HasCredentials)
				{
					webProxy.UseDefaultCredentials = false;
					webProxy.Credentials = new NetworkCredential(networkSettings.Client.Proxy.Credentials.UserName, networkSettings.Client.Proxy.Credentials.Password);
				}
			}

			return webProxy;
		}

		private static HttpClientHandler GetHttpClientHandler(Configuration.ConfigNetwork networkSettings, WebProxy webProxy)
		{
			HttpClientHandler httpClientHandler = new HttpClientHandler();

			if (networkSettings.Client.Proxy.IsEnabled)
			{
				httpClientHandler.UseProxy = true;
				httpClientHandler.Proxy = webProxy;
			}

			if (networkSettings.Client.HasCredentials)
			{
				httpClientHandler.UseDefaultCredentials = false;
				httpClientHandler.Credentials = new NetworkCredential(networkSettings.Client.Credentials.UserName, networkSettings.Client.Credentials.Password);
			}

			//httpClientHandler.ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
			//httpClientHandler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls,
			//httpClientHandler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip

			return httpClientHandler;
		}

		#region IDisposable Support
		private bool disposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					if (client != null)
					{
						client.Dispose();
					}
				}

				disposedValue = true;
			}
		}

		~StaticHttpClient()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
