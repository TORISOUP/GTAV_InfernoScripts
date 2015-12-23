using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Subjects;

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
        public ParupunteDebug(bool isDebug = false,bool isIgnore = false)
        {
            IsDebug = isDebug;
            IsIgnore = isIgnore;
        }
    }

    abstract class ParupunteScript
    {
        private bool IsFinished = false;

        public abstract string Name { get; }

        /// <summary>
        /// 終了時に表示されるメッセージ
        /// nullまたは空文字列なら表示しない
        /// </summary>
        public virtual string EndMessage { get; }

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

        /// <summary>
        /// 汎用カウンタ（終了時にCompletedになる）
        /// </summary>
        protected ReduceCounter ReduceCounter;

        protected ParupunteScript(ParupunteCore core)
        {
            this.core = core;
            coroutineIds = new List<uint>();
            core.LogWrite(this.ToString());
        }

        /// <summary>
        /// パルプンテの名前が出るより前に1回だけ実行される
        /// コンストラクタでの初期化の代わりにこっちを使う
        /// </summary>
        public virtual void OnSetUp() {;}

        /// <summary>
        /// パルプンテの名前が出たあとに1回だけ実行される
        /// </summary>
        public abstract void OnStart();

        public void OnUpdateCore()
        {
            onUpdateSubject?.OnNext(Unit.Default);
            OnUpdate();
        }

        protected IObservable<Unit> UpdateAsObservable => onUpdateSubject ?? (onUpdateSubject = new Subject<Unit>());

        /// <summary>
        /// 100msごとに実行される
        /// </summary>
        protected virtual void OnUpdate()
        {
            ;
        }


        public void OnFinishedCore()
        {
            if(IsFinished) return;
            IsFinished = true;
            ReduceCounter?.Finish();
            onUpdateSubject?.OnCompleted();
            OnFinished();

            foreach (var id in coroutineIds)
            {
                StopCoroutine(id);
            }
            coroutineIds.Clear();

            if (!string.IsNullOrEmpty(EndMessage))
            {
                core.DrawParupunteText(EndMessage, 2.0f);
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
    }
}
