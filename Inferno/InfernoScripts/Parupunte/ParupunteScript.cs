﻿using System;
using System.Reactive;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using GTA;

namespace Inferno.InfernoScripts.Parupunte
{
    /// <summary>
    /// デバッグ用Attribute
    /// </summary>
    public class ParupunteDebug : Attribute
    {
        //trueにするとそのParupunteScriptが優先される
        public bool IsDebug;

        //trueにすると除外される(IsDebugより優先度は高い）
        public bool IsIgnore;

        public ParupunteDebug(bool isDebug = false, bool isIgnore = false)
        {
            IsDebug = isDebug;
            IsIgnore = isIgnore;
        }
    }

    public class ParupunteIsono : Attribute
    {
        public string Command;

        public ParupunteIsono(string command = null)
        {
            Command = command;
        }
    }

    public class ParupunteConfigAttribute : Attribute
    {
        public string DefaultEndMessage = "";
        public string DefaultStartMessage = "";
        public string DefaultSubMessage = "";

        public ParupunteConfigAttribute(string defaultStartMessage = "",
            string defaultEndMessage = "",
            string defaultSubMessage = "")
        {
            DefaultStartMessage = defaultStartMessage;
            DefaultEndMessage = defaultEndMessage;
            DefaultSubMessage = defaultSubMessage;
        }
    }

    internal abstract class ParupunteScript
    {
        protected readonly ParupunteConfigElement Config;

        /// <summary>
        /// コア
        /// </summary>
        protected ParupunteCore core;
        
        /// <summary>
        /// 終了メッセージの表示時間[s]
        /// </summary>
        public float EndMessageDisplayTime = 2.0f;

        private bool IsFinished;
        private AsyncSubject<Unit> onFinishedSubject;
        private Subject<Unit> onUpdateSubject;
        protected readonly CancellationTokenSource _scriptCts = new();
        protected Random Random;

        /// <summary>
        /// 汎用カウンタ（終了時にCompletedになる）
        /// </summary>
        protected ReduceCounter ReduceCounter;

        protected CancellationToken ActiveCancellationToken => _scriptCts.Token;

        protected ParupunteScript(ParupunteCore core, ParupunteConfigElement element)
        {
            this.core = core;
            IsFinished = false;
            Random = new Random();
            Config = element;
        }

        /// <summary>
        /// 開始時に表示されるメインメッセージ
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// 画面右下に常に表示されるミニメッセージ
        /// </summary>
        public string SubName { get; protected set; }

        /// <summary>
        /// 終了時に表示されるメッセージ
        /// nullまたは空文字列なら表示しない
        /// </summary>
        public Func<string> EndMessage { get; protected set; }

        /// <summary>
        /// パルプンテの処理が実行中であるか
        /// </summary>
        public bool IsActive { get; private set; } = true;

        private object _gateUpdateAsync = new();

        protected IObservable<Unit> OnUpdateAsObservable
            => onUpdateSubject ??= new Subject<Unit>();

        protected IObservable<Unit> OnFinishedAsObservable
            => onFinishedSubject ??= new AsyncSubject<Unit>();

        /// <summary>
        /// OnSetUpのあとに呼ばれる
        /// </summary>
        public virtual void OnSetNames()
        {
            Name = Config.StartMessage;
            SubName = Config.SubMessage;
            EndMessage = () => Config.FinishMessage;
        }

        /// <summary>
        /// パルプンテの名前が出るより前に1回だけ実行される
        /// コンストラクタでの初期化の代わりにこっちを使う
        /// </summary>
        public virtual void OnSetUp()
        {
        }

        /// <summary>
        /// パルプンテの名前が出たあとに1回だけ実行される
        /// </summary>
        public abstract void OnStart();

        public void OnUpdateCore()
        {
            onUpdateSubject?.OnNext(Unit.Default);
            OnUpdate();
        }


        /// <summary>
        /// 毎フレーム実行される
        /// </summary>
        protected virtual void OnUpdate()
        {
        }
        
        public void OnFinishedCore()
        {
            if (IsFinished)
            {
                return;
            }

            IsActive = false;
            IsFinished = true;
            ReduceCounter?.Finish();
            ReduceCounter?.Dispose();
            _scriptCts.Cancel();
            _scriptCts.Dispose();
            onUpdateSubject?.OnCompleted();
            onUpdateSubject?.Dispose();
            OnFinished();
            onFinishedSubject?.OnNext(Unit.Default);
            onFinishedSubject?.OnCompleted();
            onFinishedSubject?.Dispose();

            var endMessage = EndMessage();
            if (!string.IsNullOrEmpty(endMessage))
            {
                core.DrawParupunteText(endMessage, EndMessageDisplayTime);
            }
        }

        /// <summary>
        /// パルプンテ終了時に実行される
        /// </summary>
        protected virtual void OnFinished()
        {
            ;
        }

        /// <summary>
        /// パルプンテ処理を終了する場合に呼び出す
        /// </summary>
        protected void ParupunteEnd()
        {
            IsActive = false;
        }
        
        protected ValueTask DelayAsync(TimeSpan timeSpan, CancellationToken ct = default)
        {
            return core.DelayAsync(timeSpan, ct);
        }

        protected ValueTask DelayFrameAsync(int frame, CancellationToken ct = default)
        {
            return core.DelayFrameAsync(frame, ct);
        }

        protected ValueTask YieldAsync(CancellationToken ct = default)
        {
            return core.YieldAsync(ct);
        }

        protected ValueTask DelayRandomFrameAsync(int min, int max, CancellationToken ct)
        {
            return core.DelayRandomFrameAsync(min, max, ct);
        }

        protected ValueTask DelayRandomSecondsAsync(float min, float max, CancellationToken ct)
        {
            return core.DelayRandomSecondsAsync(min, max, ct);
        }

        protected ValueTask DelaySecondsAsync(float seconds, CancellationToken ct)
        {
            return core.DelaySecondsAsync(seconds, ct);
        }

        protected ValueTask Delay100MsAsync(CancellationToken ct)
        {
            return core.DelaySecondsAsync(0.1f, ct);
        }

        /// <summary>
        /// ReduceCounterをProgressBarとして出す
        /// </summary>
        /// <param name="counter"></param>
        protected void AddProgressBar(ReduceCounter counter)
        {
            core.AddProgressBar(counter);
        }

        /// <summary>
        /// パルプンテが終了した時に自動的に開放してくれる
        /// </summary>
        protected void AutoReleaseOnParupunteEnd(Entity entity)
        {
            core.AutoReleaseOnParupunteEnd(entity);
        }

        /// <summary>
        /// ゲーム終了時に自動開放
        /// </summary>
        protected void AutoReleaseOnGameEnd(Entity entity)
        {
            core.AutoReleaseOnGameEnd(entity);
        }
    }
}