namespace GrailSortCSharp.Algorithm
{
    using System;
    using System.Collections.Generic;

    public sealed class GrailSort<T>
    {
        private readonly IComparer<T> _cmp;

        private const int GrailStaticExtBufferLen = 512;

        private T[] _extBuffer;
        private int _extBufferLen;

        private int _currBlockLen;
        private Subarray _currBlockOrigin;

        // Credit to phoenixbound for this clever idea
        private enum Subarray
        {
            Left,
            Right
        }

        public GrailSort(IComparer<T> cmp)
        {
            _extBuffer = Array.Empty<T>();
            _cmp = cmp ?? Comparer<T>.Default;
        }

        private static void GrailSwap(Span<T> span, int a, int b)
        {
            (span[a], span[b]) = (span[b], span[a]);
        }

        private static void GrailBlockSwap(Span<T> span, int a, int b, int blockLen)
        {
            for (var i = 0; i < blockLen; i++)
            {
                GrailSwap(span, a + i, b + i);
            }
        }

        // Swaps the order of two adjacent blocks whose lengths may or may not be equal.
        // Variant of the Gries-Mills algorithm, which is basically recursive block swaps.
        private static void GrailRotate(Span<T> span, int start, int leftLen, int rightLen)
        {
            while (leftLen > 0 && rightLen > 0)
            {
                if (leftLen <= rightLen)
                {
                    GrailBlockSwap(span, start, start + leftLen, leftLen);
                    start += leftLen;
                    rightLen -= leftLen;
                }
                else
                {
                    GrailBlockSwap(span, start + leftLen - rightLen, start + leftLen, rightLen);
                    leftLen -= rightLen;
                }
            }
        }

        // Variant of Insertion Sort that utilizes swaps instead of overwrites.
        // Also known as "Optimized Gnomesort".
        private static void GrailInsertSort(Span<T> span, int start, int length, IComparer<T> cmp)
        {
            for (var item = 1; item < length; item++)
            {
                var left = start + item - 1;
                var right = start + item;

                while (left >= start && cmp.Compare(span[left], span[right]) > 0)
                {
                    GrailSwap(span, left, right);
                    left--;
                    right--;
                }
            }
        }

        private static int GrailBinarySearchLeft(Span<T> span, int start, int length, T target, IComparer<T> cmp)
        {
            var left = 0;
            var right = length;

            while (left < right)
            {
                // equivalent to (left + right) / 2 with added overflow protection
                var middle = left + (right - left) / 2;

                if (cmp.Compare(span[start + middle], target) < 0)
                {
                    left = middle + 1;
                }
                else
                {
                    right = middle;
                }
            }

            return left;
        }

        // Credit to Anonymous0726 for debugging
        private static int GrailBinarySearchRight(Span<T> span, int start, int length, T target, IComparer<T> cmp)
        {
            var left = 0;
            var right = length;

            while (left < right)
            {
                // equivalent to (left + right) / 2 with added overflow protection
                var middle = left + (right - left) / 2;

                if (cmp.Compare(span[start + middle], target) > 0)
                {
                    right = middle;
                }
                else
                {
                    left = middle + 1;
                }
            }
            return right;
        }

        // cost: 2 * length + idealKeys^2 / 2
        private static int GrailCollectKeys(Span<T> span, int start, int length, int idealKeys, IComparer<T> cmp)
        {
            var keysFound = 1; // by itself, the first item in the array is our first unique key
            var firstKey = 0; // the first item in the array is at the first position in the array
            var currKey = 1; // the index used for finding potentially unique items ("keys") in the array

            while (currKey < length && keysFound < idealKeys)
            {
                // Find the location in the key-buffer where our current key can be inserted in sorted order.
                // If the key at insertPos is equal to currKey, then currKey isn't unique and we move on.
                var insertPos = GrailBinarySearchLeft(span, start + firstKey, keysFound, span[start + currKey], cmp);

                // The second part of this conditional does the equal check we were just talking about; however,
                // if currKey is larger than everything in the key-buffer (meaning insertPos == keysFound),
                // then that also tells us it wasn't *equal* to anything in the key-buffer.
                if (insertPos == keysFound || cmp.Compare(span[start + currKey], span[start + firstKey + insertPos]) != 0)
                {
                    // First, rotate the key-buffer over to currKey's immediate left...
                    GrailRotate(span, start + firstKey, keysFound, currKey - (firstKey + keysFound));

                    // Update the new position of firstKey...
                    firstKey = currKey - keysFound;

                    // Then, "insertion sort" currKey to its spot in the key-buffer!
                    GrailRotate(span, start + firstKey + insertPos, keysFound - insertPos, 1);

                    keysFound++;
                }
                currKey++;
            }

            // Bring however many keys we found back to the beginning of our array
            GrailRotate(span, start, firstKey, keysFound);
            return keysFound;
        }

