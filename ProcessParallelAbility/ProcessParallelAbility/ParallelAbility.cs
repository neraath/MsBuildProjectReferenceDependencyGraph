// -----------------------------------------------------------------------
// <copyright file="ParallelAbility.cs" company="Ace Olszowka">
// Copyright (c) 2020 Ace Olszowka.
// </copyright>
// -----------------------------------------------------------------------

namespace ProcessParallelAbility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class ParallelAbility
    {
        /// <summary>
        /// Given the result of <see cref="ResolveParallelTree(IDictionary{string, SortedSet{string}})"/> convert it to a structure for output.
        /// </summary>
        /// <param name="parallelTree">The structure returned by <see cref="ResolveParallelTree(IDictionary{string, SortedSet{string}})"/>.</param>
        /// <returns>A structure in which the Key is the "Level" or depth of the project and the Value is the list of projects at that level.</returns>
        internal static IDictionary<UInt64, List<string>> ConvertParallelTreeToLevels(IDictionary<string, UInt64> parallelTree)
        {
            IDictionary<UInt64, List<string>> levels = new Dictionary<UInt64, List<string>>();

            foreach (KeyValuePair<string, UInt64> entry in parallelTree)
            {
                if (!levels.ContainsKey(entry.Value))
                {
                    levels.Add(entry.Value, new List<string>());
                }

                levels[entry.Value].Add(entry.Key);
            }

            return levels;
        }

        /// <summary>
        /// Given a Dependency Tree, return a structure that represents the depth within the dependency tree.
        /// </summary>
        /// <param name="dependencyTree">The dependency tree to evaluate.</param>
        /// <returns>A structure in which the Key is the project and the value is the depth within the given dependency tree.</returns>
        internal static IDictionary<string, UInt64> ResolveParallelTree(IDictionary<string, SortedSet<string>> dependencyTree)
        {
            IDictionary<string, UInt64> parallelBuildTree = new Dictionary<string, UInt64>(StringComparer.InvariantCultureIgnoreCase);

            foreach (KeyValuePair<string, SortedSet<string>> dotGraphEntry in dependencyTree)
            { 
                if (parallelBuildTree.ContainsKey(dotGraphEntry.Key))
                {
                    // We can Skip this because it was resolved
                }
                else
                {
                    // Push only ourself, since we need to walk the tree and detect circular dependencies in realtime.
                    Stack<string> projectsToResolve = new Stack<string>();
                    projectsToResolve.Push(dotGraphEntry.Key);

                    while (projectsToResolve.Count != 0)
                    {
                        string currentProjectName = projectsToResolve.Pop();
                        if (parallelBuildTree.ContainsKey(currentProjectName)) continue;
                        try
                        {
                            string[] unresolvedDependencies = dependencyTree[currentProjectName]
                                .Where(dependency => !parallelBuildTree.ContainsKey(dependency)).ToArray();

                            // Detect no dependencies or full cyclic dependency
                            if (unresolvedDependencies.Length > 0 && unresolvedDependencies.All(dependency => projectsToResolve.Contains(dependency, StringComparer.InvariantCultureIgnoreCase)))
                            {
                                // If we do not have any dependencies or everything is cyclic, we're the deepest project
                                parallelBuildTree.Add(currentProjectName, 0);
                            }
                            else if (unresolvedDependencies.Any())
                            {
                                // We still need to resolve more N-Order Dependencies
                                projectsToResolve.Push(currentProjectName);
                                foreach (string dependency in unresolvedDependencies)
                                {
                                    // Skip it if it's already queued for resolution. 
                                    if (projectsToResolve.Contains(dependency,
                                        StringComparer.InvariantCultureIgnoreCase)) continue;
                                    projectsToResolve.Push(dependency);
                                }
                            }
                            else
                            {
                                // Everything was resolved now we need to figure out at what level we should insert
                                if (dependencyTree[currentProjectName].Count == 0)
                                {
                                    // If we do not have any dependencies we're the deepest project
                                    parallelBuildTree.Add(currentProjectName, 0);
                                }
                                else
                                {
                                    // Otherwise we need to find out what the deepest dependency is, and go one level deeper
                                    UInt64 deepestDependency = dependencyTree[currentProjectName]
                                        .Select(dependency => parallelBuildTree[dependency]).Max();
                                    parallelBuildTree.Add(currentProjectName, deepestDependency + 1);
                                }
                            }
                        }
                        catch
                        {
                            throw;
                        }
                    }
                }
            }

            return parallelBuildTree;
        }
    }
}
