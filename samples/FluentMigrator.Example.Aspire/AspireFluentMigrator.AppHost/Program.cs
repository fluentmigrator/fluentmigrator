var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var db = postgres.AddDatabase("migrationdb");

var migrationService = builder.AddProject<Projects.AspireFluentMigrator_MigrationService>("migration")
    .WithReference(db)
    .WaitFor(db);

builder.AddProject<Projects.AspireFluentMigrator_ApiService>("api")
    .WithReference(db)
    .WaitForCompletion(migrationService);

builder.Build().Run();
