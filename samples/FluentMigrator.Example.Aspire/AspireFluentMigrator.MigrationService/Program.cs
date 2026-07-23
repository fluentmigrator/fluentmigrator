using AspireFluentMigrator.MigrationService;
using FluentMigrator.Example.Migrations;
using FluentMigrator.Runner;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHostedService<Worker>();

builder.Services
    .AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddPostgres()
        .WithGlobalConnectionString(
            builder.Configuration.GetConnectionString("migrationdb"))
        .ScanIn(typeof(AddGTDTables).Assembly).For.Migrations())
    .AddLogging(lb => lb.AddFluentMigratorConsole());

var app = builder.Build();

app.Run();
