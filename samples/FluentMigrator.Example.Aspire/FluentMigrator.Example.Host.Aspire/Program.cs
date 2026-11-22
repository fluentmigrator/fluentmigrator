using FluentMigrator.Example.Host.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var db = postgres.AddDatabase("migrationdb");

var api = builder.AddProject<Projects.AspireFluentMigrator_ApiService>("api")
    .WithReference(db);

var apiMigrations = builder.AddFluentMigratorMigrations("api-migrations", db);

api.WaitForCompletion(apiMigrations);

builder.Build().Run();
