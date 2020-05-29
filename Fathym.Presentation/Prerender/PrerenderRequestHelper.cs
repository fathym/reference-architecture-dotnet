using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fathym.Presentation.Prerender
{
	public class PrerenderRequestHelper
	{
		#region Fields
		protected readonly string escapedFragment;

		protected readonly string crawlerUserAgentPattern;

		protected readonly string defaultIgnoredExtensions;

		protected readonly Encoding defaultEncoding;

		protected readonly PrerenderConfiguration options;
		#endregion

		#region Constructors
		public PrerenderRequestHelper(PrerenderConfiguration options)
		{
			this.options = options;

			crawlerUserAgentPattern = "(google)|(bing)|(Slurp)|(DuckDuckBot)|(YandexBot)|(baiduspider)|(Sogou)|(Exabot)|(ia_archiver)|(facebot)|(facebook)|(twitterbot)|(rogerbot)|(linkedinbot)|(embedly)|(quora)|(pinterest)|(slackbot)|(redditbot)|(Applebot)|(WhatsApp)|(flipboard)|(tumblr)|(bitlybot)|(Discordbot)";

			defaultIgnoredExtensions = "\\.vxml|js|css|less|png|jpg|jpeg|gif|pdf|doc|txt|zip|mp3|rar|exe|wmv|doc|avi|ppt|mpg|mpeg|tif|wav|mov|psd|ai|xls|mp4|m4a|swf|dat|dmg|iso|flv|m4v|torrent";

			defaultEncoding = Encoding.UTF8;

			escapedFragment = "_escaped_fragment_";
		}
		#endregion

		#region API Methods
		public virtual async Task<bool> HandlePrerender(HttpContext context)
		{
			var requestFeature = context.Features.Get<IHttpRequestFeature>();

			if (shouldShowPrerenderedPage(context.Request, requestFeature))
			{
				var response = await getPrerenderedPageResponse(context.Request);

				context.Response.StatusCode = (int)response.StatusCode;

				response.Headers.Each(keyValue => context.Response.Headers[keyValue.Key] = new StringValues(keyValue.Value.ToArray()));

				await context.Response.WriteAsync(response.Body);

				return true;
			}

			return false;
		}
		#endregion

		#region Helpers
		protected virtual bool shouldShowPrerenderedPage(HttpRequest request, IHttpRequestFeature requestFeature)
		{
			var userAgent = request.GetUserAgent();

			var rawUrl = requestFeature.RawTarget;

			var relativeUrl = request.Path.ToString();

			if (request.Query.Keys.Any(a => a.Equals(escapedFragment, StringComparison.OrdinalIgnoreCase)))
				return true;

			if (userAgent.IsNullOrEmpty())
				return false;

			var userAgentPattern = options.CrawlerUserAgentPattern ?? crawlerUserAgentPattern;

			if (userAgentPattern.IsNullOrEmpty() || !Regex.IsMatch(userAgent, userAgentPattern, RegexOptions.IgnorePatternWhitespace))
				return false;

			// check if the extenion matchs default extension
			if (Regex.IsMatch(relativeUrl, defaultIgnoredExtensions, RegexOptions.IgnorePatternWhitespace))
				return false;

			if (!options.AdditionalExtensionPattern.IsNullOrEmpty() && Regex.IsMatch(relativeUrl, options.AdditionalExtensionPattern, RegexOptions.IgnorePatternWhitespace))
				return false;

			if (!options.BlackListPattern.IsNullOrEmpty() && Regex.IsMatch(rawUrl, options.BlackListPattern, RegexOptions.IgnorePatternWhitespace))
				return false;

			if (!options.WhiteListPattern.IsNullOrEmpty() && Regex.IsMatch(rawUrl, options.WhiteListPattern, RegexOptions.IgnorePatternWhitespace))
				return true;

			return false;
		}

		protected virtual string getPrerenderUrl(HttpRequest request)
		{
			var url = request.GetFullUrl();

			if (request.Headers.ContainsKey("X-Forwarded-Proto") && string.Equals(request.Headers["X-Forwarded-Proto"], "https", StringComparison.InvariantCultureIgnoreCase) &&
				url.StartsWith("http:", StringComparison.OrdinalIgnoreCase))
				url = url.Replace("http", "https");

			var prerenderServiceUrl = options.PrerenderServiceUrl;

			return $"{prerenderServiceUrl.Trim('/')}/{url}";
		}

		protected virtual async Task<ProxyResult> getPrerenderedPageResponse(HttpRequest request)
		{
			var prerenderUrl = getPrerenderUrl(request);

			var httpClientHandler = new HttpClientHandler()
			{
				AllowAutoRedirect = true
			};

			if (options.Proxy?.URL != null && options.Proxy?.Port > 0)
				httpClientHandler.Proxy = new WebProxy(options.Proxy.URL, options.Proxy.Port);

			using (var httpClient = new HttpClient(httpClientHandler))
			{
				httpClient.Timeout = TimeSpan.FromSeconds(60);

				httpClient.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue() { NoCache = true };

				httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "text /html");

				httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", request.Headers["User-Agent"].ToString());

				if (!options.Token.IsNullOrEmpty())
					httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Prerender-Token", options.Token);

				using (var webMessage = await httpClient.GetAsync(prerenderUrl))
				{
					var body = default(string);
					try
					{
						using (var stream = await webMessage.Content.ReadAsStreamAsync())
						{
							using (var reader = new StreamReader(stream))
							{
								webMessage.EnsureSuccessStatusCode();

								body = reader.ReadToEnd();

								return new ProxyResult(webMessage.StatusCode, body, webMessage.Headers);
							}
						}
					}
					catch (Exception e)
					{
						body = e.Message;

						return new ProxyResult(HttpStatusCode.InternalServerError, body, null);
					}
				}
			}
		} 
		#endregion
	}
}
