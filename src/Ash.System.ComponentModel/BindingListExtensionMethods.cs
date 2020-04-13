using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Ash.System.ComponentModel
{
	public static class BindingListExtensionMethods
	{
		public static void AddRange<T>(this BindingList<T> bindingList, IEnumerable<T> items)
		{
			if (bindingList == null)
			{
				throw new ArgumentNullException("bindingList");
			}

			if (items == null)
			{
				throw new ArgumentNullException("items");
			}

			foreach (T item in items)
			{
				bindingList.Add(item);
			}
		}
	}
}
