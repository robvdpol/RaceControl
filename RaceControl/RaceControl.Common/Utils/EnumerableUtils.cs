using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RaceControl.Common.Utils
{
    public static class EnumerableUtils
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            return new ObservableCollection<T>(items);
        }
    }
}