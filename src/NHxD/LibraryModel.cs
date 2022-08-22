using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;

namespace NHxD
{
	//
	// FIXME: polling doesn't work yet.
	//

	public class LibraryModel : IDisposable
	{
		// TODO: build this list from Form.ArchiveWriters[i].FileExtension.
		private readonly string[] SupportedArchiveFileExtensions = { ".cbz", ".cbr", ".cb7", ".cba", ".mobi", ".epub" };

		private readonly BindingList<string> filters;
		private readonly FileSystemWatcher fileSystemWatcher;
		private readonly Queue<LibraryEvent> events;

		private ISearchProgressArg searchProgressArg;
		private int tagId = -1;
		private string query = "";
		private int pageIndex = 1;

		public BindingList<string> Filters => filters;
		public FileSystemWatcher FileSystemWatcher => fileSystemWatcher;
		public Queue<LibraryEvent> Events => events;

		public int TagId
		{
			get
			{
				return tagId;
			}
			set
			{
				tagId = value;
				OnTagIdChanged();
			}
		}

		public string Query
		{
			get
			{
				return query;
			}
			set
			{
				query = value;
				OnQueryChanged();
			}
		}

		public int PageIndex
		{
			get
			{
				return pageIndex;
			}
			set
			{
				pageIndex = value;
				OnPageIndexChanged();
			}
		}

		public ISearchProgressArg SearchProgressArg
		{
			get
			{
				return searchProgressArg;
			}
			set
			{
				searchProgressArg = value;
				OnSearchProgressArgChanged();
			}
		}

		public ILibraryPollingTimer Timer { get; }

		public event EventHandler FiltersChanged = delegate { };
		public event EventHandler TagIdChanged = delegate { };
		public event EventHandler QueryChanged = delegate { };
		public event EventHandler PageIndexChanged = delegate { };
		public event EventHandler SearchProgressArgChanged = delegate { };
		public event LibraryEventHandler Poll = delegate { };

		public LibraryModel(string path, ILibraryPollingTimer timer)
		{
			Timer = timer;

			Timer.Tick += Timer_Tick;

			filters = new BindingList<string>();
			events = new Queue<LibraryEvent>();

			try
			{
				Directory.CreateDirectory(path);
			}
			catch (Exception)
			{

			}

			if (Directory.Exists(path))
			{
				try
				{
					fileSystemWatcher = new FileSystemWatcher(path);
				}
				catch (Exception)
				{
				}
			}

			fileSystemWatcher = fileSystemWatcher ?? new FileSystemWatcher();

			fileSystemWatcher.BeginInit();
			fileSystemWatcher.IncludeSubdirectories = false;
			fileSystemWatcher.EnableRaisingEvents = false;
			fileSystemWatcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName;
			fileSystemWatcher.Changed += FileSystemWatcher_Changed;
			fileSystemWatcher.Created += FileSystemWatcher_Created;
			fileSystemWatcher.Deleted += FileSystemWatcher_Deleted;
			fileSystemWatcher.Renamed += FileSystemWatcher_Renamed;
			fileSystemWatcher.Error += FileSystemWatcher_Error;
			fileSystemWatcher.EndInit();
		}

		/*public void AddClearFilter()
		{
			Filters.Insert(0, "");

			FiltersChanged.Invoke(this, EventArgs.Empty);
		}*/

		public void AddFilter(string filter)
		{
			OnFilterAdded(filter);
		}

		protected virtual void OnFilterAdded(string filter)
		{
			int itemIndex = filters.IndexOf(filter);

			if (itemIndex != -1)
			{
				filters.RemoveAt(itemIndex);
			}

			filters.Insert(0, filter);

			FiltersChanged.Invoke(this, EventArgs.Empty);
		}

