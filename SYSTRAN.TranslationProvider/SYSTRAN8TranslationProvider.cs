using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Multilingual.Translation;
using Newtonsoft.Json;
using SYSTRAN.TranslationProvider.Data;
using Localizable = SYSTRAN.TranslationProvider.Resources.SYSTRANResources;

namespace SYSTRAN.TranslationProvider
{
	public class SYSTRAN8TranslationProvider : ITranslationProvider
	{
		private LanguagePair mSelectedLanguagePair;

		private readonly Stream mLogoTranslateStandard;
		private readonly Stream mLogoSuggestStandard;

		private readonly TranslationService mService;
		private readonly List<LanguagePair> mSupportedLanguagePairs;
		private readonly bool mChineseLegacyMode;

		public SYSTRAN8TranslationProvider(string configFile)
		{
			Logger.Log("SYSTRAN8TranslationProvider ctor start");

			{
				var providerAssembly = Assembly.GetExecutingAssembly();
				mLogoTranslateStandard = providerAssembly.GetManifestResourceStream("SYSTRAN.TranslationProvider.Images.logo-s-systran32.png");
				mLogoSuggestStandard =   providerAssembly.GetManifestResourceStream("SYSTRAN.TranslationProvider.Images.logo-s-systran32.png");
			}

			var fullPath = Path.GetFullPath(configFile);
			if (File.Exists(fullPath))
			{
				try
				{
					var settingsContent = File.ReadAllText(fullPath);
					var serverSettings = JsonConvert.DeserializeObject<ServerSettings>(settingsContent);

					if (serverSettings != null)
					{
						Logger.Log($"SYSTRAN8TranslationProvider ctor configFile:{fullPath} Url:{serverSettings.Url} Key:{serverSettings.Key}");
						if(string.IsNullOrEmpty(serverSettings.Key))
						{
							throw new Exception($"No key is present in {fullPath}: you must obtain a key from SYSTRAN 8 server and regsiter it");
						}
						mService = new TranslationService(serverSettings.Url, serverSettings.Key);

						mSupportedLanguagePairs = mService.GetSupportedLanguagesAsync(CancellationToken.None).ConfigureAwait().ConfigureAwait().Result;
						mChineseLegacyMode = mSupportedLanguagePairs.IsChineseLegacyMode();
						Logger.Log($"SYSTRAN8TranslationProvider ctor ChineseLegacyMode:{mChineseLegacyMode}");
					}
				}
				catch (Exception exception)
				{
					Logger.Log($"SYSTRAN8TranslationProvider ctor Error importing configFile:{fullPath} message:{exception.Message}");
					throw new Exception($"SYSTRAN8TranslationProvider ctor Error importing configFile:{fullPath} message:{exception.Message}");
				}
			}
			else
			{
				Logger.Log($"SYSTRAN8TranslationProvider ctor no configFile found:{fullPath}");
				throw new Exception($"SYSTRAN8TranslationProvider ctor no configFile found:{fullPath}");
			}

			Logger.Log("SYSTRAN8TranslationProvider ctor end");
		}

		/// <summary>
		/// Get all Supported Language pairs from the service
		/// </summary>
		/// <returns>list of supported CultureInfo</returns>
		public CultureInfo[] GetTargets(CultureInfo source)
		{
			Logger.Log($"SYSTRAN8TranslationProvider GetTargets start {source.Name}");
			if (source == null) { throw new ArgumentNullException(nameof(source)); }

			var compatibleTargets = mSupportedLanguagePairs.GetCompatiblePairs(source, mChineseLegacyMode).ToArray();
			Logger.Log($"SYSTRAN8TranslationProvider GetTargets end {compatibleTargets?.Length}");
			return compatibleTargets;
		}

		/// <summary>
		/// Checks provider supported language pairs.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public bool IsSupported(CultureInfo source, CultureInfo target)
		{
			Logger.Log($"SYSTRAN8TranslationProvider IsSupported start source:{source.Name} target:{target.Name}");
			if (source == null) { throw new ArgumentNullException(nameof(source)); }
			if (target == null) { throw new ArgumentNullException(nameof(target)); }

			var lp = mSupportedLanguagePairs.GetCompatiblePair(source, target, mChineseLegacyMode);
			Logger.Log($"SYSTRAN8TranslationProvider IsSupported end source:{source.Name} target:{target.Name} ret:{lp != null}");
			return lp != null;
		}

		/// <summary>
		/// Initializes the provider for a set language pair
		/// </summary>
		/// <param name="source">CultureInfo for source language</param>
		/// <param name="target">CultureInfo for target language</param>
		/// <param name="initializationData">Class containing the project's Name and Version</param>
		public void Initialize(CultureInfo source, CultureInfo target, ProjectInfo initializationData)
		{
			Logger.Log($"SYSTRAN8TranslationProvider Initialize start source:{source.Name} target:{target.Name} project.name:{initializationData?.ProjectName} project.version:{initializationData?.ProjectVersion}");
			if (source == null) { throw new ArgumentNullException(nameof(source)); }
			if (target == null) { throw new ArgumentNullException(nameof(target)); }

			var lp = mSupportedLanguagePairs.GetCompatiblePair(source, target, mChineseLegacyMode);
			if (lp == null) throw new ArgumentException("Incompatibles source and target");
			mSelectedLanguagePair = lp;

			Logger.Log("SYSTRAN8TranslationProvider Initialize end");
		}

