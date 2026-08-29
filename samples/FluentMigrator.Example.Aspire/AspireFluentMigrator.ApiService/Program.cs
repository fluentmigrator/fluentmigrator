using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDataSource("migrationdb");

var app = builder.Build();

app.MapGet("/", async (NpgsqlDataSource dataSource) =>
{
    await using var conn = await dataSource.OpenConnectionAsync();

    await using var insertCmd = new NpgsqlCommand(
        "INSERT INTO \"Contexts\" (\"Name\") VALUES (@name)", conn);
    insertCmd.Parameters.AddWithValue("name", $"Context {Guid.NewGuid():N}");
    await insertCmd.ExecuteNonQueryAsync();

    await using var selectCmd = new NpgsqlCommand(
        "SELECT \"Id\", \"Name\" FROM \"Contexts\" ORDER BY \"Id\"", conn);
    await using var reader = await selectCmd.ExecuteReaderAsync();

    var contexts = new List<object>();
    while (await reader.ReadAsync())
    {
        contexts.Add(new
        {
            id = reader.GetInt32(0),
            name = reader.GetString(1)
        });
    }

    return new
    {
        totalContexts = contexts.Count,
        contexts
    };
});

app.MapDefaultEndpoints();

app.Run();
