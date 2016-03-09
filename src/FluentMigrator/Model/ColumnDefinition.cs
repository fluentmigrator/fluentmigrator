#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Model
{
    public class ColumnDefinition : ICloneable, ICanBeConventional, ICanBeValidated, ISupportAdditionalFeatures
    {
        public ColumnDefinition()
        {
            DefaultValue = new UndefinedDefaultValue();
        }

        public readonly Dictionary<string, object> _additionalFeatures = new Dictionary<string, object>();

        public virtual string Name { get; set; }
        public virtual DbType? Type { get; set; }
        public virtual int Size { get; set; }
        public virtual int Precision { get; set; }
        public virtual string CustomType { get; set; }
        public virtual object DefaultValue { get; set; }
        public virtual bool IsForeignKey { get; set; }
        public virtual bool IsIdentity { get; set; }
        public virtual bool IsIndexed { get; set; }
        public virtual bool IsPrimaryKey { get; set; }
        public virtual string PrimaryKeyName { get; set; }
        public virtual bool? IsNullable { get; set; }
        public virtual bool IsUnique { get; set; }
        public virtual string TableName { get; set; }
        public virtual ColumnModificationType ModificationType { get; set; }
        public virtual string ColumnDescription { get; set; }
        public virtual string CollationName { get; set; }

        public void ApplyConventions(IMigrationConventions conventions)
        {
            if (String.IsNullOrEmpty(PrimaryKeyName))
                PrimaryKeyName = conventions.GetPrimaryKeyName(TableName);
        }

        public virtual void CollectValidationErrors(ICollection<string> errors)
        {
            if (String.IsNullOrEmpty(Name))
                errors.Add(ErrorMessages.ColumnNameCannotBeNullOrEmpty);

            if (Type == null && CustomType == null)
                errors.Add(ErrorMessages.ColumnTypeMustBeDefined);
        }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        public class UndefinedDefaultValue
        {
        }

        public IDictionary<string, object> AdditionalFeatures
        {
            get { return _additionalFeatures; }
        }

        void ISupportAdditionalFeatures.AddAdditionalFeature(string feature, object value)
        {
            if (!AdditionalFeatures.ContainsKey(feature))
            {
                AdditionalFeatures.Add(feature, value);
            }
            else
            {
                AdditionalFeatures[feature] = value;
            }
        }

        public T GetAdditionalFeature<T>(string key, T defaultValue)
        {
            if (AdditionalFeatures.ContainsKey(key))
            {
                object value = AdditionalFeatures[key];
                if (value is T)
                {
                    return (T)value;
                }
            }

            return defaultValue;
        }
    }

    public enum ColumnModificationType
    {
        Create,
        Alter
    }
}
