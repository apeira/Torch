namespace Torch.Core.Permissions
{
    public class PermissionExpression : IPermissionEvaluator
    {
        private PermissionModifier _modifier;
        private string[] _node;

        public PermissionModifier Modifier => _modifier;

        public PermissionExpression(string node)
        {
            if (node[0] == '-')
            {
                _modifier = PermissionModifier.Deny;
                node = node.Substring(1);
            }
            else
            {
                _modifier = PermissionModifier.Allow;
            }
            
            // TODO throw exception if invalid * or - characters are in the node
            _node = node.Split('.');
        }

        public PermissionModifier Evaluate(params string[] node)
        {
            for (var i = 0; i < _node.Length && i < node.Length; i++)
            {
                // Wildcards match the contents of any node segment.
                if (_node[i] == "*")
                    continue;

                // Not a match, doesn't apply to the node being checked.
                if (_node[i] != node[i])
                    return PermissionModifier.Default;
            }

            if (_node.Length == node.Length)
                return _modifier;

            var endWildcard = _node[_node.Length - 1] == "*";
            
            // Wildcard at end affects all subsequent nodes.
            if (_node.Length < node.Length && endWildcard)
                return _modifier;

            // Wildcard at end affects immediate parent node.
            if (_node.Length == node.Length + 1 && endWildcard)
                return _modifier;

            return PermissionModifier.Default;
        }

        public override string ToString()
        {
            var str = new string('-', _modifier == PermissionModifier.Deny ? 1 : 0);
            str += string.Join(".", _node);
            return str;
        }
    }
}