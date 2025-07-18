using GrailSortCSharp.Algorithm;
using System.Runtime.InteropServices;

namespace GrailSortCSharp.Extensions
{
    public static class GrailSortExtensions
    {
        public enum SortingBufferType
        {
            InPlace,
            Static,
            Dynamic
        }

        private class ReverseComparer<T> : IComparer<T>
        {
            private readonly IComparer<T> _comparer;
            public ReverseComparer(IComparer<T> comparer) => _comparer = comparer;
            public int Compare(T? x, T? y)
            {
                return _comparer.Compare(y, x);
            }
        }

        private class ProjectionComparer<TElement, TKey> : IComparer<TElement>
        {
            private readonly Func<TElement?, TKey?> _keySelector;
            private readonly IComparer<TKey> _comparer;
            public ProjectionComparer(Func<TElement?, TKey?> keySelector, IComparer<TKey> comparer)
            {
                _keySelector = keySelector;
                _comparer = comparer;
            }
            public int Compare(TElement? x, TElement? y)
            {
                return _comparer.Compare(_keySelector(x), _keySelector(y));
            }
        }

        private static void SortUsingBufferType<T>(this IGrailSort<T> sorter, Span<T> span, int length, SortingBufferType bufferType)
        {
            switch (bufferType)
            {
                case SortingBufferType.Dynamic:
                    sorter.GrailSortDynamicOop(span, 0, length);
                    break;
                case SortingBufferType.InPlace:
                    sorter.GrailSortInPlace(span, 0, length);
                    break;
                case SortingBufferType.Static:
                    sorter.GrailSortStaticOop(span, 0, length);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(bufferType));
            }
        }

        // LINQ Extensions

        public static IEnumerable<TSource> GrailOrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource?, TKey?> keySelector)
            => source.GrailOrderBy(keySelector, Comparer<TKey>.Default, SortingBufferType.Dynamic);

        public static IEnumerable<TSource> GrailOrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource?, TKey?> keySelector, IComparer<TKey> comparer)
            => source.GrailOrderBy(keySelector, comparer, SortingBufferType.Dynamic);

        public static IEnumerable<TSource> GrailOrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource?, TKey?> keySelector, SortingBufferType bufferType)
            => source.GrailOrderBy(keySelector, Comparer<TKey>.Default, bufferType);

        public static IEnumerable<TSource> GrailOrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource?, TKey?> keySelector, IComparer<TKey> comparer, SortingBufferType bufferType)
        {
            var list = source.ToList();
            var projectionComparer = new ProjectionComparer<TSource, TKey>(keySelector, comparer);
            var sorter = new GrailSort<TSource>(projectionComparer);
            var span = CollectionsMarshal.AsSpan(list);
            sorter.SortUsingBufferType(span, list.Count, bufferType);
            return list;
        }

        public static IEnumerable<TSource> GrailOrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource?, TKey?> keySelector)
            => source.GrailOrderBy(keySelector, new ReverseComparer<TKey>(Comparer<TKey>.Default), SortingBufferType.Dynamic);

        public static IEnumerable<TSource> GrailOrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource?, TKey?> keySelector, IComparer<TKey> comparer)
            => source.GrailOrderBy(keySelector, new ReverseComparer<TKey>(comparer), SortingBufferType.Dynamic);

        public static IEnumerable<TSource> GrailOrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource?, TKey?> keySelector, SortingBufferType bufferType)
            => source.GrailOrderBy(keySelector, new ReverseComparer<TKey>(Comparer<TKey>.Default), bufferType);

        // Array Extensions

        public static void GrailSort<T>(this T[] array) where T : IComparable<T>
            => array.GrailSort(Comparer<T>.Default, SortingBufferType.Dynamic);

        public static void GrailSort<T>(this T[] array, IComparer<T> comparer)
            => array.GrailSort(comparer, SortingBufferType.Dynamic);

        public static void GrailSort<T>(this T[] array, SortingBufferType bufferType)
            => array.GrailSort(Comparer<T>.Default, bufferType);

        public static void GrailSort<T>(this T[] array, IComparer<T> comparer, SortingBufferType bufferType)
        {
            if (array.Length <= 1) return;
            var span = array.AsSpan();
            var sorter = new GrailSort<T>(comparer);
            sorter.SortUsingBufferType(span, array.Length, bufferType);
        }

        public static void GrailSort<T, TKey>(this T[] array, Func<T?, TKey?> keySelector)
            => array.GrailSort(keySelector, Comparer<TKey>.Default, SortingBufferType.Dynamic);

        public static void GrailSort<T, TKey>(this T[] array, Func<T?, TKey?> keySelector, IComparer<TKey> comparer)
            => array.GrailSort(keySelector, comparer, SortingBufferType.Dynamic);

        public static void GrailSort<T, TKey>(this T[] array, Func<T?, TKey?> keySelector, SortingBufferType bufferType)
            => array.GrailSort(keySelector, Comparer<TKey>.Default, bufferType);

        public static void GrailSort<T, TKey>(this T[] array, Func<T?, TKey?> keySelector, IComparer<TKey> comparer, SortingBufferType bufferType)
        {
            if (array.Length <= 1) return;
            var projectionComparer = new ProjectionComparer<T, TKey>(keySelector, comparer);
            var sorter = new GrailSort<T>(projectionComparer);
            var span = array.AsSpan();
            sorter.SortUsingBufferType(span, array.Length, bufferType);
        }

        // List Extensions

        public static void GrailSort<T>(this List<T> list)
            => list.GrailSort(Comparer<T>.Default, SortingBufferType.Dynamic);

        public static void GrailSort<T>(this List<T> list, IComparer<T> comparer)
            => list.GrailSort(comparer, SortingBufferType.Dynamic);

        public static void GrailSort<T>(this List<T> list, SortingBufferType bufferType)
            => list.GrailSort(Comparer<T>.Default, bufferType);

        public static void GrailSort<T>(this List<T> list, IComparer<T> comparer, SortingBufferType bufferType)
        {
            if (list.Count <= 1) return;
            var span = CollectionsMarshal.AsSpan(list);
            var sorter = new GrailSort<T>(comparer);
            sorter.SortUsingBufferType(span, list.Count, bufferType);
        }

        public static void GrailSort<T, TKey>(this List<T> list, Func<T?, TKey?> keySelector)
            => list.GrailSort(keySelector, Comparer<TKey>.Default, SortingBufferType.Dynamic);

        public static void GrailSort<T, TKey>(this List<T> list, Func<T?, TKey?> keySelector, IComparer<TKey> comparer)
            => list.GrailSort(keySelector, comparer, SortingBufferType.Dynamic);

        public static void GrailSort<T, TKey>(this List<T> list, Func<T?, TKey?> keySelector, SortingBufferType bufferType)
            => list.GrailSort(keySelector, Comparer<TKey>.Default, bufferType);

        public static void GrailSort<T, TKey>(this List<T> list, Func<T?, TKey?> keySelector, IComparer<TKey> comparer, SortingBufferType bufferType)
        {
            if (list.Count <= 1) return;
            var projectionComparer = new ProjectionComparer<T, TKey>(keySelector, comparer);
            var sorter = new GrailSort<T>(projectionComparer);
            var span = CollectionsMarshal.AsSpan(list);
            sorter.SortUsingBufferType(span, list.Count, bufferType);
        }
    }
}
