using System.Collections.Generic;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Update
{
    /// <summary>
    /// Interface the specify the update condition
    /// </summary>
    public interface IUpdateWhereSyntax : IFluentSyntax
    {
        /// <summary>
        /// Specify the condition of the rows to update
        /// </summary>
        /// <param name="dataAsAnonymousType">The columns and values to be used as condition</param>
#if NET
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("The properties of the anonymous type cannot be statically analyzed.")]
#endif
        void Where(object dataAsAnonymousType);

        /// <summary>
        /// Specify the condition of the rows to update
        /// </summary>
        /// <param name="data">The columns and values to be used as condition</param>
        void Where(IDictionary<string, object> data);

        /// <summary>
        /// Specify that all rows should be updated
        /// </summary>
        void AllRows();
    }
}
