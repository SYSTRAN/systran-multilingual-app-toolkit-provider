using System.ComponentModel;

namespace SYSTRAN.TranslationProvider
{
	/// <summary>
	/// Provides the Localized metadata's display name
	/// </summary>
	/// <remarks>
	/// See: http://msdn.microsoft.com/en-us/library/vstudio/System.ComponentModel.DisplayNameAttribute(v=vs.100).aspx
	/// </remarks>
	public sealed class LocalizedDisplayNameAttribute : DisplayNameAttribute
	{
		private readonly string mResourceName;
		public LocalizedDisplayNameAttribute(string resourceName)
		{
			mResourceName = resourceName;
		}

		public override string DisplayName => Resources.SYSTRANResources.ResourceManager.GetString(mResourceName);
	}

	/// <summary>
	/// Provides the Localized metadata's description.
	/// </summary>
	/// <remarks>
	/// See: http://msdn.microsoft.com/en-us/library/vstudio/system.componentmodel.descriptionattribute(v=vs.100).aspx
	/// </remarks>
	public sealed class LocalizedDescriptionAttribute : DescriptionAttribute
	{
		private readonly string mResourceName;
		public LocalizedDescriptionAttribute(string resourceName)
		{
			mResourceName = resourceName;
		}

		public override string Description => Resources.SYSTRANResources.ResourceManager.GetString(mResourceName);
	}
}