		/// <summary>
		/// Attempts to translates a resource
		/// </summary>
		/// <returns>True if the resource is translated, otherwise returns false </returns>
		public TranslationResult Translate(TranslationRequest request)
		{
			Logger.Log($"SYSTRAN8TranslationProvider Translate start request:{request.Source} source:{mSelectedLanguagePair?.Source} target:{mSelectedLanguagePair?.Target}");
			if (request == null)                            throw new ArgumentNullException(nameof(request));
			if (string.IsNullOrWhiteSpace(request.Source) ) throw new ArgumentException("TranslationRequest.Source");
			if (mSelectedLanguagePair == null)              throw new InvalidProviderConfigurationException("No language pait selected");
			if (mSupportedLanguagePairs == null)            throw new InvalidProviderConfigurationException("supported language pairs has not been initialized");

			var transResult = new TranslationResult
			{
				RequestId        = request.RequestId,
				TranslationState = new TranslationState { State = TransState.NoMatch, SubState = "no match found" },
				TranslationType  = new TranslationType { Type = TransType.MachineTranslation },
				ProviderName     = Localizable.ProviderDisplayName,
				Source           = request.Source,
			};
			try
			{
				var target = mService.TranslateAsync(mSelectedLanguagePair.Source, mSelectedLanguagePair.Target, null, request.Source).ConfigureAwait().Result;

				transResult.Target           = target;
				transResult.TranslationState = new TranslationState {State = TransState.NeedsReview};
				//Properties                 = GetMetadata(trans);
				transResult.Confidence       = 100;
			}
			catch (Exception exception)
			{
				transResult.TranslationState = new TranslationState { State = TransState.Fail };
				transResult.Errors.Add(new ProviderErrorLog { Category = ErrorCategory.Translation, InnerException = exception, Severity = ErrorSeverity.Error });
			}
			Logger.Log($"SYSTRAN8TranslationProvider Translate end request:{request.Source} source:{mSelectedLanguagePair?.Source} target:{mSelectedLanguagePair?.Target} result:{transResult?.Target}");
			return transResult;
		}

		/// <summary>
		/// Attempts to find translation suggestions based the specified options in SuggestionRequest object.
		/// </summary>
		/// <param name="request">Suggestion Parameters</param>
		/// <returns>A list of suggestion result</returns>
		public SuggestionResult[] Suggest(SuggestionRequest request)
		{
			Logger.Log($"SYSTRAN8TranslationProvider Suggest start request:{request.Source} source:{mSelectedLanguagePair?.Source} target:{mSelectedLanguagePair?.Target}");
			if (request == null)                           throw new ArgumentNullException(nameof(request));
			if (string.IsNullOrWhiteSpace(request.Source)) throw new ArgumentException("SuggestionRequest.Source");
			if (mSelectedLanguagePair == null)             throw new InvalidProviderConfigurationException("No language pait selected");
			if (mSupportedLanguagePairs == null)           throw new InvalidProviderConfigurationException("supported language pairs has not been initialized");

			var suggestResult = new SuggestionResult
			{
				RequestId        = request.RequestId,
				Source           = request.Source,
				ProviderName     = Localizable.ProviderDisplayName,
				TranslationState = new TranslationState { State = TransState.NoMatch, SubState = "no match found" },
				TranslationType  = new TranslationType { Type = TransType.MachineTranslation }
			};
			try
			{
				var target = mService.TranslateAsync(mSelectedLanguagePair.Source, mSelectedLanguagePair.Target, null, request.Source).ConfigureAwait().Result;

				suggestResult.Target = target;
				suggestResult.TranslationState = new TranslationState {State = TransState.NeedsReview};
				suggestResult.Confidence = 100;
				//suggestResult.Properties = GetMetadata(trans);
			}
			catch (Exception exception)
			{
				suggestResult.TranslationState = new TranslationState { State = TransState.Fail};
				suggestResult.Errors.Add(new ProviderErrorLog() {Category = ErrorCategory.Translation, InnerException = exception, Severity = ErrorSeverity.Error});
			}
			Logger.Log($"SYSTRAN8TranslationProvider Suggest end request:{request.Source} source:{mSelectedLanguagePair?.Source} target:{mSelectedLanguagePair?.Target} result:{suggestResult?.Target}");
			return new[] { suggestResult };
		}

		/// <summary>
		/// Get provider logo from provider.
		/// </summary>
		/// <returns>Image in Bitmap format</returns>
		public Stream GetProviderLogo(ProviderLogoStyle style)
		{
			switch (style)
			{
				case ProviderLogoStyle.SuggestStandard:   return mLogoSuggestStandard;
				case ProviderLogoStyle.TranslateStandard: return mLogoTranslateStandard;

				default:
					throw new NotSupportedException(style.ToString());
			}
		}

		/// <summary>
		/// Provider cleanup, as needed
		/// </summary>
		public void Dispose()
		{
		}

		/// <summary>
		/// Localized Display Name
		/// </summary>
		public string DisplayName => Localizable.ProviderDisplayName;

		/// <summary>
		/// Localized Description
		/// </summary>
		public string Description => Localizable.ProviderDescription;
	}
}