        private static void GrailPairwiseSwaps(Span<T> span, int start, int length, IComparer<T> cmp)
        {
            var index = 1;
            for (; index < length; index += 2)
            {
                var left = start + index - 1;
                var right = start + index;

                if (cmp.Compare(span[left], span[right]) > 0)
                {
                    GrailSwap(span, left - 2, right);
                    GrailSwap(span, right - 2, left);
                }
                else
                {
                    GrailSwap(span, left - 2, left);
                    GrailSwap(span, right - 2, right);
                }
            }

            var single = start + index - 1;
            if (single < start + length)
            {
                GrailSwap(span, single - 2, single);
            }
        }

        private static void GrailPairwiseWrites(Span<T> span, int start, int length, IComparer<T> cmp)
        {
            var index = 1;
            for (; index < length; index += 2)
            {
                var left = start + index - 1;
                var right = start + index;

                if (cmp.Compare(span[left], span[right]) > 0)
                {
                    span[left - 2] = span[right];
                    span[right - 2] = span[left];
                }
                else
                {
                    span[left - 2] = span[left];
                    span[right - 2] = span[right];
                }
            }

            var single = start + index - 1;
            if (single < start + length)
            {
                span[single - 2] = span[single];
            }
        }

        private static void GrailMergeForwards(Span<T> span, int start, int leftLen, int rightLen, int bufferOffset, IComparer<T> cmp)
        {
            var buffer = start - bufferOffset;
            var left = start;
            var middle = start + leftLen;
            var right = middle;
            var end = middle + rightLen;

            while (right < end)
            {
                if (left == middle || cmp.Compare(span[left], span[right]) > 0)
                {
                    GrailSwap(span, buffer, right);
                    right++;
                }
                else
                {
                    GrailSwap(span, buffer, left);
                    left++;
                }
                buffer++;
            }

            if (buffer != left)
            {
                GrailBlockSwap(span, buffer, left, middle - left);
            }
        }

        // credit to 666666t for thorough bug-checking/fixing
        private static void GrailMergeBackwards(Span<T> span, int start, int leftLen, int rightLen, int bufferOffset, IComparer<T> cmp)
        {
            var end = start - 1;
            var left = end + leftLen;
            var middle = left;
            var right = middle + rightLen;
            var buffer = right + bufferOffset;

            while (left > end)
            {
                if (right == middle || cmp.Compare(span[left], span[right]) > 0)
                {
                    GrailSwap(span, buffer, left);
                    left--;
                }
                else
                {
                    GrailSwap(span, buffer, right);
                    right--;
                }
                buffer--;
            }

            if (right != buffer)
            {
                while (right > middle)
                {
                    GrailSwap(span, buffer, right);
                    buffer--;
                    right--;
                }
            }
        }

        private static void GrailMergeOutOfPlace(Span<T> span, int start, int leftLen, int rightLen, int bufferOffset, IComparer<T> cmp)
        {
            var buffer = start - bufferOffset;
            var left = start;
            var middle = start + leftLen;
            var right = middle;
            var end = middle + rightLen;

            while (right < end)
            {
                if (left == middle || cmp.Compare(span[left], span[right]) > 0)
                {
                    span[buffer] = span[right];
                    right++;
                }
                else
                {
                    span[buffer] = span[left];
                    left++;
                }
                buffer++;
            }

            if (buffer != left)
            {
                while (left < middle)
                {
                    span[buffer] = span[left];
                    buffer++;
                    left++;
                }
            }
        }

        private static void GrailBuildInPlace(Span<T> span, int start, int length, int currentLen, int bufferLen, IComparer<T> cmp)
        {
            for (var mergeLen = currentLen; mergeLen < bufferLen; mergeLen *= 2)
            {
                var fullMerge = 2 * mergeLen;
                var mergeEnd = start + length - fullMerge;
                var bufferOffset = mergeLen;

                int mergeIndex;
                for (mergeIndex = start; mergeIndex <= mergeEnd; mergeIndex += fullMerge)
                {
                    GrailMergeForwards(span, mergeIndex, mergeLen, mergeLen, bufferOffset, cmp);
                }

                var leftOver = length - (mergeIndex - start);

                if (leftOver > mergeLen)
                {
                    GrailMergeForwards(span, mergeIndex, mergeLen, leftOver - mergeLen, bufferOffset, cmp);
                }
                else
                {
                    GrailRotate(span, mergeIndex - mergeLen, mergeLen, leftOver);
                }
                start -= mergeLen;
            }

            var finalFullMerge = 2 * bufferLen;
            var lastBlock = length % finalFullMerge;
            var lastOffset = start + length - lastBlock;

            if (lastBlock <= bufferLen)
            {
                GrailRotate(span, lastOffset, lastBlock, bufferLen);
            }
            else
            {
                GrailMergeBackwards(span, lastOffset, bufferLen, lastBlock - bufferLen, bufferLen, cmp);
            }

            for (var mergeIndex = lastOffset - finalFullMerge; mergeIndex >= start; mergeIndex -= finalFullMerge)
            {
                GrailMergeBackwards(span, mergeIndex, bufferLen, bufferLen, bufferLen, cmp);
            }
        }

