using System.Collections.Generic;

namespace Torch.Core.Permissions
{
    /// <summary>
    /// A collection of <see cref="PermissionExpression"/>s and nested
    /// <see cref="IPermissionCollection"/>s that can be evaluated as a single unit.
    /// </summary>
    public interface IPermissionCollection : IPermissionEvaluator
    {
        /// <summary>
        /// Gets the <see cref="IPermissionCollection"/>s that this <see cref="IPermissionCollection"/> inherits.
        /// </summary>
        IEnumerable<IPermissionCollection> Inherits { get; }
        
        /// <summary>
        /// Gets the <see cref="PermissionExpression"/>s contained directly in this <see cref="IPermissionCollection"/>.
        /// </summary>
        IEnumerable<PermissionExpression> Expressions { get; }
        
        /// <summary>
        /// Gets the section that this <see cref="IPermissionCollection"/> is classified under.
        /// </summary>
        public string Section { get; }
        
        /// <summary>
        /// Gets the ID of this <see cref="IPermissionCollection"/>.
        /// </summary>
        public string Id { get; }

        bool AddExpression(PermissionExpression expression);
        
        bool RemoveExpression(PermissionExpression expression);

        bool AddInherits(IPermissionCollection collection);

        bool RemoveInherits(IPermissionCollection collection);
    }
}