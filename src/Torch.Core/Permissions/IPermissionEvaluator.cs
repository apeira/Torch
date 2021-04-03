namespace Torch.Core.Permissions
{
    /// <summary>
    /// An object capable of controlling access to a permission node.
    /// </summary>
    public interface IPermissionEvaluator
    {
        /// <summary>
        /// Evaluates the permission node against this evaluator.
        /// </summary>
        /// <param name="node">The permission node to check.</param>
        /// <returns>The effect that this evaluator has on the node.</returns>
        public PermissionModifier Evaluate(params string[] node);
    }
}