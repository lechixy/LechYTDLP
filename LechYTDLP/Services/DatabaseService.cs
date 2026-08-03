using LechYTDLP.Classes;
using LechYTDLP.Components;
using LechYTDLP.Util;
using Microsoft.Data.Sqlite;
using Sentry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace LechYTDLP.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private static readonly SemaphoreSlim _migrationLock = new(1, 1);

        public DatabaseService()
        {
            string localDb = Path.Combine(ApplicationData.Current.LocalFolder.Path, "history.db");
            string documentsDb = Path.Combine(LechKnownFolders.GetPath(LechKnownFolder.Documents), "LechYTDLP\\Database\\history.db");

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(localDb)!);

                if (!File.Exists(localDb))
                {
                    // We check if the old database exists and copy it to the new location
                    // Start with v1.6.0, we moved the database to the Documents folder for better accessibility.
                    // Start with v2.0.1, we moved the database to the LocalFolder for better compatibility with UWP apps.
                    if (File.Exists(documentsDb))
                    {
                        File.Copy(documentsDb, localDb, true);

                        // If the copy was successful, we can delete the old database to free up space.
                        if (File.Exists(localDb))
                        {
                            File.Delete(documentsDb);
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Database file not found. Creating a new one.");
                        File.Create(localDb).Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                Debug.WriteLine($"Error while checking/creating database file: {ex.Message}");
                throw;
            }

            SentrySdk.SetTag("DatabasePath", localDb);
            Debug.WriteLine($"Database path: {localDb}");
            _connectionString = $"Data Source={localDb}";
        }

        public async Task InitializeAsync()
        {
            await _semaphore.WaitAsync();

            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                await RunMigrationsAsync(connection);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task AddOrUpdateAsync(DownloadItem item)
        {
            await _semaphore.WaitAsync();
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText =
                @"
                INSERT INTO Downloads
                (GuidId, Url, InfoJson, State, Progress, SelectedFormatJson, SelectedFormatsJson, FilePath, Meta, CreatedAt)
                VALUES
                ($guidid, $url, $info, $state, $progress, $format, $selectedFormats, $filePath, $meta, $createdAt);
            ";

                command.Parameters.AddWithValue("$guidid", item.Id.ToString());
                command.Parameters.AddWithValue("$url", item.Url);
                command.Parameters.AddWithValue("$info", JsonSerializer.Serialize(item.Info, App.JsonSerializerOptions));
                command.Parameters.AddWithValue("$state", (int)item.State);
                command.Parameters.AddWithValue("$progress", item.Progress);
                command.Parameters.AddWithValue("$format", JsonSerializer.Serialize(item.SelectedFormat, App.JsonSerializerOptions));
                command.Parameters.AddWithValue("$selectedFormats", JsonSerializer.Serialize(item.SelectedFormats, App.JsonSerializerOptions));
                command.Parameters.AddWithValue("$filePath", item.FilePath);
                command.Parameters.AddWithValue("$meta", JsonSerializer.Serialize(item.Meta, App.JsonSerializerOptions));
                command.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("o"));

                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("There is an error while adding or updating a download item: " + ex.Message);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<DownloadItem>> GetAllAsync()
        {
            var list = new List<DownloadItem>();

            await _semaphore.WaitAsync();
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText =
                    "SELECT Id, GuidId, Url, InfoJson, State, Progress, SelectedFormatJson, SelectedFormatsJson, FilePath, Meta FROM Downloads ORDER BY Id ASC;";

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    // Id is at index 0, but we don't need it since we have GuidId
                    list.Add(new DownloadItem
                    {
                        Id = Guid.Parse(reader.GetString(1)),
                        Url = reader.GetString(2),
                        Info = JsonSerializer.Deserialize<VideoInfo>(reader.GetString(3), App.JsonSerializerOptions)!,
                        State = (DownloadState)reader.GetInt32(4),
                        Progress = reader.GetInt32(5),
                        SelectedFormat = JsonSerializer.Deserialize<SelectedFormat>(reader.GetString(6), App.JsonSerializerOptions)!,
                        SelectedFormats = JsonSerializer.Deserialize<SelectedFormat[]>(reader.GetString(7), App.JsonSerializerOptions)!,
                        FilePath = reader.GetString(8),
                        Meta = JsonSerializer.Deserialize<DownloadItemMeta>(reader.GetString(9), App.JsonSerializerOptions)!
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("There is an error while retrieving download items: " + ex.Message);
            }
            finally
            {
                _semaphore.Release();
            }

            return list;
        }

        private async Task RunMigrationsAsync(SqliteConnection connection)
        {
            await _migrationLock.WaitAsync();
            try
            {
                var create = connection.CreateCommand();
                create.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Downloads (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        GuidId TEXT NOT NULL,
                        Url TEXT NOT NULL,
                        InfoJson TEXT NOT NULL,
                        State INTEGER NOT NULL,
                        Progress INTEGER NOT NULL,
                        SelectedFormatJson TEXT NOT NULL,
                        SelectedFormatsJson TEXT NOT NULL,
                        FilePath TEXT NOT NULL,
                        Meta TEXT NOT NULL,
                        CreatedAt TEXT NOT NULL
                    );
                ";

                await create.ExecuteNonQueryAsync();

                var versionCmd = connection.CreateCommand();
                versionCmd.CommandText = "PRAGMA user_version;";
                var version = Convert.ToInt32(await versionCmd.ExecuteScalarAsync());

                // v2 Migration
                if (version < 2)
                {
                    if (!await ColumnExists(connection, "Downloads", "SelectedFormatsJson"))
                    {
                        await TryAddColumnAsync(connection, "ALTER TABLE Downloads ADD COLUMN SelectedFormatsJson TEXT NOT NULL DEFAULT '[]';");
                    }

                    var updateVersion = connection.CreateCommand();
                    updateVersion.CommandText = "PRAGMA user_version = 2;";
                    await updateVersion.ExecuteNonQueryAsync();

                    version = 2;
                }

                // v3 Migration
                if (version < 3)
                {
                    if (!await ColumnExists(connection, "Downloads", "Meta"))
                    {
                        await TryAddColumnAsync(connection, "ALTER TABLE Downloads ADD COLUMN Meta TEXT NOT NULL DEFAULT '{}';");
                    }

                    var updateVersion = connection.CreateCommand();
                    updateVersion.CommandText = "PRAGMA user_version = 3;";
                    await updateVersion.ExecuteNonQueryAsync();

                    version = 3;
                }
            }
            finally
            {
                _migrationLock.Release();
            }
        }

        private static async Task<bool> ColumnExists(SqliteConnection connection, string table, string column)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = $"PRAGMA table_info({table});";

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                if (reader.GetString(1).Equals(column, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static async Task TryAddColumnAsync(SqliteConnection connection, string alterSql)
        {
            try
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = alterSql;
                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 1 &&
                ex.Message.Contains("duplicate column name", StringComparison.OrdinalIgnoreCase))
            {
                // Kolon zaten var yok say
            }
        }

        public async Task DeleteByGuidIdAsync(string GuidId)
        {
            await _semaphore.WaitAsync();
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Downloads WHERE GuidId = $guidid;";
                command.Parameters.AddWithValue("$guidid", GuidId);

                await command.ExecuteNonQueryAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task ClearAllAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Downloads;";
                await command.ExecuteNonQueryAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
