using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaiMangou.GameBridge
{
    #region ListExtensions

    /// <summary>
    ///     Extension of the List classs
    /// </summary>
    public static class ListExt
    {
        /// <summary>
        ///     Resize <paramref name="list" /> by <paramref name="size" />
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="size"></param>
        /// <param name="c"></param>
        public static void Resize<T>(this List<T> list, int size, T c = default(T))
        {
            var cur = list.Count;
            if (size < cur)
                list.RemoveRange(size, cur - size);
            else if (size > cur)
                list.AddRange(Enumerable.Repeat(c, size - cur));
        }

        /// <summary>
        ///     Add multiple values to <paramref name="List" />
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="elements"></param>
        public static void AddMany<T>(this List<T> list, params T[] elements)
        {
            list.AddRange(elements);
        }
    }

    #endregion
}