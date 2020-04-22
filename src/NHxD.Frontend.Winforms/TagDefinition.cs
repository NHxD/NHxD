using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UrbanDictionary;

namespace NHxD.Frontend.Winforms
{
	public class TagDefinition : IDisposable
	{
		private readonly BackgroundWorker backgroundWorker;

		public string Term { get; }
		public HttpClient HttpClient { get; }

		public TagDefinition(string term, HttpClient httpClient)
		{
			Term = term;
			HttpClient = httpClient;

			backgroundWorker = new BackgroundWorker();

			backgroundWorker.WorkerReportsProgress = true;
			backgroundWorker.WorkerSupportsCancellation = true;
			backgroundWorker.DoWork += DefineTermBackgroundWorker_DoWork;
			backgroundWorker.ProgressChanged += DefineTermBackgroundWorker_ProgressChanged;
			backgroundWorker.RunWorkerCompleted += DefineTermBackgroundWorker_RunWorkerCompleted;
		}

		public void Search()
		{
			backgroundWorker.RunWorkerAsync(new RunArg(Term, HttpClient));
		}

		private void ShowDefinition(SearchResult searchResult, int itemIndex)
		{
			SearchResultItem item = searchResult.List[itemIndex];

			if (searchResult.List.Count == 1)
			{
				MessageBox.Show(item.Definition, Term, MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			else
			{
				StringBuilder sb = new StringBuilder();

				sb.AppendLine(item.Definition);
				sb.AppendLine();

				if (itemIndex > 0 && searchResult.List.Count > 1)
				{
					sb.AppendLine("* Press Yes to read previous definition");
				}
				else
				{
					sb.AppendLine("* Press Yes to read last definition");
				}

				if (itemIndex < searchResult.List.Count - 1 && searchResult.List.Count > 1)
				{
					sb.AppendLine("* Press No to read next definition");
				}
				else
				{
					sb.AppendLine("* Press No to read first definition");
				}

				sb.AppendLine("* Press cancel to close");

				DialogResult result = MessageBox.Show(sb.ToString(), string.Format(CultureInfo.CurrentUICulture, "{0} ({1}/{2})", Term, itemIndex + 1, searchResult.List.Count), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);

				if (result == DialogResult.Yes)
				{
					if (itemIndex > 0 && searchResult.List.Count > 1)
					{
						ShowDefinition(searchResult, itemIndex - 1);
					}
					else
					{
						ShowDefinition(searchResult, searchResult.List.Count - 1);
					}
				}
				else if (result == DialogResult.No)
				{
					if (itemIndex < searchResult.List.Count - 1 && searchResult.List.Count > 1)
					{
						ShowDefinition(searchResult, itemIndex + 1);
					}
					else
					{
						ShowDefinition(searchResult, 0);
					}
				}
			}
		}

		private void DefineTermBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				MessageBox.Show(e.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			if (!e.Cancelled)
			{
				SearchResult searchResult = e.Result as SearchResult;
				Exception ex = e.Result as Exception;
				HttpResponseMessage response = e.Result as HttpResponseMessage;

				if (searchResult != null)
				{
					if (searchResult.List.Count == 0)
					{
						MessageBox.Show(string.Format(CultureInfo.CurrentUICulture, "Couldn't find any definition for \"{0}\"", Term), Term, MessageBoxButtons.OK, MessageBoxIcon.Warning);
					}
					else
					{
						searchResult.List = searchResult.List
							.OrderByDescending(x => x.ThumbsUp - x.ThumbsDown).ToList();

						ShowDefinition(searchResult, 0);
					}
				}
				else if (ex != null)
				{
					MessageBox.Show(ex.ToString(), Term, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				else if (response != null)
				{
					MessageBox.Show(string.Format(CultureInfo.InvariantCulture, "{0} ({1})", response.ReasonPhrase, response.StatusCode), Term, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void DefineTermBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
		}

		private static void DefineTermBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			RunArg runArg = e.Argument as RunArg;
			string uri = string.Format(CultureInfo.InvariantCulture, "http://api.urbandictionary.com/v0/define?term={0}", WebUtility.UrlEncode(runArg.Term));

			try
			{
				using (HttpResponseMessage response = Task.Run(() => runArg.HttpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead)).GetAwaiter().GetResult())
				{
					if (!response.IsSuccessStatusCode)
					{
						response.EnsureSuccessStatusCode();
						//error = string.Format(CultureInfo.InvariantCulture, "{0} ({1})", response.ReasonPhrase, response.StatusCode);
						return;
					}
					else
					{
						try
						{
							string jsonText = Task.Run(() => response.Content.ReadAsStringAsync()).GetAwaiter().GetResult();

							e.Result = JsonConvert.DeserializeObject<SearchResult>(jsonText);
							return;
						}
						catch (Exception ex)
						{
							e.Result = ex;
							return;
						}
					}
				}
			}
			catch (Exception ex)
			{
				e.Result = ex;
				return;
			}
		}

		private class RunArg
		{
			public string Term { get; }
			public HttpClient HttpClient { get; }

			public RunArg(string term, HttpClient httpClient)
			{
				Term = term;
				HttpClient = httpClient;
			}
		}

		#region IDisposable Support
		private bool disposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					if (backgroundWorker != null)
					{
						backgroundWorker.Dispose();
					}
				}

				disposedValue = true;
			}
		}

		~TagDefinition()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
