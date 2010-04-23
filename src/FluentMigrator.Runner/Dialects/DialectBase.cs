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
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Dialects
{
	public abstract class DialectBase : IDialect
	{
		protected const string SizePlaceholder = "$size";
		protected const string PrecisionPlaceholder = "$precision";

		private readonly Dictionary<DbType, SortedList<int, string>> _templates = new Dictionary<DbType, SortedList<int, string>>();

		public void SetTypeMap(DbType type, string template)
		{
			EnsureHasList(type);
			_templates[type][0] = template;
		}

		public void SetTypeMap(DbType type, string template, int maxSize)
		{
			EnsureHasList(type);
			_templates[type][maxSize] = template;
		}

		public string GetTypeMap(DbType type, int size, int precision)
		{
			if (!_templates.ContainsKey(type))
				throw new NotSupportedException(String.Format("Unsupported DbType '{0}'", type));

			if (size == 0)
				return ReplacePlaceholders(_templates[type][0], size, precision);

			foreach (KeyValuePair<int, string> entry in _templates[type])
			{
				int capacity = entry.Key;
				string template = entry.Value;

				if (capacity <= size)
					return ReplacePlaceholders(template, size, precision);
			}

			throw new NotSupportedException(String.Format("Unsupported DbType '{0}'", type));
		}

		public abstract string GenerateDDLForColumn(ColumnDefinition column);

		protected string ReplacePlaceholders(string value, int size, int precision)
		{
			return value.Replace(SizePlaceholder, size.ToString())
				.Replace(PrecisionPlaceholder, precision.ToString());
		}

		private void EnsureHasList(DbType type)
		{
			if (!_templates.ContainsKey(type))
				_templates.Add(type, new SortedList<int, string>());
		}
	}
}