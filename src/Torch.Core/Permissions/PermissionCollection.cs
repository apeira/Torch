using System;
using System.Collections.Generic;
using System.Linq;

namespace Torch.Core.Permissions
{
    public class PermissionCollection : IPermissionCollection
    {
        private readonly List<PermissionExpression> _expressions = new();

        private readonly List<IPermissionCollection> _inherits = new();
        
        internal PermissionCollection(string section, string id)
        {
            Section = section;
            Id = id;
        }
        
        public string Section { get; }
        
        public string Id { get; }

        public IEnumerable<IPermissionCollection> Inherits => _inherits;

        public IEnumerable<PermissionExpression> Expressions => _expressions;

        public bool AddExpression(PermissionExpression expression)
        {
            if (_expressions.Any(x => x.ToString() == expression.ToString()))
                return false;

            _expressions.Add(expression);
            return true;
        }
        
        public bool RemoveExpression(PermissionExpression expression)
        {
            return _expressions.RemoveAll(x => x.ToString() == expression.ToString()) > 0;
        }
        
        public bool AddInherits(IPermissionCollection collection)
        {
            if (WouldCauseCircularInheritance(collection))
                throw new InvalidOperationException(
                    "Adding this collection would result in a circular inheritance graph.");
            
            if (_inherits.Any(x => x.Section == collection.Section && x.Id == collection.Id))
                return false;

            _inherits.Add(collection);
            return true;
        }

        public bool RemoveInherits(IPermissionCollection collection)
        {
            return _inherits.RemoveAll(x => x.Section == collection.Section && x.Id == collection.Id) > 0;
        }

        public PermissionModifier Evaluate(params string[] node)
        {
            var modifier = CheckCollection(_expressions, PermissionModifier.Default, node);
            if (modifier != PermissionModifier.Deny)
                modifier = CheckCollection(_inherits, modifier, node);

            return modifier;
        }

        public bool WouldCauseCircularInheritance(IPermissionCollection collection)
        {
            if (collection.Inherits.Any(x => Section == x.Section && Id == x.Id))
                return true;

            foreach (var inherit in _inherits)
            {
                if (inherit.WouldCauseCircularInheritance(collection))
                    return true;
            }
            
            return false;
        }
        
        private static PermissionModifier CheckCollection(IEnumerable<IPermissionEvaluator> evaluators, PermissionModifier current, params string[] node)
        {
            var modifier = current;
            foreach (var evaluator in evaluators)
            {
                switch (evaluator.Evaluate(node))
                {
                    case PermissionModifier.Default:
                        break;
                    case PermissionModifier.Allow:
                        modifier = PermissionModifier.Allow;
                        break;
                    case PermissionModifier.Deny:
                        return PermissionModifier.Deny;
                    default:
                        throw new InvalidOperationException("Unknown permission modifier value.");
                }
            }

            return modifier;
        }
    }
}