using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SYSTRAN.TranslationProvider.Data;

namespace SYSTRAN.TranslationProvider
{
	public static class LanguagePairEnumerableExtensions
	{
		public static bool IsChineseLegacyMode(this IEnumerable<LanguagePair> languagePairs)
		{
			if (languagePairs != null)
			{
				foreach (var lp in languagePairs)
				{
					var src = lp.Source.ToLower();
					if (src == "zh" || src== "zt")
					{
						return true;
					}
					var tgt = lp.Target.ToLower();
					if (tgt == "zh" || tgt == "zt")
					{
						return true;
					}
				}
			}
			return false;
		}


		private static LanguagePair GetCompatiblePair(this IEnumerable<LanguagePair> languagePairs, string sourceLanguageCode, string targetLanguageCode)
		{
			if (string.IsNullOrWhiteSpace(sourceLanguageCode) || string.IsNullOrWhiteSpace(targetLanguageCode))
			{
				return null;
			}
			if (languagePairs != null)
			{
				foreach (var lp in languagePairs)
				{
					if (lp.Source == sourceLanguageCode && lp.Target == targetLanguageCode)
						return lp;
				}
			}
			return null;
		}
		private static IEnumerable<LanguagePair> GetCompatiblePairs(this IEnumerable<LanguagePair> languagePairs, string sourceLanguageCode)
		{
			if (languagePairs != null)
			{
				foreach (var lp in languagePairs)
				{
					if (lp.Source == sourceLanguageCode)
						yield return lp;
				}
			}
		}




		public static LanguagePair GetCompatiblePair(this IEnumerable<LanguagePair> languagePairs, CultureInfo source, CultureInfo target, bool chineseLegacyMode)
		{
			string sourceLanguage = source.ToSystranName(chineseLegacyMode);
			string targetLanguage = target.ToSystranName(chineseLegacyMode);

			return languagePairs.GetCompatiblePair(sourceLanguage, targetLanguage);
		}

		public static IEnumerable<CultureInfo> GetCompatiblePairs(this IEnumerable<LanguagePair> languagePairs, CultureInfo source, bool chineseLegacyMode)
		{
			string sourceLanguage = source.ToSystranName(chineseLegacyMode);
			var pairs = languagePairs.GetCompatiblePairs(sourceLanguage);
			return pairs.Select(p => new CultureInfo(p.Target));
		}
	}
}