
namespace GrailSortCSharp
{
    public interface IGrailSort<T>
    {
        void GrailSortDynamicOop(Span<T> span, int start, int length);
        void GrailSortInPlace(Span<T> span, int start, int length);
        void GrailSortStaticOop(Span<T> span, int start, int length);
    }
}