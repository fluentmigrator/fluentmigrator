using System.Collections.Generic;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Update
{
    /// <summary>
    /// Specify the data to update
    /// </summary>
    public interface IUpdateSetSyntax : IFluentSyntax
    {
        /// <summary>
        /// Specify the values to be set
        /// </summary>
        /// <param name="dataAsAnonymousType">The columns and values to be used set</param>
        /// <returns>The next step</returns>
#if NET
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("The properties of the anonymous type cannot be statically analyzed.")]
#endif
        IUpdateWhereSyntax Set(object dataAsAnonymousType);

        /// <summary>
        /// Specify the values to be set
        /// </summary>
        /// <param name="data">The columns and values to be used set</param>
        /// <returns>The next step</returns>
        IUpdateWhereSyntax Set(IDictionary<string, object> data);
    }
}
