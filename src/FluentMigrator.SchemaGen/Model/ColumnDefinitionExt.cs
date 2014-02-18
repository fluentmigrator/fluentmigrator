using System;
using System.Data;
using System.Text;
using FluentMigrator.Model;
using FluentMigrator.SchemaGen.Extensions;
using FluentMigrator.SchemaGen.SchemaReaders;
using FluentMigrator.SchemaGen.SchemaWriters;

namespace FluentMigrator.SchemaGen.Model
{
    public class ColumnDefinitionExt : ColumnDefinition, ICodeComparable
    {
        private string codeDef;
        public virtual string SchemaName { get; set; }
        public virtual string IndexName { get; set; }

        public string FQName
        {
            get { return Name; }
        }

        public string CreateCode
        {
            get { return string.Format(".WithColumn(\"{0}\").{1}", Name, GetColumnCode()); }
        }

        public string DeleteCode
        {
            get { return string.Format("Delete.Column(\"{0}\").FromTable(\"{1}\").InSchema(\"{2}\");", Name, TableName, SchemaName); }
        }

        public string DefinitionCode
        {
            get { return codeDef ?? (codeDef = GetColumnCode()); }
        }

        public bool TypeChanged
        {
            get { return false; }
        }

        public string GetRemoveColumnCode()
        {
            return string.Format("Delete.Column(\"{0}\").FromTable(\"{1}\").InSchema(\"{2}\");", Name, TableName, SchemaName);
        }

        public string GetColumnCode()
        {
            var sb = new StringBuilder();

            sb.Append(GetMigrationTypeFunctionForType());

            if (IsIdentity) 
            {
                sb.Append(".Identity()");
            }

            if (IsPrimaryKey)
            {
                //sb.AppendFormat(".PrimaryKey(\"{0}\")", column.PrimaryKeyName);
                sb.AppendFormat(".PrimaryKey()");
            }
            else if (IsUnique)
            {
                sb.AppendFormat(".Unique(\"{0}\")", IndexName);
            }
            else if (IsIndexed)
            {
                sb.AppendFormat(".Indexed(\"{0}\")", IndexName);
            }

            if (IsNullable.HasValue)
            {
                sb.Append(IsNullable.Value ? ".Nullable()" : ".NotNullable()");
            }

            if (DefaultValue != null && !IsIdentity)
            {
                sb.AppendFormat(".WithDefaultValue({0})", GetColumnDefaultValue());
            }

            //if (lastColumn) sb.Append(";");
            return sb.ToString();
        }

        public string GetColumnDefaultValue()
        {
            string sysType = null;
            string defValue = DefaultValue.ToString().CleanBracket().ToUpper().Trim();

            var guid = Guid.Empty;
            switch (Type)
            {
                case DbType.Boolean:
                case DbType.Byte:
                case DbType.Currency:
                case DbType.Decimal:
                case DbType.Double:
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.Single:
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                    sysType = defValue.Replace("'", "").Replace("\"", "").CleanBracket();
                    break;

                case DbType.Guid:
                    if (defValue == "NEWID()")
                    {
                        sysType = "SystemMethods.NewGuid";
                    } else if (defValue == "NEWSEQUENTIALID()")
                    {
                        sysType = "SystemMethods.NewSequentialId";
                    }
                    else if (defValue.IsGuid(out guid))
                    {
                        if (guid == Guid.Empty)
                        {
                            sysType = "Guid.Empty";
                        }
                        else
                        {
                            sysType = string.Format("new System.Guid(\"{0}\")", guid);
                        }
                    }
                    break;

                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.Date:
                    if (defValue == "CURRENT_TIME"
                        || defValue == "CURRENT_DATE"
                        || defValue == "CURRENT_TIMESTAMP"
                        || defValue == "GETDATE()")
                    {
                        sysType = "SystemMethods.CurrentDateTime";
                    }
                    else if (defValue == "GETUTCDATE()")
                    {
                        sysType = "SystemMethods.CurrentUTCDateTime";
                    }
                    else
                    {
                        sysType = "\"" + defValue + "\"";
                    }
                    break;

                default:
                    if (defValue == "CURRENT_USER")
                    {
                        sysType = "SystemMethods.CurrentUser";
                    }
                    else
                    {
                        sysType = string.Format("\"{0}\"", DefaultValue);
                    }
                    break;
            }

            return sysType.Replace("'", "''");
        }

