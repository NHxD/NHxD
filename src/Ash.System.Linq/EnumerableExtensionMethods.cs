using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Ash.System.Linq
{
	public static class EnumerableExtensionMethods
	{
		public static IOrderedEnumerable<T> OrderBy<T, TKey>(this IEnumerable<T> that, Func<T, TKey> keySelector, SortOrder sortOrder)
		{
			if (sortOrder == SortOrder.Descending)
			{
				return that.OrderByDescending(keySelector);
			}
			else if (sortOrder == SortOrder.Ascending)
			{
				return that.OrderBy(keySelector);
			}
			else
			{
				return that.OrderBy(x => true);
			}
		}
	}
}
