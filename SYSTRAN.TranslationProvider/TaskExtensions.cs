using System.Threading.Tasks;

namespace SYSTRAN.TranslationProvider
{
	/// <summary>
	/// This tricks is used to prevent deadlock when called from VS
	/// </summary>
	public static class TaskExtensions
	{
		public static Task ConfigureAwait(this Task t)
		{
			do
			{
				System.Windows.Forms.Application.DoEvents();
			} while (!t.IsCompleted);
			return t;
		}
		public static Task<TResult> ConfigureAwait<TResult>(this Task<TResult> t)
		{
			do
			{
				System.Windows.Forms.Application.DoEvents();
			} while (!t.IsCompleted);
			return t;
		}
	}
}
