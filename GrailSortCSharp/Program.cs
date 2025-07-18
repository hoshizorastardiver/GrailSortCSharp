namespace GrailSortCSharp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int[] arr = { 3, 2, 1 };

            var span = arr.AsSpan();
            span[2] = 4;
            Console.WriteLine(arr[2]);
        }
    }
}