        private void GrailBuildOutOfPlace(Span<T> span, int start, int length, int bufferLen, int extLen, IComparer<T> cmp)
        {
            span.Slice(start - extLen, extLen).CopyTo(_extBuffer.AsSpan());

            GrailPairwiseWrites(span, start, length, cmp);
            start -= 2;

            int mergeLen;
            for (mergeLen = 2; mergeLen < extLen; mergeLen *= 2)
            {
                var fullMerge = 2 * mergeLen;
                var mergeEnd = start + length - fullMerge;
                var bufferOffset = mergeLen;

                int mergeIndex;
                for (mergeIndex = start; mergeIndex <= mergeEnd; mergeIndex += fullMerge)
                {
                    GrailMergeOutOfPlace(span, mergeIndex, mergeLen, mergeLen, bufferOffset, cmp);
                }

                var leftOver = length - (mergeIndex - start);

                if (leftOver > mergeLen)
                {
                    GrailMergeOutOfPlace(span, mergeIndex, mergeLen, leftOver - mergeLen, bufferOffset, cmp);
                }
                else
                {
                    span.Slice(mergeIndex, leftOver).CopyTo(span.Slice(mergeIndex - mergeLen, leftOver));
                }
                start -= mergeLen;
            }

            _extBuffer.AsSpan(0, extLen).CopyTo(span.Slice(start + length, extLen));
            GrailBuildInPlace(span, start, length, mergeLen, bufferLen, cmp);
        }

        private void GrailBuildBlocks(Span<T> span, int start, int length, int bufferLen, IComparer<T> cmp)
        {
            if (_extBuffer.Length != 0)
            {
                int extLen;
                if (bufferLen < _extBufferLen)
                {
                    extLen = bufferLen;
                }
                else
                {
                    // max power of 2 -- just in case
                    extLen = 1;
                    while (extLen * 2 <= _extBufferLen)
                    {
                        extLen *= 2;
                    }
                }
                GrailBuildOutOfPlace(span, start, length, bufferLen, extLen, cmp);
            }
            else
            {
                GrailPairwiseSwaps(span, start, length, cmp);
                GrailBuildInPlace(span, start - 2, length, 2, bufferLen, cmp);
            }
        }

        private static int GrailBlockSelectSort(Span<T> span, int firstKey, int start, int medianKey, int blockCount, int blockLen, IComparer<T> cmp)
        {
            for (var firstBlock = 0; firstBlock < blockCount; firstBlock++)
            {
                var selectBlock = firstBlock;

                for (var currBlock = firstBlock + 1; currBlock < blockCount; currBlock++)
                {
                    var compare = cmp.Compare(span[start + currBlock * blockLen], span[start + selectBlock * blockLen]);
                    if (compare < 0 || compare == 0 && cmp.Compare(span[firstKey + currBlock], span[firstKey + selectBlock]) < 0)
                    {
                        selectBlock = currBlock;
                    }
                }

                if (selectBlock != firstBlock)
                {
                    GrailBlockSwap(span, start + firstBlock * blockLen, start + selectBlock * blockLen, blockLen);
                    GrailSwap(span, firstKey + firstBlock, firstKey + selectBlock);

                    // MASSIVE, MASSIVE credit to lovebuny for figuring this one out!
                    if (medianKey == firstBlock)
                    {
                        medianKey = selectBlock;
                    }
                    else if (medianKey == selectBlock)
                    {
                        medianKey = firstBlock;
                    }
                }
            }

            return medianKey;
        }

        private static void GrailInPlaceBufferReset(Span<T> span, int start, int length, int bufferOffset)
        {
            var buffer = start + length - 1;
            var index = buffer - bufferOffset;
            while (buffer >= start)
            {
                GrailSwap(span, index, buffer);
                buffer--;
                index--;
            }
        }

        private static void GrailOutOfPlaceBufferReset(Span<T> span, int start, int length, int bufferOffset)
        {
            var buffer = start + length - 1;
            var index = buffer - bufferOffset;
            while (buffer >= start)
            {
                span[buffer] = span[index];
                buffer--;
                index--;
            }
        }

