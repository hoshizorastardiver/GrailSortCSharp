# GrailSort C#: A Stable, In-Place Sorting Algorithm in C#

<p align="center"><img width="512" height="512" alt="0b753261-0e02-428a-9b08-87ccd1857649" src="https://github.com/user-attachments/assets/7275e987-d99b-4100-bfb1-f50d928d7e05" /></p>

[![NuGet](https://img.shields.io/nuget/v/GrailSortCSharp.svg)](https://www.nuget.org/packages/GrailSortCSharp)
[![NuGet version](https://img.shields.io/nuget/v/GrailSortCSharp?color=ff4081&label=nuget%20version&logo=nuget&style=flat-square)]( https://www.nuget.org/packages/GrailSortCSharp/ )
[![NuGet downloads](https://img.shields.io/nuget/dt/GrailSortCSharp?color=ff4081&label=nuget%20downloads&logo=nuget&style=flat-square)]( https://www.nuget.org/packages/GrailSortCSharp/ )
![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/hoshizorastardiver/GrailSortCSharp/build-test-mudblazor.yml?branch=dev&logo=github&style=flat-square)
[![Codecov]( https://img.shields.io/codecov/c/github/hoshizorastardiver/GrailSortCSharp )](https://app.codecov.io/github/hoshizorastardiver/GrailSortCSharp )
[![GitHub]( https://img.shields.io/github/license/hoshizorastardiver/GrailSortCSharp?color=594ae2&logo=github&style=flat-square)]( https://github.com/hoshizorastardiver/GrailSortCSharp/blob/master/LICENSE )
[![GitHub Repo stars](https://img.shields.io/github/stars/hoshizorastardiver/GrailSortCSharp?color=594ae2&style=flat-square&logo=github)]( https://github.com/hoshizorastardiver/GrailSortCSharp/stargazers )
[![GitHub last commit](https://img.shields.io/github/last-commit/hoshizorastardiver/GrailSortCSharp?color=594ae2&style=flat-square&logo=github)]( https://github.com/hoshizorastardiver/GrailSortCSharp )
[![Contributors](https://img.shields.io/github/contributors/hoshizorastardiver/GrailSortCSharp?color=594ae2&style=flat-square&logo=github)]( https://github.com/hoshizorastardiver/GrailSortCSharp/graphs/contributors )
[![Discussions](https://img.shields.io/github/discussions/hoshizorastardiver/GrailSortCSharp?color=594ae2&logo=github&style=flat-square)]( https://github.com/hoshizorastardiver/GrailSortCSharp/discussions )
[![Discord](https://img.shields.io/discord/786656789310865418?color=%237289da&label=Discord&logo=discord&logoColor=%237289da&style=flat-square)]( https://discord.gg/GrailSortCSharp )

A set of extension methods that integrate the GrailSort algorithm into .NET collections and LINQ, providing high-performance, in-place sorting with flexible buffer strategies. Based on [Summer Dragonfly et al.'s Rewritten Grailsort for Java](https://github.com/HolyGrailSortProject/Rewritten-Grailsort). It has been retested and further optimized specifically for C#.

## Table of Contents

* [Installation](#installation)
* [Overview](#overview)
* [What Is GrailSort?](#what-is-grailsort)
* [Why Choose GrailSort Over C# Built-In Sort?](#why-choose-grailsort-over-c-built-in-sort)
* [Buffer Types](#buffer-types)
* [LINQ Extensions](#linq-extensions)
  * [GrailOrderBy](#grailorderby)
  * [GrailOrderByDescending](#grailorderbydescending)
* [Array Extensions](#array-extensions)
* [List Extensions](#list-extensions)
* [Examples](#examples)
* [License](#license)

## Installation

Install via NuGet:

```shell
Install-Package GrailSortCSharp
```

Or using the .NET CLI:

```shell
dotnet add package GrailSortCSharp
```

## Overview

This library provides extension methods for in-place sorting of arrays, lists, and LINQ queries using the GrailSort algorithm. GrailSort is a stable, efficient sorting algorithm that can operate with different buffer strategies:

* **Dynamic**: Allocates a temporary buffer at runtime.
* **Static**: Uses a pre-allocated internal buffer.
* **InPlace**: Performs the sort entirely in-place without extra memory.

The extensions wrap `GrailSort<T>` from `GrailSortCSharp.Algorithm` and expose familiar APIs similar to `Enumerable.OrderBy` and `List<T>.Sort`.

## What Is GrailSort?

GrailSort is a sophisticated variant of Merge Sort that achieves **stability**, **in-place sorting**, and **worst-case O(n log n)** performance. Unlike traditional Merge Sorts that require O(n) extra space, GrailSort cleverly utilizes the array itself to simulate buffer space, using a strategy based on collecting roughly **2 × √n unique elements**.

The algorithm proceeds by:

1. **Collecting Unique Elements**: Attempts to gather 2 × √n unique values at the front of the array. One half acts as a working buffer; the other half serves as keys to guide block merges.
2. **Buffered Merging**: Uses these elements to perform efficient merges of increasingly large subarrays.
3. **Block Merge Sort**: When the buffer becomes insufficient, the array is divided into √n-sized blocks, which are reordered using tagged block swaps.
4. **Fallback Logic**: If there aren't enough unique keys, GrailSort reverts to in-place, rotation-based merge techniques that preserve stability and maintain O(n log n) performance.

This design allows GrailSort to fall back gracefully in degenerate scenarios while remaining fast and space-efficient.

## Why Choose GrailSort Over C# Built-In Sort?

C#'s built-in `Array.Sort` and `List<T>.Sort` methods use **Introspective Sort**, a hybrid of **QuickSort**, **HeapSort**, and **Insertion Sort**. While this is fast on average, it has limitations:

* ❌ **Not Stable**: Equal elements may not preserve their original order.
* ❌ **Not In-Place in Practice**: Sorting large reference-type arrays or custom comparers may cause temporary allocations.
* ❌ **Poor Worst-Case Behavior**: Although introspection kicks in to switch to HeapSort when recursion gets deep, it's still vulnerable to crafted worst-case QuickSort inputs.

**GrailSort** addresses these shortcomings:

* ✅ **Stable**: Preserves the order of equal elements — critical for sorting objects with composite keys.
* ✅ **In-Place**: Fully supports in-place sorting (no heap allocations if `InPlace` buffer mode is selected).
* ✅ **Worst-Case O(n log n)**: Performs consistently well even on adversarial input.
* ✅ **Buffer-Configurable**: Lets you tune for memory vs. performance with `InPlace`, `Static`, or `Dynamic` strategies.
* ✅ **High Customizability**: Works well with custom comparers, key selectors, and LINQ-style usage.

## Buffer Types

```csharp
public enum SortingBufferType
{
    InPlace,
    Static,
    Dynamic
}
```

### 🔹 `InPlace`

Uses **no external buffer** (`buffer length = 0`).

  * Minimal memory overhead
  * Ideal for memory-constrained environments
  * Slightly slower
  * Relies on internal tricks (e.g., rotations and key-based swaps)

### 🔸 `Static`

Uses a **fixed-size buffer** of 512 elements (`GrailStaticExtBufferLen`).

  * Fixed memory usage
  * No repeated allocations for repeated sorts

### ⚡ `Dynamic`

Allocates a buffer of size approximately **√n**, rounded up to the nearest power of 2 (`bufferLen² ≥ length`).

  * Balances speed and memory usage
  * Scales well with various input sizes
  * Allocates a new buffer every sort
  * Still far less memory than algorithms like mergesort (O(√n) vs O(n))

  
## LINQ Extensions

### GrailOrderBy

```csharp
IEnumerable<TSource> GrailOrderBy<TSource, TKey>(
    this IEnumerable<TSource> source,
    Func<TSource, TKey> keySelector,
    IComparer<TKey>? comparer = null,
    SortingBufferType bufferType = SortingBufferType.Dynamic
)
```

Sorts an `IEnumerable<TSource>` by a key. Deferred execution: sorting occurs when the result is enumerated.

* `keySelector`: Projection function for sorting key.
* `comparer`: Key comparer (defaults to `Comparer<TKey>.Default`).
* `bufferType`: Chooses sorting buffer strategy.

### GrailOrderByDescending

```csharp
IEnumerable<TSource> GrailOrderByDescending<TSource, TKey>(
    this IEnumerable<TSource> source,
    Func<TSource, TKey> keySelector,
    IComparer<TKey>? comparer = null,
    SortingBufferType bufferType = SortingBufferType.Dynamic
)
```

Same as `GrailOrderBy`, but in descending order.

## Array\<T\> Extensions

```csharp
void GrailSort<T>(
    this T[] array,
    IComparer<T>? comparer = null,
    SortingBufferType bufferType = SortingBufferType.Dynamic
)

void GrailSort<T, TKey>(
    this T[] array,
    Func<T, TKey> keySelector,
    IComparer<TKey>? comparer = null,
    SortingBufferType bufferType = SortingBufferType.Dynamic
)
```

Sorts the array in-place. Overloads allow specifying a custom comparer or key selector.

## List\<T\> Extensions

```csharp
void GrailSort<T>(
    this List<T> list,
    IComparer<T>? comparer = null,
    SortingBufferType bufferType = SortingBufferType.Dynamic
)

void GrailSort<T, TKey>(
    this List<T> list,
    Func<T, TKey> keySelector,
    IComparer<TKey>? comparer = null,
    SortingBufferType bufferType = SortingBufferType.Dynamic
)
```

Sorts the `List<T>` in-place, with overloads similar to the array extensions.

## Examples
It can actually be used as a drop-in replacement for C# LINQ, Array.Sort<T>() and List<T>.Sort()
### Sorting an array of primitives

```csharp
int[] data = { 5, 3, 8, 1 };
data.GrailSort();              // Default dynamic buffer and natural order

data.GrailSort(SortingBufferType.InPlace); // In-place sorting
```

### Sorting with a key selector

```csharp
var people = new[] {
    new Person("Alice", 30),
    new Person("Bob", 25),
    new Person("Eve", 35)
};

people.GrailSort(p => p.Age); // Sort by Age
```

### Using LINQ-style GrailOrderBy

```csharp
var sortedNames = names
    .GrailOrderBy(name => name.Length)
    .ToList();

var sortedDesc = data
    .GrailOrderByDescending(x => x)
    .ToArray();
```

### Custom comparer and buffer strategy

```csharp
var comparer = Comparer<string>.Create((a, b) => a.CompareTo(b));
names.GrailSort(comparer, SortingBufferType.Static);
```

## License

This project is licensed under the [MIT License](LICENSE).
