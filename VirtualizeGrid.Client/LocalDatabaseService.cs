using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;
using static VirtualizeGrid.Client.Pages.Home;
using System.Diagnostics;

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
        return;
        var clearTableQuery = "DELETE FROM Parts;";
        var insertQuery = "INSERT INTO Parts (Id, Name, CreationDate, Status) VALUES (@Id, @Name, @CreationDate, @Status)";

        using var transaction = _connection.BeginTransaction();
        try
        {
            // Clear existing records
            await Connection.ExecuteAsync(clearTableQuery, transaction: transaction);

            // Insert new records in bulk
            int rowsAffected = await Connection.ExecuteAsync(insertQuery, parts, transaction: transaction);
            transaction.Commit();

            // Debugging: Output number of rows inserted
            Debug.WriteLine($"Rows inserted: {rowsAffected}");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            Debug.WriteLine($"Error during insertion: {ex.Message}");
            throw;
        }
    }

    public async Task<List<PartDto>> LoadCachedPartsDataAsync()
    {
        var query = "SELECT * FROM Parts";
        try
        {
            var queryResult = await Connection.QueryAsync<PartDto>(query);

            // Debugging: Output number of rows fetched
            Debug.WriteLine($"Rows fetched: {queryResult.Count()}");

            return queryResult.ToList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error during reading: {ex.Message}");
            throw;
        }
    }
}
