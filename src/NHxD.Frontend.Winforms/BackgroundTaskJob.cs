using Ash.System.ComponentModel;
using System.ComponentModel;

namespace NHxD.Frontend.Winforms
{
	public class BackgroundTaskJob : IJob
	{
		private bool isDone;

		public object UserState { get; }

		public event ProgressChangedEventHandler ProgressChanged = delegate { };

		public BackgroundTaskJob(object userState)
		{
			UserState = userState;
		}

		public void OnProgressChanged(ProgressChangedEventArgs e)
		{
			ProgressChanged.Invoke(this, e);
		}

		public bool IsBusy()
		{
			return !isDone;
		}

		public void CancelAsync()
		{
			isDone = true;
		}

		public void RunAsync()
		{
			OnProgressChanged(new ProgressChangedEventArgs(100, UserState));
			isDone = true;
		}
	}
}
