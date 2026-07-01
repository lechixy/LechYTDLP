using LechYTDLP.Classes;
using LechYTDLP.Components;
using LechYTDLP.Util;
using Microsoft.Data.Sqlite;
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

        public DatabaseService()
        {
            string dbPath = Path.Combine(LechKnownFolders.GetPath(LechKnownFolder.Documents), "LechYTDLP\\Database\\history.db");
            string oldDbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "history.db");

            try
            {
                if (!File.Exists(dbPath))
                {
                    // We check if the old database exists and copy it to the new location
                    // Start with v1.6.0, we moved the database to the Documents folder for better accessibility.
                    if (File.Exists(oldDbPath))
                    {
                        File.Copy(oldDbPath, dbPath, true);
                        File.Delete(oldDbPath);
                    }

                    Debug.WriteLine("Database file not found. Creating a new one.");
                    Directory.CreateDirectory(Path.GetDirectoryName(dbPath) ?? Path.Combine(LechKnownFolders.GetPath(LechKnownFolder.Documents), "LechYTDLP\\Database"));
                    File.Create(dbPath).Close();
                }
            } catch (Exception ex)
            {
                Debug.WriteLine($"Error while checking/creating database file: {ex.Message}");
            }

            _connectionString = $"Data Source={dbPath}";
        }

        public async Task InitializeAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText =
                @"
                CREATE TABLE IF NOT EXISTS Downloads (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    GuidId TEXT NOT NULL,
                    Url TEXT NOT NULL,
                    InfoJson TEXT NOT NULL,
                    State INTEGER NOT NULL,
                    Progress INTEGER NOT NULL,
                    SelectedFormatJson TEXT NOT NULL,
                    FilePath TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL
                );
            ";

                await command.ExecuteNonQueryAsync();
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
                (GuidId, Url, InfoJson, State, Progress, SelectedFormatJson, FilePath, CreatedAt)
                VALUES
                ($guidid, $url, $info, $state, $progress, $format, $filePath, $createdAt);
            ";

                command.Parameters.AddWithValue("$guidid", item.Id.ToString());
                command.Parameters.AddWithValue("$url", item.Url);
                command.Parameters.AddWithValue("$info", JsonSerializer.Serialize(item.Info, App.JsonSerializerOptions));
                command.Parameters.AddWithValue("$state", (int)item.State);
                command.Parameters.AddWithValue("$progress", item.Progress);
                command.Parameters.AddWithValue("$format", JsonSerializer.Serialize(item.SelectedFormat, App.JsonSerializerOptions));
                command.Parameters.AddWithValue("$filePath", item.FilePath);
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
                    "SELECT Id, GuidId, Url, InfoJson, State, Progress, SelectedFormatJson, FilePath FROM Downloads ORDER BY Id ASC;";

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
                        FilePath = reader.GetString(7)
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
