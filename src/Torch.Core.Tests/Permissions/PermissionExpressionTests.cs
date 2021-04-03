using Torch.Core.Permissions;
using Xunit;

namespace Torch.Core.Tests.Permissions
{
    /// <summary>
    /// Tests the evaluation of permission nodes using <see cref="PermissionExpression"/>.
    /// </summary>
    public class PermissionExpressionTests
    {
        /// <summary>
        /// Tests expressions without wildcards.
        /// </summary>
        /// <param name="expression">The permission expression.</param>
        /// <param name="node">The permission node to compare.</param>
        /// <param name="expected">The expected result of the evaluation.</param>
        [Theory]
        [InlineData("test",         "test",            PermissionModifier.Allow)]
        [InlineData("test",         "nottest",         PermissionModifier.Default)]
        [InlineData("test",         "test.perm",       PermissionModifier.Default)]
        [InlineData("-test",        "test",            PermissionModifier.Deny)]
        [InlineData("-test",        "nottest",         PermissionModifier.Default)]
        [InlineData("-test",        "test.perm",       PermissionModifier.Default)]
        [InlineData("test.perm",    "test.perm",       PermissionModifier.Allow)]
        [InlineData("test.perm",    "test.notperm",    PermissionModifier.Default)]
        [InlineData("test.perm",    "test",            PermissionModifier.Default)]
        [InlineData("-test.perm",   "test",            PermissionModifier.Default)]
        [InlineData("-test.perm",   "test.perm",       PermissionModifier.Deny)]
        [InlineData("-test.perm",   "test.notperm",    PermissionModifier.Default)]
        public void ExactMatch(string expression, string node, PermissionModifier expected)
        {
            Test(expression, node, expected);
        }

        /// <summary>
        /// Tests expressions with trailing wildcards.
        /// </summary>
        /// <param name="expression">The permission expression.</param>
        /// <param name="node">The permission node to compare.</param>
        /// <param name="expected">The expected result of the evaluation.</param>
        [Theory]
        [InlineData("test.*",       "test",            PermissionModifier.Allow)]
        [InlineData("test.*",       "test.perm",       PermissionModifier.Allow)]
        [InlineData("test.*",       "test.blah.perm",  PermissionModifier.Allow)]
        [InlineData("-test.*",      "test",            PermissionModifier.Deny)]
        [InlineData("-test.*",      "test.perm",       PermissionModifier.Deny)]
        [InlineData("-test.*",      "test.blah.perm",  PermissionModifier.Deny)]
        [InlineData("*",            "test",            PermissionModifier.Allow)]
        [InlineData("*",            "test.perm",       PermissionModifier.Allow)]
        [InlineData("-*",           "test",            PermissionModifier.Deny)]
        [InlineData("-*",           "test.perm",       PermissionModifier.Deny)]
        public void EndWildcard(string expression, string node, PermissionModifier expected)
        {
            Test(expression, node, expected);
        }

        /// <summary>
        /// Tests expressions with wildcards in the middle.
        /// </summary>
        /// <param name="expression">The permission expression.</param>
        /// <param name="node">The permission node to compare.</param>
        /// <param name="expected">The expected result of the evaluation.</param>
        [Theory]
        [InlineData("test.*.perm",  "test",            PermissionModifier.Default)]
        [InlineData("test.*.perm",  "test.blah.perm",  PermissionModifier.Allow)]
        [InlineData("-test.*.perm", "test",            PermissionModifier.Default)]
        [InlineData("-test.*.perm", "test.blah.perm",  PermissionModifier.Deny)]
        [InlineData("*.perm",       "test",            PermissionModifier.Default)]
        [InlineData("*.perm",       "test.perm",       PermissionModifier.Allow)]
        [InlineData("-*.perm",      "test",            PermissionModifier.Default)]
        [InlineData("-*.perm",      "test.perm",       PermissionModifier.Deny)]
        public void MiddleWildcard(string expression, string node, PermissionModifier expected)
        {
            Test(expression, node, expected);
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private void Test(string expression, string node, PermissionModifier expected)
        {
            Assert.Equal(expected, new PermissionExpression(expression).Evaluate(node.Split('.')));
        }
    }
}
