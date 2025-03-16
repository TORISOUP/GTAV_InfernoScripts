﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
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

    public class ParupunteConfigAttribute : Attribute
    {
        public string DefaultStartMessage = "";
        public string DefaultSubMessage = "";
        public string DefaultEndMessage = "";

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
        private bool IsFinished = false;

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

        protected readonly ParupunteConfigElement Config;

        private SoundPlayer _soundPlayer;

        protected ParupunteScript(ParupunteCore core, ParupunteConfigElement element)
        {
            this.core = core;
            coroutineIds = new List<uint>();
            core.LogWrite(this.ToString());
            IsFinished = false;
            Random = new Random();

            Config = element;
        }

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
            ;
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


        /// <summary>
        /// 効果音のロード
        /// </summary>
        public void SetUpSound()
        {
            if (_soundPlayer != null) return;

            var filePaths = LoadWavFiles(@"scripts/InfernoSEs");
            var setupWav = filePaths.FirstOrDefault(x => x.Contains($"{GetClassName()}.wav"));
            if (setupWav != null)
            {
                _soundPlayer = new SoundPlayer(setupWav);
            }
        }

        private string[] LoadWavFiles(string targetPath)
        {
            if (!Directory.Exists(targetPath))
            {
                return Array.Empty<string>();
            }

            return Directory.GetFiles(targetPath).Where(x => Path.GetExtension(x) == ".wav").ToArray();
        }

        public void PlaySound()
        {
            _soundPlayer?.Play();
        }
        
        public virtual void StopSound()
        {
            _soundPlayer?.Stop();
        }
        
        public string GetClassName()
        {
            return this.GetType().Name;
        }
    }
}