        private static void GrailInPlaceBufferRewind(Span<T> span, int start, int leftBlock, int buffer)
        {
            while (leftBlock >= start)
            {
                GrailSwap(span, buffer, leftBlock);
                leftBlock--;
                buffer--;
            }
        }

        private static void GrailOutOfPlaceBufferRewind(Span<T> span, int start, int leftBlock, int buffer)
        {
            while (leftBlock >= start)
            {
                span[buffer] = span[leftBlock];
                leftBlock--;
                buffer--;
            }
        }

        private static Subarray GrailGetSubarray(Span<T> span, int currentKey, int medianKey, IComparer<T> cmp)
        {
            return cmp.Compare(span[currentKey], span[medianKey]) < 0 ? Subarray.Left : Subarray.Right;
        }

        private static int GrailCountLastMergeBlocks(Span<T> span, int offset, int blockCount, int blockLen, IComparer<T> cmp)
        {
            var blocksToMerge = 0;
            var lastRightFrag = offset + blockCount * blockLen;
            var prevLeftBlock = lastRightFrag - blockLen;

            while (blocksToMerge < blockCount && cmp.Compare(span[lastRightFrag], span[prevLeftBlock]) < 0)
            {
                blocksToMerge++;
                prevLeftBlock -= blockLen;
            }

            return blocksToMerge;
        }

        private void GrailSmartMerge(Span<T> span, int start, int leftLen, Subarray leftOrigin, int rightLen, int bufferOffset, IComparer<T> cmp)
        {
            var buffer = start - bufferOffset;
            var left = start;
            var middle = start + leftLen;
            var right = middle;
            var end = middle + rightLen;

            if (leftOrigin == Subarray.Left)
            {
                while (left < middle && right < end)
                {
                    if (cmp.Compare(span[left], span[right]) <= 0)
                    {
                        GrailSwap(span, buffer, left);
                        left++;
                    }
                    else
                    {
                        GrailSwap(span, buffer, right);
                        right++;
                    }
                    buffer++;
                }
            }
            else
            {
                while (left < middle && right < end)
                {
                    if (cmp.Compare(span[left], span[right]) < 0)
                    {
                        GrailSwap(span, buffer, left);
                        left++;
                    }
                    else
                    {
                        GrailSwap(span, buffer, right);
                        right++;
                    }
                    buffer++;
                }
            }

            if (left < middle)
            {
                _currBlockLen = middle - left;
                GrailInPlaceBufferRewind(span, left, middle - 1, end - 1);
            }
            else
            {
                _currBlockLen = end - right;
                _currBlockOrigin = leftOrigin == Subarray.Left ? Subarray.Right : Subarray.Left;
            }
        }

        private void GrailSmartLazyMerge(Span<T> span, int start, int leftLen, Subarray leftOrigin, int rightLen, IComparer<T> cmp)
        {
            var middle = start + leftLen;

            if (leftOrigin == Subarray.Left)
            {
                if (cmp.Compare(span[middle - 1], span[middle]) > 0)
                {
                    while (leftLen != 0)
                    {
                        var mergeLen = GrailBinarySearchLeft(span, middle, rightLen, span[start], cmp);
                        if (mergeLen != 0)
                        {
                            GrailRotate(span, start, leftLen, mergeLen);
                            start += mergeLen;
                            middle += mergeLen;
                            rightLen -= mergeLen;
                        }

                        if (rightLen == 0)
                        {
                            _currBlockLen = leftLen;
                            return;
                        }

                        do
                        {
                            start++;
                            leftLen--;
                        } while (leftLen != 0 && cmp.Compare(span[start], span[middle]) <= 0);
                    }
                }
            }
            else
            {
                if (cmp.Compare(span[middle - 1], span[middle]) >= 0)
                {
                    while (leftLen != 0)
                    {
                        var mergeLen = GrailBinarySearchRight(span, middle, rightLen, span[start], cmp);
                        if (mergeLen != 0)
                        {
                            GrailRotate(span, start, leftLen, mergeLen);
                            start += mergeLen;
                            middle += mergeLen;
                            rightLen -= mergeLen;
                        }

                        if (rightLen == 0)
                        {
                            _currBlockLen = leftLen;
                            return;
                        }

                        do
                        {
                            start++;
                            leftLen--;
                        } while (leftLen != 0 && cmp.Compare(span[start], span[middle]) < 0);
                    }
                }
            }

            _currBlockLen = rightLen;
            _currBlockOrigin = leftOrigin == Subarray.Left ? Subarray.Right : Subarray.Left;
        }

