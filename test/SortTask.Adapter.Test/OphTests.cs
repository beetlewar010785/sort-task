using System.Collections;
using System.Text;
using Bogus;

namespace SortTask.Adapter.Test;

public class UlongOphTests
{
    [TestCaseSource(nameof(TestCases))]
    public void Should_Preserve_Order(Encoding encoding, int numStrings)
    {
        var sut = new UlongOph();

        Faker faker = new();
        var strings = Enumerable.Range(0, numStrings)
            .Select(_ => faker.Random.Words(1))
            .ToList();
        strings.Sort(string.CompareOrdinal);

        var hashes = strings.Select(s => sut.Hash(encoding.GetBytes(s)));

        Assert.That(hashes, Is.Ordered.Using(Comparer.Default), string.Join(";", strings));
    }

    private static IEnumerable<TestCaseData> TestCases() =>
    [
        new(Encoding.ASCII, 1000),
        new(Encoding.UTF8, 1000),
        new(Encoding.Unicode, 1000),
        new(Encoding.UTF8, 1000),
    ];
}