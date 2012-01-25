using System;

namespace FluentMigrator
{
    /// <summary>Represents how to handle SQL commands that are not supported by the underlying database.</summary>
    [Flags]
    public enum CompatibilityMode
    {
        /// <summary>Throw an exception.</summary>
        Strict = 1,

        /// <summary>Ignore the command.</summary>
        Loose = 2,

        /// <summary>Emulate the SQL command using supported functionality.</summary>
        /// <remarks>
        ///   <para>For example, schema support can be emulated by prefixing the schema name to the table name (<c>`schema`.`table`</c> => <c>`schema_table`</c>).</para>
        ///   <para>This can be combined with the <see cref="Strict"/> flag to throw an exception if the command cannot be emulated (the default behaviour is <see cref="Loose"/>).</para>
        /// </remarks>
        Emulate = 4
    };
}