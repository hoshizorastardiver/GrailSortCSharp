using GrailSortCSharp.Algorithm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GrailSortCSharp.Extensions
{
    public static class GrailSortExtensions
    {
        /// <summary>
        /// Sorts the elements of a sequence in ascending order according to a key.
        /// </summary>
        /// 

        /// <summary>
        /// Sorts the elements of a sequence in ascending order using a specified comparer.
        /// </summary>
        /// 

        /// <summary>
        /// Sorts the elements of a sequence in descending order according to a key.
        /// </summary>
        

        /// <summary>
        /// Sorts the elements of a sequence in descending order using a specified comparer.
        /// </summary>
        

        /// <summary>
        /// Sorts the elements in a List<T> in place using the default comparer.
        /// </summary>
        

        /// <summary>
        /// Sorts the elements in a List<T> in place using the specified comparer.
        /// </summary>
        

        /// <summary>
        /// Sorts the elements in a List<T> in place using the specified comparison.
        /// </summary>
        

        /// <summary>
        /// Sorts the elements in a List<T> in place in descending order using the default comparer.
        /// </summary>
        

        /// <summary>
        /// Sorts the elements in a List<T> in place in descending order using the specified comparer.
        /// </summary>
        

        /// <summary>
        /// Sorts the elements in an array in place using the default comparer.
        /// </summary>
        

        /// <summary>
        /// Sorts the elements in an array in place using the specified comparer.
        /// </summary>
        

        /// <summary>
        /// Sorts the elements in an array in place using the specified comparison.
        /// </summary>
        

        /// <summary>
        /// Sorts the elements in an array in place in descending order using the default comparer.
        /// </summary>
        

        /// <summary>
        /// Sorts the elements in an array in place in descending order using the specified comparer.
        /// </summary>
        

        private class ReverseComparer<T> : IComparer<T>
        {
            private readonly IComparer<T> _comparer;
            public ReverseComparer(IComparer<T> comparer)
            {
                _comparer = comparer;
            }
            public int Compare(T x, T y)
            {
                return _comparer.Compare(y, x);
            }
        }

        public enum SortingBufferType
        {
            InPlace, Static, Dynamic
        }
    }
}
