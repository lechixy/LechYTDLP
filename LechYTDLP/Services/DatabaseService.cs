using LechYTDLP.Classes;
using LechYTDLP.Components;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
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
            string dbPath = Path.Combine(
                ApplicationData.Current.LocalFolder.Path,
                "history.db");

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
                (Url, InfoJson, State, Progress, SelectedFormatJson, FilePath, CreatedAt)
                VALUES
                ($url, $info, $state, $progress, $format, $filePath, $createdAt);
            ";

                command.Parameters.AddWithValue("$url", item.Url);
                command.Parameters.AddWithValue("$info", JsonSerializer.Serialize(item.Info));
                command.Parameters.AddWithValue("$state", (int)item.State);
                command.Parameters.AddWithValue("$progress", item.Progress);
                command.Parameters.AddWithValue("$format", JsonSerializer.Serialize(item.SelectedFormat));
                command.Parameters.AddWithValue("$filePath", item.FilePath);
                command.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("o"));

                await command.ExecuteNonQueryAsync();
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
                    "SELECT Url, InfoJson, State, Progress, SelectedFormatJson, FilePath FROM Downloads ORDER BY Id DESC;";

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    list.Add(new DownloadItem
                    {
                        Url = reader.GetString(0),
                        Info = JsonSerializer.Deserialize<VideoInfo>(reader.GetString(1))!,
                        State = (DownloadState)reader.GetInt32(2),
                        Progress = reader.GetInt32(3),
                        SelectedFormat = JsonSerializer.Deserialize<SelectedFormat>(reader.GetString(4))!,
                        FilePath = reader.GetString(5)
                    });
                }
            }
            finally
            {
                _semaphore.Release();
            }

            return list;
        }

        public async Task DeleteByUrlAsync(string url)
        {
            await _semaphore.WaitAsync();
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Downloads WHERE Url = $url;";
                command.Parameters.AddWithValue("$url", url);

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
