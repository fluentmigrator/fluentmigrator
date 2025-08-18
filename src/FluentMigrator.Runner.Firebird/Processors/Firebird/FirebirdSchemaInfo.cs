using System;
using System.Collections.Generic;
using System.Data;

using FluentMigrator.Runner.Generators.Firebird;

namespace FluentMigrator.Runner.Processors.Firebird
{
    public static class AdoHelper
    {
        public static int? GetIntValue(object val)
        {
            if (val == DBNull.Value)
                return null;
            return int.Parse(val.ToString());
        }

        public static string GetStringValue(object val)
        {
            return val.ToString();
        }

        public static string FormatValue(string value)
        {
            return value.Replace(@"'", @"''");
        }

    }

    public sealed class TableInfo
    {
        private static readonly string query = "select rdb$relation_name from rdb$relations where lower(rdb$relation_name) = lower('{0}')";

        public string Name { get; }
        public bool Exists { get; }

        public TableInfo(DataRow drMeta)
            : this(drMeta["rdb$relation_name"].ToString().Trim(), true)
        {
        }

        public TableInfo(string name, bool exists)
        {
            Name = name;
            Exists = exists;
        }

        public static TableInfo Read(FirebirdProcessor processor, string tableName, FirebirdQuoter quoter)
        {
            var fbTableName = quoter.ToFbObjectName(tableName);
            var table = processor.Read(query, AdoHelper.FormatValue(fbTableName)).Tables[0];
            if (table.Rows.Count == 0)
                return new TableInfo(tableName, false);
            return new TableInfo(table.Rows[0]);
        }
    }

    public sealed class ColumnInfo
    {
        private static readonly string query = @"select
                    fields.rdb$field_name as field_name,
                    fields.rdb$relation_name as relation_name,
                    fields.rdb$default_source as default_source,
                    fields.rdb$field_position as field_position,
                    fields.rdb$null_flag as null_flag,
                    fieldinfo.rdb$field_precision as field_precision,
                    fieldinfo.rdb$character_length as field_character_length,
                    fieldinfo.rdb$field_type as field_type,
                    fieldinfo.rdb$field_sub_type as field_sub_type,
                    fieldtype.rdb$type_name as field_type_name

                    from rdb$relation_fields as fields
                    left outer join rdb$fields as fieldinfo on (fields.rdb$field_source = fieldinfo.rdb$field_name)
                    left outer join rdb$types as fieldtype on ( (fieldinfo.rdb$field_type = fieldtype.rdb$type) and (fieldtype.rdb$field_name = 'RDB$FIELD_TYPE') )
                    where (lower(fields.rdb$relation_name) = lower('{0}'))
                    ";

        public string Name { get; }
        public string TableName { get; }
        public object DefaultValue { get; }
        public int Position { get; }
        public DbType? DBType => GetDBType();
        public string CustomType => GetCustomDBType();
        public bool IsNullable { get; }
        public int? Precision { get; }
        public int? CharacterLength { get; }
        public int? FieldType { get; }
        public int? FieldSubType { get; }
        public string FieldTypeName { get; }

        private ColumnInfo(DataRow drColumn)
        {
            Name = AdoHelper.GetStringValue(drColumn["field_name"]).Trim();
            TableName = AdoHelper.GetStringValue(drColumn["relation_name"]).Trim();
            DefaultValue = GetDefaultValue(drColumn["default_source"]);
            Position = AdoHelper.GetIntValue(drColumn["field_position"]) ?? 0;
            IsNullable = AdoHelper.GetIntValue(drColumn["null_flag"]) != 1;
            Precision = AdoHelper.GetIntValue(drColumn["field_precision"]);
            CharacterLength = AdoHelper.GetIntValue(drColumn["field_character_length"]);
            FieldType = AdoHelper.GetIntValue(drColumn["field_type"]);
            FieldSubType = AdoHelper.GetIntValue(drColumn["field_sub_type"]);
            FieldTypeName = AdoHelper.GetStringValue(drColumn["field_type_name"]).Trim();

        }
        public static List<ColumnInfo> Read(FirebirdProcessor processor, TableInfo table)
        {
            using (DataSet ds = processor.Read(query, AdoHelper.FormatValue(table.Name)))
            {
                List<ColumnInfo> rows = new List<ColumnInfo>();
                foreach (DataRow dr in ds.Tables[0].Rows)
                    rows.Add(new ColumnInfo(dr));
                return rows;
            }
        }

        private DbType? GetDBType()
        {
            return null;
        }

