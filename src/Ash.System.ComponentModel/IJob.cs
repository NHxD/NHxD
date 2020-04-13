namespace Ash.System.ComponentModel
{
	public interface IJob
	{
		void RunAsync();
		void CancelAsync();
		bool IsBusy();
	}
}
