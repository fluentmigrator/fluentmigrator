#region License
// Copyright (c) 2024, Fluent Migrator Project
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
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.Extensions.DependencyInjection;

namespace FluentMigrator.EFCore;

public class FluentMigratorDesignTimeServices : IDesignTimeServices
{
    public void ConfigureDesignTimeServices(IServiceCollection services)
    {
        services.AddSingleton<ICSharpMigrationOperationGenerator, FluentMigratorCSharpMigrationOperationGenerator>();
        services.AddSingleton<IMigrationsCodeGenerator, FluentMigratorCSharpMigrationsGenerator>();
        services.AddSingleton<IMigrationsScaffolder, FluentMigratorMigrationsScaffolder>();
    }
}
