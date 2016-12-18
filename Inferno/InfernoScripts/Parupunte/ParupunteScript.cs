using System;
using System.Collections;
using System.Collections.Generic;
using GTA;
using Inferno.InfernoScripts.Event;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte
{
    /// <summary>
    ///　デバッグ用Attribute
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
    
    internal abstract class ParupunteScript
    {
        private bool IsFinished = false;

        /// <summary>
        /// 開始時に表示されるメインメッセージ
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// 画面右下に常に表示されるミニメッセージ
        /// </summary>
        public virtual string SubName { get; }

        /// <summary>
        /// 終了時に表示されるメッセージ
        /// nullまたは空文字列なら表示しない
        /// </summary>
        public virtual string EndMessage { get; }

        /// <summary>
        /// 終了メッセージの表示時間[s]
        /// </summary>
        public float EndMessageDisplayTime = 2.0f;

        /// <summary>
        /// パルプンテの処理が実行中であるか
        /// </summary>
        public bool IsActive { get; private set; } = true;

        /// <summary>
        /// コア
        /// </summary>
        protected ParupunteCore core;

        /// <summary>
        /// このパルプンテスクリプト中で使用しているcoroutine一覧
        /// </summary>
        protected List<uint> coroutineIds;

        private Subject<Unit> onUpdateSubject;
        private Subject<Unit> onFinishedSubject;

        /// <summary>
        /// 汎用カウンタ（終了時にCompletedになる）
        /// </summary>
        protected ReduceCounter ReduceCounter;

        protected Random Random;

        protected ParupunteScript(ParupunteCore core)
        {
            this.core = core;
            coroutineIds = new List<uint>();
            core.LogWrite(this.ToString());
            IsFinished = false;
            Random = new Random();
        }

        /// <summary>
        /// パルプンテの名前が出るより前に1回だけ実行される
        /// コンストラクタでの初期化の代わりにこっちを使う
        /// </summary>
        public virtual void OnSetUp() {; }

        /// <summary>
        /// パルプンテの名前が出たあとに1回だけ実行される
        /// </summary>
        public abstract void OnStart();

        public void OnUpdateCore()
        {
            onUpdateSubject?.OnNext(Unit.Default);
            OnUpdate();
        }

        protected UniRx.IObservable<Unit> OnUpdateAsObservable
            => onUpdateSubject ?? (onUpdateSubject = new Subject<Unit>());

        protected UniRx.IObservable<Unit> OnFinishedAsObservable
            => onFinishedSubject ?? (onFinishedSubject = new Subject<Unit>());

        /// <summary>
        /// 100msごとに実行される
        /// </summary>
        protected virtual void OnUpdate()
        {
            ;
        }

        public void OnFinishedCore()
        {
            if (IsFinished) return;

            IsFinished = true;
            ReduceCounter?.Finish();

            onUpdateSubject?.OnCompleted();
            OnFinished();
            onFinishedSubject?.OnNext(Unit.Default);
            onFinishedSubject?.OnCompleted();

            foreach (var id in coroutineIds)
            {
                StopCoroutine(id);
            }
            coroutineIds.Clear();

            if (!string.IsNullOrEmpty(EndMessage))
            {
                core.DrawParupunteText(EndMessage, EndMessageDisplayTime);
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

        /// <summary>
        /// コルーチンを実行する（処理自体はParupunteCoreが行う）
        /// </summary>
        protected uint StartCoroutine(IEnumerable<object> coroutine)
        {
            var id = core.RegisterCoroutine(coroutine);
            coroutineIds.Add(id);
            return id;
        }

        /// <summary>
        /// コルーチンを終了する
        /// </summary>
        protected void StopCoroutine(uint id)
        {
            core.UnregisterCoroutine(id);
        }

        /// <summary>
        /// 指定秒数待機するIEnumerable
        /// </summary>
        /// <param name="seconds"></param>
        protected IEnumerable WaitForSeconds(float seconds)
        {
            return core.CreateWaitForSeconds(seconds);
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
