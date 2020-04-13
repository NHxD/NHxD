using System.ComponentModel;

namespace NHxD.Frontend.Winforms
{
	public class ApplicationLoader
	{
		public event ProgressChangedEventHandler ProgressChanged = delegate { };

		public void SetProgress(int progressPercentage, object userState)
		{
			OnLoadProgressChanged(new ProgressChangedEventArgs(progressPercentage, userState));
		}

		protected void OnLoadProgressChanged(ProgressChangedEventArgs e)
		{
			ProgressChanged.Invoke(this, e);
		}
	}
}
