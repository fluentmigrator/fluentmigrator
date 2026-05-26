using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDataSource("migrationdb");

var app = builder.Build();

app.MapGet("/", async (NpgsqlDataSource dataSource) =>
{
    await using var conn = await dataSource.OpenConnectionAsync();

    await using var insertCmd = new NpgsqlCommand(
        "INSERT INTO entries (id, created_at) VALUES (@id, @createdAt)", conn);
    insertCmd.Parameters.AddWithValue("id", Guid.NewGuid());
    insertCmd.Parameters.AddWithValue("createdAt", DateTime.UtcNow);
    await insertCmd.ExecuteNonQueryAsync();

    await using var selectCmd = new NpgsqlCommand(
        "SELECT id, created_at FROM entries ORDER BY created_at", conn);
    await using var reader = await selectCmd.ExecuteReaderAsync();

    var entries = new List<object>();
    while (await reader.ReadAsync())
    {
        entries.Add(new
        {
            id = reader.GetGuid(0),
            createdAt = reader.GetDateTime(1)
        });
    }

    return new
    {
        totalEntries = entries.Count,
        entries
    };
});

app.MapDefaultEndpoints();

app.Run();
