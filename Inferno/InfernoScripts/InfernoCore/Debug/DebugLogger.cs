﻿using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Inferno.Utilities;

namespace Inferno
{
    public interface IDebugLogger : IDisposable
    {
        public void Log(string message);
    }

    public class DebugLogger : IDebugLogger
    {
        private readonly Encoding _encoding;
        private readonly string _logPath;
        private readonly SemaphoreSlim _semaphore = new(1);

        public static DebugLogger Instance { get; } = new DebugLogger(@"Inferno.log");

        private DebugLogger(string logPath)
        {
            _logPath = logPath;
            _encoding = Encoding.GetEncoding("UTF-8");
        }

        public void Dispose()
        {
            try
            {
                _semaphore?.Dispose();
            }
            catch
            {
                //
            }
        }

        /// <summary>
        /// テキストに書き出す
        /// </summary>
        /// <param name="message"></param>
        private void WriteToText(string message)
        {
            WriteToTextAsync(message).Forget();
        }

        private async ValueTask WriteToTextAsync(string message)
        {
            try
            {
                await _semaphore.WaitAsync();
                using var writer = new StreamWriter(_logPath, true, _encoding);
                await writer.WriteLineAsync(message).ConfigureAwait(false);
                await writer.FlushAsync();
            }
            finally
            {
                try
                {
                    _semaphore.Release(1);
                }
                catch
                {
                    //
                }
            }
        }

        public void Log(string message)
        {
            var sendMessage = $"[{DateTime.Now}] {message}";
            WriteToText(sendMessage);
        }
    }
}