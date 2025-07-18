# GrailSortCSharp

[![NuGet](https://img.shields.io/nuget/v/GrailSortCSharp.svg)](https://www.nuget.org/packages/GrailSortCSharp)

![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/hoshizorastardiver/GrailSortCSharp/build-test-mudblazor.yml?branch=dev&logo=github&style=flat-square)
[![Codecov]( https://img.shields.io/codecov/c/github/hoshizorastardiver/GrailSortCSharp )](https://app.codecov.io/github/hoshizorastardiver/GrailSortCSharp )
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=hoshizorastardiver_GrailSortCSharp&metric=alert_status)]( https://sonarcloud.io/summary/overall?id=hoshizorastardiver_GrailSortCSharp)
[![GitHub]( https://img.shields.io/github/license/hoshizorastardiver/GrailSortCSharp?color=594ae2&logo=github&style=flat-square)]( https://github.com/hoshizorastardiver/GrailSortCSharp/blob/master/LICENSE )
[![GitHub Repo stars](https://img.shields.io/github/stars/hoshizorastardiver/GrailSortCSharp?color=594ae2&style=flat-square&logo=github)]( https://github.com/hoshizorastardiver/GrailSortCSharp/stargazers )
[![GitHub last commit](https://img.shields.io/github/last-commit/hoshizorastardiver/GrailSortCSharp?color=594ae2&style=flat-square&logo=github)]( https://github.com/hoshizorastardiver/GrailSortCSharp )
[![Contributors](https://img.shields.io/github/contributors/hoshizorastardiver/GrailSortCSharp?color=594ae2&style=flat-square&logo=github)]( https://github.com/hoshizorastardiver/GrailSortCSharp/graphs/contributors )
[![Discussions](https://img.shields.io/github/discussions/hoshizorastardiver/GrailSortCSharp?color=594ae2&logo=github&style=flat-square)]( https://github.com/hoshizorastardiver/GrailSortCSharp/discussions )
[![Discord](https://img.shields.io/discord/786656789310865418?color=%237289da&label=Discord&logo=discord&logoColor=%237289da&style=flat-square)]( https://discord.gg/GrailSortCSharp )
[![Twitter](https://img.shields.io/twitter/follow/hoshizorastardiver?color=1DA1F2&label=Twitter&logo=Twitter&style=flat-square)]( https://twitter.com/hoshizorastardiver )
[![NuGet version](https://img.shields.io/nuget/v/GrailSortCSharp?color=ff4081&label=nuget%20version&logo=nuget&style=flat-square)]( https://www.nuget.org/packages/GrailSortCSharp/ )
[![NuGet downloads](https://img.shields.io/nuget/dt/GrailSortCSharp?color=ff4081&label=nuget%20downloads&logo=nuget&style=flat-square)]( https://www.nuget.org/packages/GrailSortCSharp/ )

A set of extension methods that integrate the GrailSort algorithm into .NET collections and LINQ, providing high-performance, in-place sorting with flexible buffer strategies. Based on [Summer Dragonfly et al.'s Rewritten Grailsort for Java](https://github.com/HolyGrailSortProject/Rewritten-Grailsort). It has been retested and further optimized specifically for C#.

## Table of Contents

* [Installation](#installation)
* [Overview](#overview)
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

## Buffer Types

```csharp
public enum SortingBufferType
{
    InPlace,
    Static,
    Dynamic
}
```

* **Dynamic**: Best for general use; allocates O(n) extra space.
* **Static**: Uses a fixed-size buffer; good for repeated sorts on similar input sizes.
* **InPlace**: Minimizes memory usage; slightly slower due to in-place constraint.

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

## Array Extensions

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

## List Extensions

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