        public string GetMigrationTypeFunctionForType()
        {
            var options = Options.Instance;
            var precision = Precision;
            string sizeStr = GetMigrationTypeSize(Type, Size);
            string precisionStr = (precision == -1) ? "" : "," + precision.ToString();
            string sysType = "AsString(" + sizeStr + ")";

            switch (Type)
            {
                case DbType.AnsiString:
                    if (options.UseDeprecatedTypes && Size == DbTypeSizes.AnsiTextCapacity)
                    {
                        sysType = "AsCustom(\"TEXT\")";
                    }
                    else
                    {
                        sysType = string.Format("AsAnsiString({0})", sizeStr);
                    }
                    break;
                case DbType.AnsiStringFixedLength:
                    sysType = string.Format("AsFixedLengthAnsiString({0})", sizeStr);
                    break;
                case DbType.String:
                    if (options.UseDeprecatedTypes && Size == DbTypeSizes.UnicodeTextCapacity)
                    {
                        sysType = "AsCustom(\"NTEXT\")";
                    }
                    else
                    {
                        sysType = string.Format("AsString({0})", sizeStr);
                    }
                    break;
                case DbType.StringFixedLength:
                    sysType = string.Format("AsFixedLengthString({0})", sizeStr);
                    break;
                case DbType.Binary:
                    if (options.UseDeprecatedTypes && Size == DbTypeSizes.ImageCapacity)
                    {
                        sysType = "AsCustom(\"IMAGE\")";
                    }
                    else
                    {
                        sysType = string.Format("AsBinary({0})", sizeStr);
                    }
                    break;
                case DbType.Boolean:
                    sysType = "AsBoolean()";
                    break;
                case DbType.Byte:
                    sysType = "AsByte()";
                    break;
                case DbType.Currency:
                    sysType = "AsCurrency()";
                    break;
                case DbType.Date:
                    sysType = "AsDate()";
                    break;
                case DbType.DateTime:
                    sysType = "AsDateTime()";
                    break;
                case DbType.Decimal:
                    sysType = string.Format("AsDecimal({0})", sizeStr + precisionStr);
                    break;
                case DbType.Double:
                    sysType = "AsDouble()";
                    break;
                case DbType.Guid:
                    sysType = "AsGuid()";
                    break;
                case DbType.Int16:
                case DbType.UInt16:
                    sysType = "AsInt16()";
                    break;
                case DbType.Int32:
                case DbType.UInt32:
                    sysType = "AsInt32()";
                    break;
                case DbType.Int64:
                case DbType.UInt64:
                    sysType = "AsInt64()";
                    break;
                case DbType.Single:
                    sysType = "AsFloat()";
                    break;
                case null:
                    sysType = string.Format("AsCustom({0})", CustomType);
                    break;
                default:
                    break;
            }

            return sysType;
        }

        private string GetMigrationTypeSize(DbType? type, int size)
        {
            if (size == -1) return "int.MaxValue";

            if (type == DbType.Binary && size == DbTypeSizes.ImageCapacity) return "DbTypeSizes.ImageCapacity";              // IMAGE fields
            if (type == DbType.AnsiString && size == DbTypeSizes.AnsiTextCapacity) return "DbTypeSizes.AnsiTextCapacity";    // TEXT fields
            if (type == DbType.String && size == DbTypeSizes.UnicodeTextCapacity) return "DbTypeSizes.UnicodeTextCapacity";  // NTEXT fields

            return size.ToString();
        }
    }
}