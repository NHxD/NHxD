using System;
using System.Windows.Forms;

namespace Ash.System.Windows.Forms
{
	public sealed class CursorScope : IDisposable
	{
		private readonly Cursor previousCursor;

		public Cursor PreviousCursor => previousCursor;

		public Cursor Cursor { get; }

		public CursorScope(Cursor cursor)
		{
			Cursor = cursor;

			previousCursor = Cursor.Current;

			if (Cursor.Current != cursor)
			{
				Cursor.Current = cursor;
			}
		}

		public void Dispose()
		{
			if (Cursor.Current != previousCursor)
			{
				Cursor.Current = previousCursor;
			}
		}
	}
}
