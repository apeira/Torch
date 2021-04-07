using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Decider.Csp.BaseTypes;
using Decider.Csp.Integer;
using SemVer;

namespace TorchSetup.Plugins
{
    public class PluginManager
    {
        private readonly Dictionary<string, List<PluginSpecification>> _plugins = new ();
        private readonly ConditionalWeakTable<PluginSpecification, VariableInteger> _variables = new ();
        private readonly List<IConstraint> _constraints = new ();
        private VariableInteger? _minimize;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginManager"/> class.
        /// </summary>
        /// <param name="pluginIndexDir">A directory to search for plugin specification files.</param>
        public PluginManager(string? pluginIndexDir = null)
        {
            if (pluginIndexDir is null || !Directory.Exists(pluginIndexDir))
                return;

            foreach (var yamlFile in Directory.EnumerateFiles(pluginIndexDir, "*.yaml", SearchOption.AllDirectories))
            {
                var spec = PluginSpecification.Load(yamlFile);
                VersionList(spec.Id).Add(spec);
            }
        }

        /// <summary>
        /// Gets a set of plugins that the user wants to install.
        /// </summary>
        public Dictionary<string, Range> ExplicitlyInstalled { get; } = new ();

        /// <summary>
        /// Adds a new specification to the manager.
        /// </summary>
        public void AddSpec(PluginSpecification spec)
        {
            VersionList(spec.Id).Add(spec);
        }

        /// <summary>
        /// Finds a minimum set of plugins that must be installed to satisfy the dependency graph.
        /// </summary>
        public void SolveDependencies()
        {
            SetupConstraints();
            var variables = _plugins.SelectMany(x => x.Value).Select(x => (VariableInteger)VariableFor(x)).Append(_minimize);
            IState<int> state = new StateInteger(variables, _constraints);
            state.StartSearch(out var searchResult, _minimize, out IDictionary<string, IVariable<int>> solution);

            Console.WriteLine(searchResult.ToString("G"));
            foreach (var variable in solution)
            {
                Console.WriteLine($"{variable.Key}: {variable.Value}");
            }
        }

        /// <summary>
        /// Gets the set of versions for a plugin.
        /// </summary>
        private List<PluginSpecification> VersionList(string pluginId)
        {
            if (!_plugins.TryGetValue(pluginId, out var list))
                list = _plugins[pluginId] = new List<PluginSpecification>();

            return list;
        }

        /// <summary>
        /// Gets a VariableInteger representing the installation state of the given plugin version
        /// where 1 means installed and 0 means not installed.
        /// </summary>
        private ExpressionInteger VariableFor(PluginSpecification spec)
        {
            return _variables.GetValue(
                spec,
                key => new VariableInteger($"{key.Id} {key.Version}", 0, 1));
        }

        /// <summary>
        /// Creates constraints that model the current system of available plugins.
        /// </summary>
        private void SetupConstraints()
        {
            _constraints.Clear();

            // Only one version of a plugin may be installed.
            foreach (var plugin in _plugins)
            {
                var numInstalled = plugin.Value.Select(VariableFor).Aggregate((x, y) => x + y);
                _constraints.Add(new ConstraintInteger(numInstalled <= 1));
            }

            // If a plugin is installed, a satisfying version of each of its dependencies must be installed.
            foreach (var pluginVer in _plugins.SelectMany(x => x.Value))
            {
                if (pluginVer.Requires is not null)
                {
                    foreach (var req in pluginVer.Requires)
                    {
                        var satisfyingVersions = _plugins[req.Id].Where(x => req.Range.IsSatisfied(x.Version));
                        var numSatisfyingInstalled = satisfyingVersions.Select(VariableFor).Aggregate((x, y) => x + y);
                        _constraints.Add(new ConstraintInteger(numSatisfyingInstalled >= VariableFor(pluginVer)));
                    }
                }
            }

            // A plugin cannot be installed alongside a conflicting plugin.
            foreach (var pluginVer in _plugins.SelectMany(x => x.Value))
            {
                if (pluginVer.Conflicts is not null)
                {
                    foreach (var conflict in pluginVer.Conflicts)
                    {
                        var conflictingVersions = _plugins[conflict.Id].Where(x => conflict.Range.IsSatisfied(x.Version));
                        var numConflictingInstalled = conflictingVersions.Select(VariableFor).Aggregate((x, y) => x + y);
                        _constraints.Add(new ConstraintInteger(VariableFor(pluginVer) * numConflictingInstalled == 0));
                    }
                }
            }

            // A satisfying version of explicitly installed plugins must be installed.
            foreach (var req in ExplicitlyInstalled)
            {
                var satisfyingVersions = _plugins[req.Key].Where(x => req.Value.IsSatisfied(x.Version));
                var numSatisfyingInstalled = satisfyingVersions.Select(VariableFor).Aggregate((x, y) => x + y);
                _constraints.Add(new ConstraintInteger(numSatisfyingInstalled >= 1));
            }

            // Install the minimum number of plugins possible.
            var allPlugins = _plugins.SelectMany(x => x.Value).Select(VariableFor);
            var allInstalled = allPlugins.Aggregate((x, y) => x + y);
            _minimize = new VariableInteger("numInstalled", -_plugins.Keys.Count, 0);
            _constraints.Add(new ConstraintInteger(_minimize * -1 == allInstalled));

            // TODO Install the most recent versions of plugins possible.
        }
    }
}