		public void RemoveFilter(string filter)
		{
			int index = filters.IndexOf(filter);

			if (index == -1)
			{
				return;
			}

			filters.RemoveAt(index);

			FiltersChanged.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnTagIdChanged()
		{
			TagIdChanged.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnQueryChanged()
		{
			QueryChanged.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnPageIndexChanged()
		{
			PageIndexChanged.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnSearchProgressArgChanged()
		{
			SearchProgressArgChanged.Invoke(this, EventArgs.Empty);
		}

		public void EnablePolling()
		{
			if (fileSystemWatcher == null)
			{
				return;
			}

			Timer.Start();

			//LibraryModel.FileSystemWatcher.Created += FileSystemWatcher_Created;
			//LibraryModel.FileSystemWatcher.Deleted += FileSystemWatcher_Deleted;
			//LibraryModel.FileSystemWatcher.Renamed += FileSystemWatcher_Renamed;
			//LibraryModel.FileSystemWatcher.Changed += FileSystemWatcher_Changed;
		}

		public void DisablePolling()
		{
			if (fileSystemWatcher == null
				|| Timer == null)
			{
				return;
			}

			Timer.Stop();
		}

		private void Timer_Tick(object sender, TickEventArgs e)
		{
			while (events.Count() > 0)
			{
				LibraryEvent libraryEvent = events.Dequeue();

				Poll.Invoke(this, new LibraryEventArgs(libraryEvent));
			}
		}

		public void Start()
		{
			if (string.IsNullOrEmpty(fileSystemWatcher.Path))
			{
				return;
			}

			fileSystemWatcher.EnableRaisingEvents = true;
		}

		public void Stop()
		{
			if (string.IsNullOrEmpty(fileSystemWatcher.Path))
			{
				return;
			}

			fileSystemWatcher.EnableRaisingEvents = false;
		}

		private static void FileSystemWatcher_Error(object sender, ErrorEventArgs e)
		{
			FileSystemWatcher fileSystemWatcher = sender as FileSystemWatcher;

			fileSystemWatcher.EnableRaisingEvents = false;
			int tryCount = 5;
			int tryInterval = 200;
			//Stopwatch stopwatch = Stopwatch.StartNew();

			for (int i = 0; i < tryCount; ++i)
			{
				//if (stopwatch.ElapsedMilliseconds < tryInterval)
				//{
				//	continue;
				//}

				try
				{
					fileSystemWatcher.EnableRaisingEvents = true;
					break;
				}
				catch
				{
					fileSystemWatcher.EnableRaisingEvents = false;
					Thread.Sleep(tryInterval);
					//stopwatch.Restart();
				}
			}
		}

		private void FileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
		{
			if (ShouldFilterPath(e.FullPath))
			{
				return;
			}

			events.Enqueue(new LibraryEvent(LibraryEventType.Rename, e));
		}

		private void FileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
		{
			if (ShouldFilterPath(e.FullPath))
			{
				return;
			}

			events.Enqueue(new LibraryEvent(LibraryEventType.Delete, e));
		}

		private void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
		{
			if (ShouldFilterPath(e.FullPath))
			{
				return;
			}

			events.Enqueue(new LibraryEvent(LibraryEventType.Create, e));
		}

		private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
		{
			if (ShouldFilterPath(e.FullPath))
			{
				return;
			}

			events.Enqueue(new LibraryEvent(LibraryEventType.Change, e));
		}

		private bool ShouldFilterPath(string fullPath)
		{
			return (File.Exists(fullPath) && SupportedArchiveFileExtensions.Contains(Path.GetExtension(fullPath).ToLowerInvariant()))
				|| (!Directory.Exists(fullPath));
		}

		#region IDisposable Support
		private bool disposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					if (fileSystemWatcher != null)
					{
						fileSystemWatcher.Dispose();
					}
				}

				disposedValue = true;
			}
		}

		~LibraryModel()
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

	public delegate void LibraryEventHandler(object sender, LibraryEventArgs e);
	//public delegate void ItemRemovedEventHandler(object sender, EventArgs e);
	//public delegate void ItemAddedEventHandler(object sender, EventArgs e);

	public class LibraryEventArgs : EventArgs
	{
		public LibraryEvent LibraryEvent { get; }

		public LibraryEventArgs(LibraryEvent libraryEvent)
		{
			LibraryEvent = libraryEvent;
		}
	}

	public enum LibraryEventType
	{
		Change,
		Create,
		Delete,
		Rename
	}

	public class LibraryEvent
	{
		public LibraryEventType EventType { get; }
		public object EventData { get; }

		public LibraryEvent(LibraryEventType eventType, object eventData)
		{
			EventType = eventType;
			EventData = eventData;
		}
	}

	public class TickEventArgs : EventArgs
	{
		public object UserData { get; }

		public TickEventArgs(object userData)
		{
			UserData = userData;
		}
	}

	public delegate void TickEventHandler(object sender, TickEventArgs e);

	public interface ILibraryPollingTimer
	{
		object UserData { get; }
		int Interval { get; }

		event TickEventHandler Tick;

		void Start();
		void Stop();
	}
}
