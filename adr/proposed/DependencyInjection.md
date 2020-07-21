@iceoz raised in https://github.com/fluentmigrator/fluentmigrator/issues/1255 the idea to make how dependency injection works more flexible.

Here are my thoughts on simplifying the design here.

# Three levels of abstractions for Dependency Injection
At the heart of a flexible framework that allows for DI are three components:

1. Component that gets types
2. Component that instantiates the types
3. Component that returns the types

Levels two and three are basically combine-able - there's usually not a lot of benefit to creating the instance and returning it separately.

In FluentMigrator, as we'll see in my table constructed below, sometimes components 1 and 2 are combined, and sometimes 3 is a standalone component.  More often than not, components 2 and 3 are combined.

Whether we use Microsoft.Extensions.DependencyInjection is somewhat irrelevant.  We just need two components: A component that gets types and a component that instantiates types.

The only scenario where it would be nice to use the types from `Microsoft.Extensions.DependencyInjection.IServiceCollection` type universe is cross-wiring dependencies.  The problem is you can't pass around `IServiceCollection`, because an `IServiceCollection` is supposed to be converted into an IServiceProvider once-and-only-once.  Once you've made the conversion, mutating the `IServiceCollection` instance further would require creating a second `IServiceProvider`, which in turn could create some rather hard to debug object lifetime scope issues.  In this way, it's probably better that FluentMigrator provide an abstract interface for "component that gets types" and let the concrete implementation inject an IServiceProvider if the end-user needs to pull in any instances from, say, the ASP.NET Core Generic Host container.

# Injectability (Complex Objects) vs. Configuration (Simple Property)
Should something be a parameter (static value) or injectable (factory that can return slightly different object depending on a Context)?
1. If it's injectable, it needs an interface and a default implementation.
2. If it's injectable, should it use an [Abstract Factory or a Service Locator](https://blog.ploeh.dk/2010/11/01/PatternRecognitionAbstractFactoryorServiceLocator/)? 
3. If it's just a parameter, it should be something you can easily configure from an `IApplication` instance.  See: #1208 
4. If it's just a parameter, ideally the paramter is effectively just a Service Locator key/object context and is itself used to resolve something from DI.  In other words, for things like connection strings, the functional transformation is: `string -> IConnectionStringAccessor -> string`.  In this way, there is less code to maintain.

# The list of different things FluentMigrator cares about

| ##   | Object Concept                       | Interface                                        | Type Getter                                       | Type Instantiator                            |
| ---- | -------------------------------------| ------------------------------------------------ | ------------------------------------------------- | -------------------------------------------- |
| 1.   | Migration Runner                     | `IMigrationRunner`                               | MS DI `IServiceCollection`                        | MS DI `System.IServiceProvider`              |
| 2.   | Migration Runner Conventions         | `IMigrationRunnerConventions`                    | `IMigrationRunnerConventionsAccessor`             |                                              |
| 3.   | Migrations                           | `IMigration`                                     | `IMigrationSource` or `IFilteringMigrationSource` | Same as Type Getter*                         |
| 4.   | Profiles as **Migration Type**       | `IEnumerable<IMigration>`                        | `IProfileSource`                                  | `IProfileLoader`                             |
| 5.   | Maintenance as **Migration Type**    | `IMigration` decorated w/ `MaintenanceAttribute` |                                                   | `IMaintenanceLoader`                         |
| 6.   | Tags as **Migration Filter**         | `TagsAttribute`                                  |                                                   |                                              |
| 7.   | Trait as **Migration Filter**        | `MigrationTraitAttribute`                        | _not injectable_                                  | _not injectable_                             |
| 8.   | Constraints as **Migration Filter**  | `MigrationConstraintAttribute`                   |                                                   |                                              |
| 9.   | Journaling (Version Table Metadata)  | `IVersionTableMetaData`                          | `AssemblyVersionTableMetaDataSourceItem`          | `AssemblySourceVersionTableMetaDataAccessor` |
| 10.  | Logging as **ILoggerProvider**       | `ILoggerProvider`                                | MS DI `IServiceCollection`                        | MS DI `IServiceCollection.Configure`         |
| 11.  | Mapping FM Types to DB Types         | `ITypeMap`                                       |                                                   |                                              |
| 12.  | Connection strings                   | string                                           | `IConnectionStringReader`                         | `IConnectionStringAccessor`                  |