        private void GrailSmartMergeOutOfPlace(Span<T> span, int start, int leftLen, Subarray leftOrigin, int rightLen, int bufferOffset, IComparer<T> cmp)
        {
            var buffer = start - bufferOffset;
            var left = start;
            var middle = start + leftLen;
            var right = middle;
            var end = middle + rightLen;

            if (leftOrigin == Subarray.Left)
            {
                while (left < middle && right < end)
                {
                    if (cmp.Compare(span[left], span[right]) <= 0)
                    {
                        span[buffer] = span[left];
                        left++;
                    }
                    else
                    {
                        span[buffer] = span[right];
                        right++;
                    }
                    buffer++;
                }
            }
            else
            {
                while (left < middle && right < end)
                {
                    if (cmp.Compare(span[left], span[right]) < 0)
                    {
                        span[buffer] = span[left];
                        left++;
                    }
                    else
                    {
                        span[buffer] = span[right];
                        right++;
                    }
                    buffer++;
                }
            }

            if (left < middle)
            {
                _currBlockLen = middle - left;
                GrailOutOfPlaceBufferRewind(span, left, middle - 1, end - 1);
            }
            else
            {
                _currBlockLen = end - right;
                _currBlockOrigin = leftOrigin == Subarray.Left ? Subarray.Right : Subarray.Left;
            }
        }

        private void GrailMergeBlocks(Span<T> span, int firstKey, int medianKey, int start, int blockCount, int blockLen, int lastMergeBlocks, int lastLen, IComparer<T> cmp)
        {
            _currBlockLen = blockLen;
            _currBlockOrigin = GrailGetSubarray(span, firstKey, medianKey, cmp);

            for (var keyIndex = 1; keyIndex < blockCount; keyIndex++)
            {
                var currBlock = start + keyIndex * blockLen - _currBlockLen;
                var nextBlockOrigin = GrailGetSubarray(span, firstKey + keyIndex, medianKey, cmp);

                if (nextBlockOrigin == _currBlockOrigin)
                {
                    var buffer = currBlock - blockLen;
                    GrailBlockSwap(span, buffer, currBlock, _currBlockLen);
                    _currBlockLen = blockLen;
                }
                else
                {
                    GrailSmartMerge(span, currBlock, _currBlockLen, _currBlockOrigin, blockLen, blockLen, cmp);
                }
            }

            var finalBlock = start + blockCount * blockLen;
            var fBCurrBlock = finalBlock - _currBlockLen;
            var fBBuffer = fBCurrBlock - blockLen;

            if (lastLen != 0)
            {
                if (_currBlockOrigin == Subarray.Right)
                {
                    GrailBlockSwap(span, fBBuffer, fBCurrBlock, _currBlockLen);
                    _currBlockLen = blockLen * lastMergeBlocks;
                    _currBlockOrigin = Subarray.Left;
                    GrailMergeForwards(span, finalBlock, _currBlockLen, lastLen, blockLen, cmp);
                }
                else
                {
                    _currBlockLen += blockLen * lastMergeBlocks;
                    GrailMergeForwards(span, fBCurrBlock, _currBlockLen, lastLen, blockLen, cmp);
                }
            }
            else
            {
                GrailBlockSwap(span, fBBuffer, fBCurrBlock, _currBlockLen);
            }
        }

        private void GrailLazyMergeBlocks(Span<T> span, int firstKey, int medianKey, int start, int blockCount, int blockLen, int lastMergeBlocks, int lastLen, IComparer<T> cmp)
        {
            _currBlockLen = blockLen;
            _currBlockOrigin = GrailGetSubarray(span, firstKey, medianKey, cmp);

            for (var keyIndex = 1; keyIndex < blockCount; keyIndex++)
            {
                var currBlock = start + keyIndex * blockLen - _currBlockLen;
                var nextBlockOrigin = GrailGetSubarray(span, firstKey + keyIndex, medianKey, cmp);

                if (nextBlockOrigin == _currBlockOrigin)
                {
                    _currBlockLen = blockLen;
                }
                else
                {
                    if (blockLen != 0 && _currBlockLen != 0)
                    {
                        GrailSmartLazyMerge(span, currBlock, _currBlockLen, _currBlockOrigin, blockLen, cmp);
                    }
                }
            }

            var finalBlock = start + blockCount * blockLen;
            var fBCurrBlock = finalBlock - _currBlockLen;

            if (lastLen != 0)
            {
                if (_currBlockOrigin == Subarray.Right)
                {
                    _currBlockLen = blockLen * lastMergeBlocks;
                    _currBlockOrigin = Subarray.Left;
                    GrailLazyMerge(span, finalBlock, _currBlockLen, lastLen, cmp);
                }
                else
                {
                    _currBlockLen += blockLen * lastMergeBlocks;
                    GrailLazyMerge(span, fBCurrBlock, _currBlockLen, lastLen, cmp);
                }
            }
        }

