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
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Expressions
{
    public class UpdateDataExpression : MigrationExpressionBase
    {
        public string SchemaName { get; set; }
        public string TableName { get; set; }

        public List<KeyValuePair<string, object>> Set { get; set; }
        public List<KeyValuePair<string, object>> Where { get; set; }
        public bool IsAllRows { get; set; }

        public override void CollectValidationErrors(ICollection<string> errors)
        {
            if (String.IsNullOrEmpty(TableName))
                errors.Add(ErrorMessages.TableNameCannotBeNullOrEmpty);

            if (!IsAllRows && (Where == null || Where.Count == 0)) 
                errors.Add(ErrorMessages.UpdateDataExpressionMustSpecifyWhereClauseOrAllRows);

            if (IsAllRows && Where != null && Where.Count > 0)
                errors.Add(ErrorMessages.UpdateDataExpressionMustNotSpecifyBothWhereClauseAndAllRows);
        }

        public override void ExecuteWith(IMigrationProcessor processor)
        {
            processor.Process(this);
        }
    }
}
