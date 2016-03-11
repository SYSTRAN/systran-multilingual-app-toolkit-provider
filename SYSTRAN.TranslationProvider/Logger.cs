using System;
using System.Diagnostics;
using System.IO;

namespace SYSTRAN.TranslationProvider
{
	public class Logger
	{
		public static string GetLogFilePath()
		{
			return Path.GetTempPath() + "SYSTRAN.TranslationProvider.log";
		}

		[Conditional("DEBUG")]
		public static void Log(string msg)
		{
			var sw = File.AppendText(GetLogFilePath());
			try
			{
				string logLine = $"{DateTime.Now:G}: {msg}";
				sw.WriteLine(logLine);
				Debug.WriteLine(logLine);
			}
			finally
			{
				sw.Close();
			}
		}
	}
}
