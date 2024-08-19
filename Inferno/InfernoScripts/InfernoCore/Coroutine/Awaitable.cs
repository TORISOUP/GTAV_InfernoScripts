using System;
using System.Collections;
using System.Diagnostics;
using System.Reactive.Concurrency;

namespace Inferno.InfernoScripts.InfernoCore.Coroutine
{
    public static class Awaitable
    {
        private static readonly Stopwatch stopWatch = Stopwatch.StartNew();
        private static TimeSpan Time => stopWatch.Elapsed;

        /// <summary>
        /// 指定時間待機するコルーチンを生成する
        /// </summary>
        public static IEnumerable ToCoroutine(TimeSpan timeSpan)
        {
            var dt = Time + Scheduler.Normalize(timeSpan);

            while ((dt - Time).Ticks > 0) yield return null;
        }
    }
}