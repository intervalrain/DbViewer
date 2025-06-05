using System.Reflection;
using Common.Modularity.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Modularity;

public static class ModuleRegister
{
    public static ServiceConfigurationContext ConfigureModules(this IServiceCollection services, Type type)
    {
        var context = new ServiceConfigurationContext(services);

        if (type.IsAssignableFrom(typeof(IModule)))
            throw new ArgumentException($"{type.Name} must implment IModule.");

        var graph = BuildGraph<Type>(type);
        var sorted = TopologicalSort(type, graph);
        CheckAllModulesReachable(sorted);

        foreach (var t in sorted)
        {
            var module = (IModule)Activator.CreateInstance(t)!;
            if (module != null)
            {
                module.ConfigureServices(context);
                context.AddConfigureAction(module);
            }
        }

        return context;
    }

    private static void CheckAllModulesReachable(IEnumerable<Type> sorted)
    {
        var allModules = DiscoverAllModules();
        var rechable = new HashSet<Type>(sorted);
        var uncovered = allModules.Where(m => !rechable.Contains(m));
        if (uncovered.Any())
        {
            var names = string.Join(", ", uncovered.Select(t => t.Name));
            var root = sorted.Last();
            throw new InvalidOperationException($"The following modules are not reachable from root module '{root.Name}': {names}");
        }
    }


    private static HashSet<Type> DiscoverAllModules()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a =>
            {
                try
                {
                    return a.GetTypes();
                }
                catch
                {
                    return [];
                }
            })
            .Where(Module.IsModule)
            .ToHashSet();
    }


    private static IEnumerable<Type> TopologicalSort(Type curr, Dictionary<Type, List<Type>> graph)
    {
        List<Type> result = [];
        HashSet<Type> visited = [];
        HashSet<Type> visiting = [];

        void Dfs(Type curr)
        {
            if (visiting.Contains(curr))
                throw new InvalidOperationException($"Circular dependency detected at {curr.Name}");

            if (visited.Contains(curr)) return;
            visiting.Add(curr);

            foreach (var next in graph[curr])
            {
                Dfs(next);
            }

            visiting.Remove(curr);
            visited.Add(curr);
            result.Add(curr);
        }

        Dfs(curr);

        return result;
    }

    private static Dictionary<Type, List<Type>> BuildGraph<T>(Type root)
    {
        var graph = new Dictionary<Type, List<Type>>();
        var visited = new HashSet<Type>();
        var stack = new Stack<Type>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var curr = stack.Pop();
            if (visited.Contains(curr)) continue;
            visited.Add(curr);

            var dependencies = GetDependencies(curr).ToList();
            graph[curr] = dependencies;

            foreach (var next in dependencies)
            {
                stack.Push(next);
            }
        }

        return graph;
    }

    private static IEnumerable<Type> GetDependencies(Type type)
    {
        return type.GetCustomAttributes<DependsOnAttribute>()
            .SelectMany(attr => attr.Dependencies);

    }
}