        private string GetCustomDBType()
        {
            #region FieldTypes by number
            switch (FieldType)
            {
                case 261:
                    if (FieldSubType.HasValue)
                    {
                        return "BLOB sub_type " + FieldSubType.Value.ToString();
                    }
                    else
                        return "BLOB";
                case 14:
                    return "CHAR";
                case 40:
                    return "CSTRING";
                case 11:
                    return "D_FLOAT";
                case 27:
                    return "DOUBLE";
                case 10:
                    return "FLOAT";
                case 16:
                    return "BIGINT";
                case 8:
                    return "INTEGER";
                case 9:
                    return "QUAD";
                case 7:
                    return "SMALLINT";
                case 12:
                    return "DATE";
                case 13:
                    return "TIME";
                case 35:
                    return "TIMESTAMP";
                case 37:
                    return "VARCHAR(" + CharacterLength.ToString() + ")";
            }
            #endregion

            switch (FieldTypeName)
            {
                case "VARYING":
                    return "VARCHAR(" + CharacterLength.ToString() + ")";
                case "LONG":
                    return "INTEGER";
                case "INT64":
                    return "BIGINT";
            }
            return FieldTypeName;
        }

        private object GetDefaultValue(object val)
        {
            if (val == null)
                return DBNull.Value;

            string src = val.ToString().Trim();
            if (string.IsNullOrEmpty(src))
                return DBNull.Value;

            if (src.StartsWith("DEFAULT ", StringComparison.InvariantCultureIgnoreCase))
            {
                string value = src.Substring(8).Trim();
                if (value.StartsWith("'"))
                {
                    return value.TrimStart('\'').TrimEnd('\'');
                }
                else if (value.Equals("current_timestamp", StringComparison.InvariantCultureIgnoreCase))
                {
                    return SystemMethods.CurrentDateTime;
                }
                else if (value.Equals("gen_uuid()", StringComparison.InvariantCultureIgnoreCase))
                {
                    return SystemMethods.NewGuid;
                }
                else
                {
                    if (int.TryParse(value, out var res))
                        return res;

                }
            }
            throw new NotSupportedException(string.Format("Can't parse default value {0}", src));
        }
    }

    public sealed class IndexInfo
    {
        private static readonly string query = @"select
                rdb$index_name, rdb$relation_name, rdb$unique_flag, rdb$index_type
                from rdb$indices where rdb$relation_name = '{0}'";
        private static readonly string singleQuery = @"select
                rdb$index_name, rdb$relation_name, rdb$unique_flag, rdb$index_type
                from rdb$indices where rdb$index_name = '{0}'";
        private static readonly string indexFieldQuery = @"select rdb$field_name from rdb$index_segments where rdb$index_name = '{0}'";

        public string Name { get; }
        public string TableName { get; }
        public bool IsUnique { get; }
        public bool IsAscending { get; }
        public List<string> Columns { get; }


        private IndexInfo(DataRow drIndex, FirebirdProcessor processor)
        {
            Name = drIndex["rdb$index_name"].ToString().Trim();
            TableName = drIndex["rdb$relation_name"].ToString().Trim();
            IsUnique = drIndex["rdb$unique_flag"].ToString().Trim() == "1";
            IsAscending = drIndex["rdb$index_type"].ToString().Trim() == "0";
            Columns = new List<string>();
            using (DataSet dsColumns = processor.Read(indexFieldQuery, AdoHelper.FormatValue(Name)))
            {
                foreach (DataRow indexColumn in dsColumns.Tables[0].Rows)
                    Columns.Add(indexColumn["rdb$field_name"].ToString().Trim());
            }
        }

        public static List<IndexInfo> Read(FirebirdProcessor processor, TableInfo table)
        {
            using (DataSet ds = processor.Read(query, AdoHelper.FormatValue(table.Name)))
            {
                List<IndexInfo> rows = new List<IndexInfo>();
                foreach (DataRow dr in ds.Tables[0].Rows)
                    rows.Add(new IndexInfo(dr, processor));
                return rows;
            }
        }

        public static IndexInfo Read(FirebirdProcessor processor, string indexName)
        {
            using (DataSet ds = processor.Read(singleQuery, AdoHelper.FormatValue(indexName)))
            {
                return new IndexInfo(ds.Tables[0].Rows[0], processor);
            }
        }
    }

    public sealed class ConstraintInfo
    {
        private static readonly string query = @"select
                rdb$constraint_name, rdb$constraint_type, rdb$index_name
                from rdb$relation_constraints where rdb$relation_name = '{0}'";
        private static readonly string colQuery = @"select
                rdb$const_name_uq, rdb$update_rule, rdb$delete_rule
                from rdb$ref_constraints where rdb$constraint_name = '{0}'";

        public string Name { get; }
        public bool IsPrimaryKey { get; }
        public bool IsUnique { get; }
        public bool IsNotNull { get; }
        public bool IsForeignKey { get; }
        public string IndexName { get; }
        public IndexInfo ForeignIndex { get; }
        public Rule UpdateRule { get; }
        public Rule DeleteRule { get; }

