using System;
using System.Globalization;
using Microsoft.Multilingual.Translation;
using SYSTRAN.TranslationProvider;

namespace Test
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				var provider = new SYSTRAN8TranslationProvider(@"C:\Users\All Users\Multilingual App Toolkit\SYSTRAN.DataProvider\SYSTRAN.DataProvider.json");

				var enUs = new CultureInfo("en-US");

				var zhHans = new CultureInfo("zh-Hans");
				TestTranslation(provider, enUs, zhHans);

				var fr = new CultureInfo("fr");
				TestTranslation(provider, enUs, fr);
			}
			catch (Exception exception)
			{
				Console.WriteLine($"Error: {exception.Message}");
			}
		}

		private static void TestTranslation(ITranslationProvider provider, CultureInfo src, CultureInfo tgt)
		{
			if (provider.IsSupported(src, tgt))
			{
				provider.Initialize(src, tgt, null);
				var response    = provider.Translate(new TranslationRequest {RequestId = "1", Source = "The dog is blue.", MinimumConfidence = 50});
				Console.WriteLine($"Translate from {src} to {tgt}");
				Console.WriteLine($"   {response.Source}");
				if (response.IsTranslated)
				{
					Console.WriteLine($"   {response.Target}");
				}
				else
				{
					foreach (var error in response?.Errors)
					{
						Console.WriteLine($"   {error.Message}");
					}
				}
				var suggestions = provider.Suggest  (new SuggestionRequest  {RequestId = "1", Source = "The dog is blue.", MinimumConfidence = 50});
			}
		}
	}
}