        private void GrailMergeBlocksOutOfPlace(Span<T> span, int firstKey, int medianKey, int start, int blockCount, int blockLen, int lastMergeBlocks, int lastLen, IComparer<T> cmp)
        {
            _currBlockLen = blockLen;
            _currBlockOrigin = GrailGetSubarray(span, firstKey, medianKey, cmp);

            for (var keyIndex = 1; keyIndex < blockCount; keyIndex++)
            {
                var currBlock = start + keyIndex * blockLen - _currBlockLen;
                var nextBlockOrigin = GrailGetSubarray(span, firstKey + keyIndex, medianKey, cmp);

                if (nextBlockOrigin == _currBlockOrigin)
                {
                    var buffer = currBlock - blockLen;
                    span.Slice(currBlock, _currBlockLen).CopyTo(span.Slice(buffer, _currBlockLen));
                    _currBlockLen = blockLen;
                }
                else
                {
                    GrailSmartMergeOutOfPlace(span, currBlock, _currBlockLen, _currBlockOrigin, blockLen, blockLen, cmp);
                }
            }

            var finalBlock = start + blockCount * blockLen;
            var fBCurrBlock = finalBlock - _currBlockLen;
            var fBBuffer = fBCurrBlock - blockLen;

            if (lastLen != 0)
            {
                if (_currBlockOrigin == Subarray.Right)
                {
                    span.Slice(fBCurrBlock, _currBlockLen).CopyTo(span.Slice(fBBuffer, _currBlockLen));
                    _currBlockLen = blockLen * lastMergeBlocks;
                    _currBlockOrigin = Subarray.Left;
                    GrailMergeOutOfPlace(span, finalBlock, _currBlockLen, lastLen, blockLen, cmp);
                }
                else
                {
                    _currBlockLen += blockLen * lastMergeBlocks;
                    GrailMergeOutOfPlace(span, fBCurrBlock, _currBlockLen, lastLen, blockLen, cmp);
                }
            }
            else
            {
                span.Slice(fBCurrBlock, _currBlockLen).CopyTo(span.Slice(fBBuffer, _currBlockLen));
            }
        }

        private void GrailCombineInPlace(Span<T> span, int firstKey, int start, int length, int subarrayLen, int blockLen, int mergeCount, int lastSubarrays, bool buffer)
        {
            var cmp = _cmp;
            var fullMerge = 2 * subarrayLen;
            var blockCount = fullMerge / blockLen;

            for (var mergeIndex = 0; mergeIndex < mergeCount; mergeIndex++)
            {
                var offset = start + mergeIndex * fullMerge;
                GrailInsertSort(span, firstKey, blockCount, cmp);
                var medianKey = subarrayLen / blockLen;
                medianKey = GrailBlockSelectSort(span, firstKey, offset, medianKey, blockCount, blockLen, cmp);

                if (buffer)
                {
                    GrailMergeBlocks(span, firstKey, firstKey + medianKey, offset, blockCount, blockLen, 0, 0, cmp);
                }
                else
                {
                    GrailLazyMergeBlocks(span, firstKey, firstKey + medianKey, offset, blockCount, blockLen, 0, 0, cmp);
                }
            }

            if (lastSubarrays != 0)
            {
                var offset = start + mergeCount * fullMerge;
                var lastBlockCount = lastSubarrays / blockLen;
                GrailInsertSort(span, firstKey, lastBlockCount + 1, cmp);
                var medianKey = subarrayLen / blockLen;
                medianKey = GrailBlockSelectSort(span, firstKey, offset, medianKey, lastBlockCount, blockLen, cmp);
                var lastFragment = lastSubarrays - lastBlockCount * blockLen;
                int lastMergeBlocks;
                if (lastFragment != 0)
                {
                    lastMergeBlocks = GrailCountLastMergeBlocks(span, offset, lastBlockCount, blockLen, cmp);
                }
                else
                {
                    lastMergeBlocks = 0;
                }

                var smartMerges = lastBlockCount - lastMergeBlocks;
                if (smartMerges == 0)
                {
                    var leftLen = lastMergeBlocks * blockLen;
                    if (buffer)
                    {
                        GrailMergeForwards(span, offset, leftLen, lastFragment, blockLen, cmp);
                    }
                    else
                    {
                        GrailLazyMerge(span, offset, leftLen, lastFragment, cmp);
                    }
                }
                else
                {
                    if (buffer)
                    {
                        GrailMergeBlocks(span, firstKey, firstKey + medianKey, offset, smartMerges, blockLen, lastMergeBlocks, lastFragment, cmp);
                    }
                    else
                    {
                        GrailLazyMergeBlocks(span, firstKey, firstKey + medianKey, offset, smartMerges, blockLen, lastMergeBlocks, lastFragment, cmp);
                    }
                }
            }

            if (buffer)
            {
                GrailInPlaceBufferReset(span, start, length, blockLen);
            }
        }

