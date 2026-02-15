using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace LechYTDLP.Services
{
    public enum LogKey
    {
        Download
    }

    public enum LogTag
    {
        // ofc i had to add this one <3
        Lechixy,
        // Tags
        Normal,
        LechYTDLP,
        Warning,
        Error,
        ApiServer
    }

    public class LogItem : INotifyPropertyChanged
    {
        public Guid Id { get; } = Guid.NewGuid();

        private string _message = string.Empty;
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        public LogTag Tag { get; set; }

        public DateTime Time { get; } = DateTime.Now;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public static class LogService
    {
        private static readonly object _lock = new();

        private static readonly List<LogItem> _logs = new();
        private static readonly Dictionary<LogKey, LogItem> _keyedLogs = new();

        public static event Action<LogItem>? LogAdded;
        public static event Action<LogItem>? LogUpdated;

        public static event Action<int, string>? BadgeChanged;

        public static int LogCount { get; private set; }

        public static IReadOnlyList<LogItem> GetAll()
        {
            lock (_lock)
                return _logs.ToList();
        }

        public static void Add(string text, LogTag tag = LogTag.Normal)
        {
            var item = new LogItem
            {
                Message = text,
                Tag = tag
            };

            lock (_lock)
            {
                _logs.Add(item);
                IncrementBadgeInternal();
            }

            LogAdded?.Invoke(item);
        }

        public static void AddOrUpdate(LogKey key, string text, LogTag tag = LogTag.Normal)
        {
            lock (_lock)
            {
                if (_keyedLogs.TryGetValue(key, out var existing))
                {
                    existing.Message = text;
                    existing.Tag = tag;

                    LogUpdated?.Invoke(existing);
                }
                else
                {
                    var item = new LogItem
                    {
                        Message = text,
                        Tag = tag
                    };

                    _logs.Add(item);
                    _keyedLogs[key] = item;

                    IncrementBadgeInternal();
                    LogAdded?.Invoke(item);
                }
            }
        }

        private static void IncrementBadgeInternal()
        {
            LogCount++;
            BadgeChanged?.Invoke(LogCount, "Log");
        }

        public static void DecrementLog()
        {
            lock (_lock)
            {
                if (LogCount > 0)
                {
                    LogCount--;
                    BadgeChanged?.Invoke(LogCount, "Log");
                }
            }
        }

        public static void ResetLog()
        {
            lock (_lock)
            {
                LogCount = 0;
                BadgeChanged?.Invoke(LogCount, "Log");
            }
        }
    }

}