1. Migration Runner. See also:
    1. Interface: [`IMigrationRunner`](https://github.com/fluentmigrator/fluentmigrator/blob/e82aafa20e6dbe3cefa221303fe23cf8bf59fffd/src/FluentMigrator.Runner/IMigrationRunner.cs)
    2. Type Getter: MS DI [`IServiceCollection`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection?view=dotnet-plat-ext-3.1)
    3. Type Instantiator: MS DI [`System.IServiceProvider`](https://docs.microsoft.com/en-us/dotnet/api/system.iserviceprovider?view=netcore-3.1&viewFallbackFrom=dotnet-plat-ext-3.1)
2. Migration Runner Conventions. See also:
    1. Interface: [`IMigrationRunnerConventions`](https://github.com/fluentmigrator/fluentmigrator/blob/e82aafa20e6dbe3cefa221303fe23cf8bf59fffd/src/FluentMigrator.Runner.Core/IMigrationRunnerConventions.cs)
	2. Type Getter:
	     1. Interface: [`IMigrationRunnerConventionsAccessor`](https://github.com/fluentmigrator/fluentmigrator/blob/e82aafa20e6dbe3cefa221303fe23cf8bf59fffd/src/FluentMigrator.Runner.Core/Initialization/IMigrationRunnerConventionsAccessor.cs)
	     2. Implementations
	         1: [`AssemblySourceMigrationRunnerConventionsAccessor`](https://github.com/fluentmigrator/fluentmigrator/blob/e82aafa20e6dbe3cefa221303fe23cf8bf59fffd/src/FluentMigrator.Runner.Core/Initialization/AssemblySourceMigrationRunnerConventionsAccessor.cs)
	3. Type Instantiator: MS DI [``]()
3. Migrations
    1. Interface: `IMigration`
	2. Type Getter: `IMigrationSource` or `IFilteringMigrationSource`
	3. Type Instantiator: Same as Type Getter*. However, IMigrationInformationLoader  returns types.
4. Profiles
    1. Interface: `IEnumerable<IMigration>`
	2. Type Getter: `IProfileSource`
	3. Type Instantiator: `IProfileLoader`
5. Maintenance
    1. Interface: `IMigration` decorated w/ `MaintenanceAttribute`
	2. Type Getter: 
11. Connection strings
    1. Interface: string
	2. Type Getter:
	    1. Interface: [`IConnectionStringReader`](https://github.com/fluentmigrator/fluentmigrator/blob/e82aafa20e6dbe3cefa221303fe23cf8bf59fffd/src/FluentMigrator.Runner.Core/Initialization/IConnectionStringReader.cs)
		2. Implementations: 

## Additional Notes

### 9. Journaling
The code here is a bit less than ideal.  `AssemblySourceVersionTableMetaDataAccessor` contains logic that should belong to `AssemblyVersionTableMetaDataSourceItem`.  `AssemblyVersionTableMetaDataSourceItem` should probably have a better name.

Right now, the way we do journaling has a couple of limitations.
1. Users cannot add columns to the journal table
2. Users cannot query the journal table with incremental progress
3. Ownership of the version table metadata transaction is ambiguous

### 10. Type Maps
1. Used by FluentMigrator to map FluentMigrator/.NET types to target database's types.
2. Outstanding request to make it injectable, here: #1063 

### 11. Connections/connection strings
See: https://github.com/fluentmigrator/fluentmigrator/issues/1075 - Ability to configure runner with multiple connections