        private void GrailCombineOutOfPlace(Span<T> span, int firstKey, int start, int length, int subarrayLen, int blockLen, int mergeCount, int lastSubarrays)
        {
            var cmp = _cmp;
            span.Slice(start - blockLen, blockLen).CopyTo(_extBuffer.AsSpan());
            var fullMerge = 2 * subarrayLen;
            var blockCount = fullMerge / blockLen;

            for (var mergeIndex = 0; mergeIndex < mergeCount; mergeIndex++)
            {
                var offset = start + mergeIndex * fullMerge;
                GrailInsertSort(span, firstKey, blockCount, cmp);
                var medianKey = subarrayLen / blockLen;
                medianKey = GrailBlockSelectSort(span, firstKey, offset, medianKey, blockCount, blockLen, cmp);
                GrailMergeBlocksOutOfPlace(span, firstKey, firstKey + medianKey, offset, blockCount, blockLen, 0, 0, cmp);
            }

            if (lastSubarrays != 0)
            {
                var offset = start + mergeCount * fullMerge;
                var lastBlockCount = lastSubarrays / blockLen;
                GrailInsertSort(span, firstKey, lastBlockCount + 1, cmp);
                var medianKey = subarrayLen / blockLen;
                medianKey = GrailBlockSelectSort(span, firstKey, offset, medianKey, lastBlockCount, blockLen, cmp);
                var lastFragment = lastSubarrays - lastBlockCount * blockLen;
                int lastMergeBlocks;
                if (lastFragment != 0)
                {
                    lastMergeBlocks = GrailCountLastMergeBlocks(span, offset, lastBlockCount, blockLen, cmp);
                }
                else
                {
                    lastMergeBlocks = 0;
                }

                var smartMerges = lastBlockCount - lastMergeBlocks;
                if (smartMerges == 0)
                {
                    var leftLen = lastMergeBlocks * blockLen;
                    GrailMergeOutOfPlace(span, offset, leftLen, lastFragment, blockLen, cmp);
                }
                else
                {
                    GrailMergeBlocksOutOfPlace(span, firstKey, firstKey + medianKey, offset, smartMerges, blockLen, lastMergeBlocks, lastFragment, cmp);
                }
            }

            GrailOutOfPlaceBufferReset(span, start, length, blockLen);
            _extBuffer.AsSpan(0, blockLen).CopyTo(span.Slice(start - blockLen, blockLen));
        }

        private void GrailCombineBlocks(Span<T> span, int firstKey, int start, int length, int subarrayLen, int blockLen, bool buffer)
        {
            var fullMerge = 2 * subarrayLen;
            var mergeCount = length / fullMerge;
            var lastSubarrays = length - fullMerge * mergeCount;

            if (lastSubarrays <= subarrayLen)
            {
                length -= lastSubarrays;
                lastSubarrays = 0;
            }

            if (buffer && blockLen <= _extBufferLen)
            {
                GrailCombineOutOfPlace(span, firstKey, start, length, subarrayLen, blockLen, mergeCount, lastSubarrays);
            }
            else
            {
                GrailCombineInPlace(span, firstKey, start, length, subarrayLen, blockLen, mergeCount, lastSubarrays, buffer);
            }
        }

        private static void GrailLazyMerge(Span<T> span, int start, int leftLen, int rightLen, IComparer<T> cmp)
        {
            if (leftLen < rightLen)
            {
                var middle = start + leftLen;
                while (leftLen != 0)
                {
                    var mergeLen = GrailBinarySearchLeft(span, middle, rightLen, span[start], cmp);
                    if (mergeLen != 0)
                    {
                        GrailRotate(span, start, leftLen, mergeLen);
                        start += mergeLen;
                        middle += mergeLen;
                        rightLen -= mergeLen;
                    }

                    if (rightLen == 0) break;

                    do
                    {
                        start++;
                        leftLen--;
                    } while (leftLen != 0 && cmp.Compare(span[start], span[middle]) <= 0);
                }
            }
            else
            {
                var end = start + leftLen + rightLen - 1;
                while (rightLen != 0)
                {
                    var mergeLen = GrailBinarySearchRight(span, start, leftLen, span[end], cmp);
                    if (mergeLen != leftLen)
                    {
                        GrailRotate(span, start + mergeLen, leftLen - mergeLen, rightLen);
                        end -= leftLen - mergeLen;
                        leftLen = mergeLen;
                    }

                    if (leftLen == 0) break;

                    var middle = start + leftLen;
                    do
                    {
                        rightLen--;
                        end--;
                    } while (rightLen != 0 && cmp.Compare(span[middle - 1], span[end]) <= 0);
                }
            }
        }