        private ConstraintInfo(DataRow drConstraint, FirebirdProcessor processor)
        {
            Name = drConstraint["rdb$constraint_name"].ToString().Trim();
            IsPrimaryKey = drConstraint["rdb$constraint_type"].ToString().Trim() == "PRIMARY KEY";
            IsNotNull = drConstraint["rdb$constraint_type"].ToString().Trim() == "NOT NULL";
            IsForeignKey = drConstraint["rdb$constraint_type"].ToString().Trim() == "FOREIGN KEY";
            IsUnique = drConstraint["rdb$constraint_type"].ToString().Trim() == "UNIQUE";
            IndexName = drConstraint["rdb$index_name"].ToString().Trim();

            if (IsForeignKey)
            {
                using (DataSet dsForeign = processor.Read(colQuery, AdoHelper.FormatValue(Name)))
                {
                    DataRow drForeign = dsForeign.Tables[0].Rows[0];
                    ForeignIndex = IndexInfo.Read(processor, IndexName);
                    UpdateRule = GetForeignRule(drForeign["rdb$update_rule"]);
                    DeleteRule = GetForeignRule(drForeign["rdb$delete_rule"]);
                }
            }
        }

        private Rule GetForeignRule(object val)
        {
            if (val == null)
                return Rule.None;
            string ruleName = AdoHelper.GetStringValue(val);
            switch (ruleName)
            {
                case "CASCADE":
                    return Rule.Cascade;
                case "SET NULL":
                    return Rule.SetNull;
                case "SET DEFAULT":
                    return Rule.SetDefault;
                // ReSharper disable once RedundantCaseLabel
                case "RESTRICT":
                default:
                    return Rule.None;
            }
        }

        public static List<ConstraintInfo> Read(FirebirdProcessor processor, TableInfo table)
        {
            using (DataSet ds = processor.Read(query, AdoHelper.FormatValue(table.Name)))
            {
                List<ConstraintInfo> rows = new List<ConstraintInfo>();
                foreach (DataRow dr in ds.Tables[0].Rows)
                    rows.Add(new ConstraintInfo(dr, processor));
                return rows;
            }
        }
    }

    public enum TriggerEvent { Insert, Update, Delete }
    public sealed class TriggerInfo
    {
        private static readonly string query = @"select
                rdb$trigger_name, rdb$trigger_sequence, rdb$trigger_type, rdb$trigger_source
                from rdb$triggers where rdb$relation_name = '{0}'";

        public string Name { get; }
        public int Sequence { get; }
        public int Type { get; }
        public string Body { get; }
        public bool Before => Type % 2 == 1;
        public bool OnInsert => Type == 1 || Type == 2;
        public bool OnUpdate => Type == 3 || Type == 4;
        public bool OnDelete => Type == 5 || Type == 6;
        public TriggerEvent Event => OnInsert ? TriggerEvent.Insert : OnUpdate ? TriggerEvent.Update : TriggerEvent.Delete;

        private TriggerInfo(DataRow drTrigger)
        {
            Name = drTrigger["rdb$trigger_name"].ToString().Trim();
            Sequence = AdoHelper.GetIntValue(drTrigger["rdb$trigger_sequence"]) ?? 0;
            Type = AdoHelper.GetIntValue(drTrigger["rdb$trigger_type"]) ?? 1;
            Body = drTrigger["rdb$trigger_source"].ToString().Trim();
        }

        public static List<TriggerInfo> Read(FirebirdProcessor processor, TableInfo table)
        {
            using (DataSet ds = processor.Read(query, AdoHelper.FormatValue(table.Name)))
            {
                List<TriggerInfo> rows = new List<TriggerInfo>();
                foreach (DataRow dr in ds.Tables[0].Rows)
                    rows.Add(new TriggerInfo(dr));
                return rows;
            }
        }
    }

    public sealed class SequenceInfo
    {
        private static readonly string query = @"select rdb$generator_name from rdb$generators where rdb$generator_name = '{0}'";
        private static readonly string queryValue = "select gen_id(\"{0}\", 0) as gen_val from rdb$database";

        public string Name { get; }
        public int CurrentValue{get; }

        private SequenceInfo(DataRow drSequence, FirebirdProcessor processor)
        {
            Name = drSequence["rdb$generator_name"].ToString().Trim();
            using (DataSet ds = processor.Read(queryValue, Name))
            {
                CurrentValue = AdoHelper.GetIntValue(ds.Tables[0].Rows[0]["gen_val"]) ?? 0;
            }
        }

        public static SequenceInfo Read(FirebirdProcessor processor, string sequenceName, FirebirdQuoter quoter)
        {
            var fbSequenceName = quoter.ToFbObjectName(sequenceName);
            using (DataSet ds = processor.Read(query, AdoHelper.FormatValue(fbSequenceName)))
            {
                return new SequenceInfo(ds.Tables[0].Rows[0], processor);
            }
        }
    }

}
