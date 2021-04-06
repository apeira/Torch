using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace TorchSetup.Plugins
{
    /// <summary>
    /// Finds the smallest number of plugins with the highest versions that satisfy the installation constraints.
    /// </summary>
    public class DependencySolver
    {
        private Dictionary<string, int> _literals = new ();
        private int _literalCount = 0;
        private List<Expression> _clauses = new();
        private Func<bool[], bool> _test;
        private List<bool[]> _solutions = new();
        private ParameterExpression _arrayParam = Expression.Parameter(typeof(bool[]), "Array");

        public List<bool[]> Solutions => _solutions;

        public void AddSingleConstraint(PluginSpecification spec)
        {
            _clauses.Add(Literal(spec));
        }

        /// <summary>
        /// Only allow one in the set of given specs to be selected.
        /// </summary>
        public void AddExclusiveConstraint(params PluginSpecification[] pickOne)
        {
            Expression finalConstraint = null;
            foreach (var include in pickOne)
            {
                var specLiteral = Literal(include);

                Expression subConstraint = null;
                foreach (var exclude in pickOne)
                {
                    if (exclude == include)
                        continue;

                    if (subConstraint == null)
                        subConstraint = Literal(exclude);
                    else
                        subConstraint = Expression.Or(subConstraint, Literal(exclude));
                }

                subConstraint = Expression.And(specLiteral, Expression.Not(subConstraint));
                if (finalConstraint == null)
                    finalConstraint = subConstraint;
                else
                    finalConstraint = Expression.Or(finalConstraint, subConstraint);
            }

            _clauses.Add(finalConstraint);
        }

        /// <summary>
        /// Spec needs exactly one from the collection.
        /// </summary>
        public void AddRequirementConstraint(PluginSpecification spec, params PluginSpecification[] pickOne)
        {
            Expression combined = Expression.Not(Literal(spec));

            foreach (var pick in pickOne)
            {
                var exp = Expression.And(Literal(spec), Literal(pick));
                combined = Expression.Or(combined, exp);
            }

            _clauses.Add(combined);
        }

        /// <summary>
        /// Gets the index of a literal representing this plugin.
        /// </summary>
        public int LiteralIndex(PluginSpecification spec)
        {
            var specName = $"{spec.Id} {spec.Version}";

            if (!_literals.TryGetValue(specName, out var literal))
                literal = _literals[specName] = _literalCount++;

            return literal;
        }

        private void CompileClauses()
        {
            Expression expression = null;

            foreach (var clause in _clauses)
            {
                if (expression == null)
                    expression = clause;
                else
                    expression = Expression.And(expression, clause);
            }

            Console.WriteLine(expression);
            _test = Expression.Lambda<Func<bool[], bool>>(expression, _arrayParam).Compile();
        }

        public void FindSolutions()
        {
            _solutions.Clear();
            CompileClauses();
            var array = new bool[_literalCount];

            SolveInternal(array, 0, true);
            SolveInternal(array, 0, false);

            void SolveInternal(bool[] literals, int index, bool value)
            {
                if (index == _literals.Count)
                    return;

                literals[index] = value;

                if (_test(literals))
                {
                    _solutions.Add(literals.ToArray());
                    return;
                }

                SolveInternal(literals, index + 1, true);
                SolveInternal(literals, index + 1, false);
            }
        }

        private IndexExpression Literal(PluginSpecification spec)
        {
            return Expression.ArrayAccess(_arrayParam, Expression.Constant(LiteralIndex(spec)));
        }
    }
}