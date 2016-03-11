using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SYSTRAN.TranslationProvider.Data;

namespace SYSTRAN.TranslationProvider
{
	public class APIException : Exception
	{
		public HttpStatusCode? StatusCode { get; set; }

		public APIException()
		{
		}

		public APIException(string message)
			: base(message)
		{
		}

		public APIException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}

	public class BadErrorResponseAPIException : APIException
	{
		public string ContentReceived { get; private set; }

		public BadErrorResponseAPIException(string message, string contentReceived)
			: this(message, contentReceived, null)
		{
			ContentReceived = contentReceived;
		}

		public BadErrorResponseAPIException(string message, string contentReceived, Exception innerException)
			: base(message, innerException)
		{
			ContentReceived = contentReceived;
		}
	}


	namespace Data
	{
		[DataContract]
		public class WarningError
		{
			[DataMember(Name = "warning")]
			public string Warning;

			[DataMember(Name = "error")]
			public string Error;

			public void EnsureNoError()
			{
				if (string.IsNullOrEmpty(Error) == false)
				{
					throw new APIException($"Error received:\"{Error}\"");
				}
			}
		}

		[DebuggerDisplay("{ToString()}")]
		[DataContract]
		public class LanguagePair
		{
			[DataMember(Name = "source")]
			public string Source { get; set; }

			[DataMember(Name = "target")]
			public string Target { get; set; }

			public LanguagePair()
			{
			}

			public LanguagePair(LanguagePair lp)
			{
				Source = lp.Source;
				Target = lp.Target;
			}

			public LanguagePair(string source, string target)
			{
				Source = source?.ToLower() ?? string.Empty;
				Target = target?.ToLower() ?? string.Empty;
			}

			public override string ToString() => $"{Source}>{Target}";

			public override bool Equals(object obj)
			{
				LanguagePair lp = obj as LanguagePair;
				if (lp == null) return false;
				return string.Equals(Source, lp.Source) && string.Equals(Target, lp.Target);
			}

			public override int GetHashCode()
			{
				return ToString().GetHashCode();
			}
		}

		[DebuggerDisplay("id:{Id} name:{Name}")]
		[DataContract]
		public class Profile
		{
			[DataMember(Name = "id")]
			public string Id { get; set; }

			[DataMember(Name = "private")]
			public bool? Private { get; set; }

			[DataMember(Name = "name")]
			public string Name { get; set; }

			/// <summary>
			/// presentation property
			/// </summary>
			public string Prettyname
			{
				get
				{
					string prettyName = string.Empty;
					if (string.IsNullOrWhiteSpace(Id) == false)
					{
						prettyName = $"Profile {Id}";
					}
					if (string.IsNullOrWhiteSpace(Name) == false)
					{
						prettyName += $" - {Name}";
					}
					if (string.IsNullOrEmpty(prettyName) == false)
					{
						if (Private.HasValue)
						{
							if (Private.Value)
							{
								prettyName += " - [private]";
							}
							else
							{
								prettyName += " - [public]";
							}
						}
					}
					return prettyName;
				}
			}
		}

		[DataContract]
		public class SupportedLanguagesResponse : WarningError
		{
			[DataMember(Name = "languagePairs")]
			public List<LanguagePair> LanguagePairs;
		}
	}

	public class TranslationService
	{
		private readonly HttpClient mClient = new HttpClient();

		private readonly string mServerbaseAdress;
		private readonly string mKey;

		public TranslationService(string serverbaseAdress, string key)
		{
			mServerbaseAdress = serverbaseAdress;
			mKey              = key;
		}

		protected static async Task<APIException> TryToExtractInfoFromErrorResponse(HttpResponseMessage response)
		{
			string finalMessage = $"StatusCode {(int)response.StatusCode} [{response.ReasonPhrase}]";

			string contentString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			try
			{
				JObject errorResponse = JObject.Parse(contentString);
				string errorMessage = null;

				JToken errorField = errorResponse["error"];
				if (errorField != null)
				{
					switch (errorField.Type)
					{
						case JTokenType.String:
							errorMessage = (string)errorField;
							break;
						case JTokenType.Object:
							var errorMessageToken = errorField["message"];
							if (errorMessageToken != null)
							{
								errorMessage = errorMessageToken.ToString();
							}
							else
							{
								var errorMsgToken = errorField["msg"];
								if (errorMsgToken != null)
								{
									errorMessage = errorMsgToken.ToString();
								}
							}
							break;
						default:
							errorMessage = errorField.ToString();
							break;
					}
				}

				if (errorMessage == null)
				{
					return new BadErrorResponseAPIException(finalMessage, contentString) { StatusCode = response.StatusCode };
				}
				return new APIException($"{errorMessage} - " + finalMessage) { StatusCode = response.StatusCode };
			}
			catch (JsonSerializationException exception)
			{
				return new BadErrorResponseAPIException(finalMessage, contentString, exception) { StatusCode = response.StatusCode };
			}
			catch (JsonReaderException exception)
			{
				return new BadErrorResponseAPIException(finalMessage, contentString, exception) { StatusCode = response.StatusCode };
			}
			catch (Exception exception)
			{
				return new BadErrorResponseAPIException(finalMessage, contentString, exception) { StatusCode = response.StatusCode };
			}
		}

		/// <summary>
		/// Read content response as JSON and ensure no error present
		/// </summary>
		/// <typeparam name="T">Type of data to read</typeparam>
		/// <param name="response"></param>
		/// <returns></returns>
		protected static async Task<T> ReadAsValidJSONWarningErrorAsync<T>(HttpResponseMessage response) where T : Data.WarningError
		{
			if (response == null) 
			{
				throw new APIException("No response");
			}

			if (response.IsSuccessStatusCode)
			{
				var contentString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
				T t = JsonConvert.DeserializeObject<T>(contentString);
				if (t == null)
				{
					throw new APIException("No JSON readen");
				}
				t.EnsureNoError();
				return t;
			}
			var exception = await TryToExtractInfoFromErrorResponse(response).ConfigureAwait(false);
			throw exception;
		}


		public async Task<List<LanguagePair>> GetSupportedLanguagesAsync(CancellationToken ct, string srcLang = null, string tgtLang = null)
		{
			return await Task.Run(async () =>
			{
				var response = await mClient.GetAsync($"{mServerbaseAdress}translation/supportedLanguages?key={mKey}"+
				                                      (!string.IsNullOrEmpty(srcLang) ? "&source=" + srcLang : "") +
				                                      (!string.IsNullOrEmpty(tgtLang) ? "&target=" + tgtLang : ""), ct);

				var supportedLanguagesResponse = await ReadAsValidJSONWarningErrorAsync<SupportedLanguagesResponse>(response);

				return supportedLanguagesResponse.LanguagePairs;
			}, ct);
		}


		public async Task<string> TranslateAsync(string sourceLang, string targetlang, string profileId, string text)
		{
			using (var response = await mClient.PostAsync($"{mServerbaseAdress}translate?key={mKey}&source={sourceLang}&target={targetlang}&profile={profileId}&format=text/plain&rawBody=true", new StringContent(text)))
			{
				if (response.IsSuccessStatusCode)
				{
					var httpContent = response.Content;
					response.Content = null; // to prevent response disposal to Dispose internal stream
					return await httpContent.ReadAsStringAsync();
				}
				var exception = await TryToExtractInfoFromErrorResponse(response);
				throw exception;
			}
		}
	}
}