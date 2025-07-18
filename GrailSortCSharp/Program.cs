using GrailSortCSharp.Extensions;

namespace GrailSortCSharp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int[] arr = { 3, 2, 1 };

            arr.GrailSort(GrailSortExtensions.SortingBufferType.InPlace);

            foreach (int x in arr)
            {
                Console.WriteLine(x);
            }
        }
    }
}