        private static void GrailLazyStableSort(Span<T> span, int start, int length, IComparer<T> cmp)
        {
            for (var index = 1; index < length; index += 2)
            {
                var left = start + index - 1;
                var right = start + index;
                if (cmp.Compare(span[left], span[right]) > 0)
                {
                    GrailSwap(span, left, right);
                }
            }

            for (var mergeLen = 2; mergeLen < length; mergeLen *= 2)
            {
                var fullMerge = 2 * mergeLen;
                int mergeIndex;
                var mergeEnd = length - fullMerge;
                for (mergeIndex = 0; mergeIndex <= mergeEnd; mergeIndex += fullMerge)
                {
                    GrailLazyMerge(span, start + mergeIndex, mergeLen, mergeLen, cmp);
                }

                var leftOver = length - mergeIndex;
                if (leftOver > mergeLen)
                {
                    GrailLazyMerge(span, start + mergeIndex, mergeLen, leftOver - mergeLen, cmp);
                }
            }
        }

        private void GrailCommonSort(Span<T> span, int start, int length, T[] extBuffer, int extBufferLen)
        {
            if (length < 16)
            {
                GrailInsertSort(span, start, length, _cmp);
                return;
            }

            var blockLen = 1;
            while (blockLen * blockLen < length)
            {
                blockLen *= 2;
            }

            var keyLen = (length - 1) / blockLen + 1;
            var idealKeys = keyLen + blockLen;
            var keysFound = GrailCollectKeys(span, start, length, idealKeys, _cmp);

            bool idealBuffer;
            if (keysFound < idealKeys)
            {
                if (keysFound < 4)
                {
                    // STRATEGY 3: No block swaps or scrolling buffer; resort to Lazy Stable Sort
                    GrailLazyStableSort(span, start, length, _cmp);
                    return;
                }

                // STRATEGY 2: Block swaps with small scrolling buffer and/or lazy merges
                keyLen = blockLen;
                blockLen = 0;
                idealBuffer = false;
                while (keyLen > keysFound)
                {
                    keyLen /= 2;
                }
            }
            else
            {
                // STRATEGY 1: Block swaps with scrolling buffer
                idealBuffer = true;
            }

            var bufferEnd = blockLen + keyLen;
            var subarrayLen = idealBuffer ? blockLen : keyLen;

            if (idealBuffer && extBuffer.Length != 0)
            {
                _extBuffer = extBuffer;
                _extBufferLen = extBufferLen;
            }
            else
            {
                _extBuffer = Array.Empty<T>();
                _extBufferLen = 0;
            }

            GrailBuildBlocks(span, start + bufferEnd, length - bufferEnd, subarrayLen, _cmp);

            while (length - bufferEnd > 2 * subarrayLen)
            {
                subarrayLen *= 2;
                var currentBlockLen = blockLen;
                var scrollingBuffer = idealBuffer;

                if (!idealBuffer)
                {
                    var keyBuffer = keyLen / 2;
                    if (keyBuffer >= 2 * subarrayLen / keyBuffer)
                    {
                        currentBlockLen = keyBuffer;
                        scrollingBuffer = true;
                    }
                    else
                    {
                        currentBlockLen = 2 * subarrayLen / keyLen;
                    }
                }
                GrailCombineBlocks(span, start, start + bufferEnd, length - bufferEnd, subarrayLen, currentBlockLen, scrollingBuffer);
            }

            GrailInsertSort(span, start, bufferEnd, _cmp);
            GrailLazyMerge(span, start, bufferEnd, length - bufferEnd, _cmp);
        }

        public void GrailSortInPlace(Span<T> span, int start, int length)
        {
            GrailCommonSort(span, start, length, Array.Empty<T>(), 0);
        }

        public void GrailSortStaticOop(Span<T> span, int start, int length)
        {
            var buffer = new T[GrailStaticExtBufferLen];
            GrailCommonSort(span, start, length, buffer, GrailStaticExtBufferLen);
        }

        public void GrailSortDynamicOop(Span<T> span, int start, int length)
        {
            var bufferLen = 1;
            while (bufferLen * bufferLen < length)
            {
                bufferLen *= 2;
            }
            var buffer = new T[bufferLen];
            GrailCommonSort(span, start, length, buffer, bufferLen);
        }
    }
}
