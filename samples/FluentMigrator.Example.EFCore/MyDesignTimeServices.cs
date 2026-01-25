#region License
// Copyright (c) 2026, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace FluentMigrator.EFCore.Example;

public class MyDesignTimeServices : IDesignTimeServices
{
    public void ConfigureDesignTimeServices(IServiceCollection services)
    {
        services.ConfigureFluentMigratorMigrationGenerator(options =>
        {
            options.BaseMigrationClass = nameof(MyBaseMigration);
            options.TableNameTransformer = name => name.ToUpperInvariant();
            options.ColumnNameTransformer = name => ToSnakeCase(name);
            options.TimestampProvider = (format) => DateTime.Now.ToString(format);
            options.TimestampFormat = "yyyy_MM_dd_HH_mm_ss";
            // ... etc
        });
    }

    private string ToSnakeCase(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        var result = "";
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c))
            {
                if (i > 0)
                {
                    result += "_";
                }

                result += char.ToLower(c);
            }
            else
            {
                result += c;
            }
        }
        return result;
    }
}
