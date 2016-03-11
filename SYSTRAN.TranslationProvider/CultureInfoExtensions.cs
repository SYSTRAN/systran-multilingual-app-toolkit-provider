using System.Globalization;

namespace SYSTRAN.TranslationProvider
{
	public static class CultureInfoExtensions
	{
		/// <summary>
		/// zh/zt was previously used at SYSTRAN
		/// </summary>
		/// <param name="ci"></param>
		/// <param name="chineseLegacyMode">indicate we use a legacy server</param>
		/// <returns></returns>
		public static string ToSystranName(this CultureInfo ci, bool chineseLegacyMode)
		{
			switch (ci.Name.ToLower())
			{
				case "zh-hant":
					if (chineseLegacyMode)
						return "zt";
					else
						return ci.Name;
				case "zh-hans":
					if (chineseLegacyMode)
						return "zh";
					else
						return ci.Name;
			}
			var splitted = ci.Name.Split('-');
			if (splitted.Length == 1)
				return ci.Name;
			else
				return splitted[0];
		}
	}
}
