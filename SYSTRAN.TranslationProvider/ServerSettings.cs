using System.Diagnostics;
using System.Runtime.Serialization;

namespace SYSTRAN.TranslationProvider
{
	[DataContract]
	[DebuggerDisplay("{Url} [{Key}]")]
	public class ServerSettings
	{
		[DataMember]
		public string Url { get; set; }

		[DataMember]
		public string Key { get; set; }

	}
}