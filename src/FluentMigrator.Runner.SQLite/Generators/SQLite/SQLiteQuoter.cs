using System;

using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.SQLite
{
    /// <summary>
    /// The SQLite SQL quoter for FluentMigrator.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class SQLiteQuoter : GenericQuoter
    {
        private readonly bool _binaryGuid;

        /// <inheritdoc />
        public SQLiteQuoter(bool binaryGuid = false)
        {
            _binaryGuid = binaryGuid;
        }

        /// <inheritdoc />
        public override string FormatSystemMethods(SystemMethods value)
        {
            switch (value)
            {
                case SystemMethods.CurrentUTCDateTime:
                    return "CURRENT_TIMESTAMP";
                case SystemMethods.CurrentDateTime:
                    return "(datetime('now','localtime'))";
            }

            return base.FormatSystemMethods(value);
        }

        /// <inheritdoc />
        public override string FormatGuid(Guid value)
        {
            if (_binaryGuid)
            {
                var byteArray = value.ToByteArray();
                var hexadecimalStringRepresentation = BitConverter.ToString(byteArray).Replace("-", string.Empty);
                return "x'" + hexadecimalStringRepresentation + "'";
            }
            return base.FormatGuid(value);
        }

        /// <inheritdoc />
        public override string QuoteSchemaName(string schemaName)
        {
            // SQLite doesn't support Schemas in the same sense as other SQL databases (e.g. SQL Server) does,
            // instead it allows multiple databases to be attached to the current DB connection
            // and those attached DB's have an `alias` that can be used as a prefix similar to schemas.
            // As this is nearest paradigm in FM, we'll allow schemas to be defined and generate the
            // relevant SQL, however we don't currently have an approach for attaching databases
            // so we'll have to currently assume that the implementor does this.
            // See: https://www.sqlite.org/lang_attach.html  and https://www.sqlite.org/lang_naming.html
            return base.QuoteSchemaName(schemaName);
        }

        /// <inheritdoc />
        public override string QuoteIndexName(string indexName, string schemaName)
        {
            return CreateSchemaPrefixedQuotedIdentifier(
                QuoteSchemaName(schemaName),
                IsQuoted(indexName) ? indexName : Quote(indexName));
        }

        /// <inheritdoc />
        protected override string FormatByteArray(byte[] value)
        {
            var hex = new System.Text.StringBuilder((value.Length * 2) + 3);
            hex.Append("X'");
            foreach (var b in value)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            hex.Append("'");

            return hex.ToString();
        }
    }
}
