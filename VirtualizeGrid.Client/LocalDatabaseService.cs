using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;
using static VirtualizeGrid.Client.Pages.Home;

namespace VirtualizeGrid.Client;

public class LocalDatabaseService
{
    private const string DbName = "cachedData.db";
    private readonly SqliteConnection _connection;

    public LocalDatabaseService()
    {
        _connection = new SqliteConnection($"Filename={DbName}");
        _connection.Open();

        // Enable WAL mode for better write performance
        using var walCmd = _connection.CreateCommand();
        walCmd.CommandText = "PRAGMA journal_mode=WAL;";
        walCmd.ExecuteNonQuery();

        // Create table if it doesn't exist
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Parts (
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                CreationDate TEXT NOT NULL,
                Status TEXT NOT NULL
            );";
        cmd.ExecuteNonQuery();
    }

    public IDbConnection Connection => _connection;

    public async Task CachePartsDataAsync(IEnumerable<PartDto> parts)
    {
        var query = "INSERT OR REPLACE INTO Parts (Id, Name, CreationDate, Status) VALUES (@Id, @Name, @CreationDate, @Status)";

        using var transaction = _connection.BeginTransaction();
        try
        {
            await Connection.ExecuteAsync(query, parts, transaction: transaction);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<List<PartDto>> LoadCachedPartsDataAsync()
    {
        var query = "SELECT * FROM Parts";
        var queryResult = await Connection.QueryAsync<PartDto>(query);
        return queryResult.ToList();
    }
}
