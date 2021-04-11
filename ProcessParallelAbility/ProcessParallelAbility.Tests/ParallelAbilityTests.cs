// -----------------------------------------------------------------------
// <copyright file="ParallelAbilityTests.cs" company="Ace Olszowka">
// Copyright (c) 2020 Ace Olszowka.
// </copyright>
// -----------------------------------------------------------------------

namespace ProcessParallelAbility.Tests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using NUnit.Framework;

    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class ParallelAbilityTests
    {
        [TestCaseSource(typeof(ResolveParallelTreeTests))]
        public void ResolveParallelTree(IDictionary<string, SortedSet<string>> dependencyTree, IDictionary<string, int> expected)
        {
            IDictionary<string, UInt64> actual = ParallelAbility.ResolveParallelTree(dependencyTree);

            Assert.That(actual, Is.EquivalentTo(expected));
        }
    }

    internal class ResolveParallelTreeTests : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            yield return new TestCaseData(
                new Dictionary<string, SortedSet<string>>(StringComparer.InvariantCultureIgnoreCase)
                {
                    // This is the tree
                    // digraph {
                    // A -> B
                    // A -> C
                    // A -> K
                    // B -> D
                    // B -> E
                    // D -> E
                    // D -> H
                    // E -> H
                    // C -> E
                    // C -> F
                    // C -> G
                    // G -> E
                    // G -> J
                    // G -> K
                    // H -> I
                    // I -> G
                    // }
                    { "A", new SortedSet<string>() {"B", "C", "K"} },
                    { "B", new SortedSet<string>() {"D", "E"} },
                    { "C", new SortedSet<string>() {"E", "F", "G"} },
                    { "D", new SortedSet<string>() {"E", "H"} },
                    { "E", new SortedSet<string>() {"H"} },
                    { "F", new SortedSet<string>() {} },
                    { "G", new SortedSet<string>() {"E", "J", "K"} },
                    { "H", new SortedSet<string>() {"I"} },
                    { "I", new SortedSet<string>() {"G"} },
                    { "J", new SortedSet<string>() },
                    { "K", new SortedSet<string>() }
                },
                new Dictionary<string, int>
                {
                    { "A", 6 },
                    { "B", 5 },
                    { "C", 4 },
                    { "D", 4 },
                    { "E", 3 },
                    { "F", 0 },
                    { "G", 0 },
                    { "H", 2 },
                    { "I", 1 },
                    { "J", 0 },
                    { "K", 0 }
                }
            ).SetName("{m}ComplexMultiCircularDependency");
            yield return new TestCaseData(
                new Dictionary<string, SortedSet<string>>(StringComparer.InvariantCultureIgnoreCase)
                {
                    // This is the tree
                    // digraph {
                    // A -> B
                    // B -> C
                    // B -> D
                    // D -> E
                    // D -> G
                    // C -> F
                    // E -> F
                    // F -> A
                    // F -> H
                    // H -> I
                    // I -> G
                    // }
                    { "A", new SortedSet<string>() {"B"} },
                    { "B", new SortedSet<string>() {"C", "D"} },
                    { "C", new SortedSet<string>() {"F"} },
                    { "D", new SortedSet<string>() {"E", "G"} },
                    { "E", new SortedSet<string>() {"F", "G"} },
                    { "F", new SortedSet<string>() {"H"} },
                    { "G", new SortedSet<string>() {} },
                    { "H", new SortedSet<string>() {"I"} },
                    { "I", new SortedSet<string>() {"G"} }
                },
                new Dictionary<string, int>
                {
                    { "A", 7 },
                    { "B", 6 },
                    { "C", 4 },
                    { "D", 5 },
                    { "E", 4 },
                    { "F", 3 },
                    { "G", 0 },
                    { "H", 2 },
                    { "I", 1 }
                }
            ).SetName("{m}ComplexCircularDependency");
            yield return new TestCaseData(
                new Dictionary<string, SortedSet<string>>(StringComparer.InvariantCultureIgnoreCase)
                {
                    // This is the tree
                    // digraph {
                    // A -> B
                    // b -> c
                    // b -> d
                    // c -> d
                    // D -> e
                    // e -> a
                    // }
                    { "A", new SortedSet<string>() {"B"} },
                    { "B", new SortedSet<string>() {"c", "d"} },
                    { "C", new SortedSet<string>() {"d"} },
                    { "D", new SortedSet<string>() {"e"} },
                    { "e", new SortedSet<string>() {"a"} }
                },
                new Dictionary<string, int>
                {
                    { "A", 4 },
                    { "B", 3 },
                    { "c", 2 },
                    { "d", 1 },
                    { "e", 0 }
                }
                ).SetName("{m}SimpleCircularDependency");
            yield return new TestCaseData
                (
                new Dictionary<string, SortedSet<string>>()
                {
                    // This is this tree
                    // digraph {
                    // A -> B
                    // B -> C
                    // D -> C
                    // E
                    // F -> B
                    // }
                    { "A", new SortedSet<string>() {"B"} },
                    { "B", new SortedSet<string>() {"C"} },
                    { "C", new SortedSet<string>() },
                    { "D", new SortedSet<string>() {"C"} },
                    { "E", new SortedSet<string>() },
                    { "F", new SortedSet<string>() {"B"} },
                },
                new Dictionary<string, int>
                {
                    { "A", 2 },
                    { "B", 1 },
                    { "C", 0 },
                    { "D", 1 },
                    { "E", 0 },
                    { "F", 2 },
                }
                ).SetName("{m}(ComplexDependencyTest)");
            yield return new TestCaseData
                (
                new Dictionary<string, SortedSet<string>>()
                {
                                // This is this tree
                                // digraph {
                                // A -> B
                                // }
                                { "A", new SortedSet<string>() {"B"} },
                                { "B", new SortedSet<string>() },
                },
                new Dictionary<string, int>
                {
                                { "A", 1 },
                                { "B", 0 },
                }
                ).SetName("{m}(SimpleDependencyTest)");
           yield return new TestCaseData
                (
                new Dictionary<string, SortedSet<string>>()
                {
                                // This is this tree
                                // digraph {
                                // A
                                // B
                                // }
                                { "A", new SortedSet<string>() },
                                { "B", new SortedSet<string>() },
                },
                new Dictionary<string, int>
                {
                                { "A", 0 },
                                { "B", 0 },
                }
                ).SetName("{m}(SimpleTest)");
        }
    }
}
