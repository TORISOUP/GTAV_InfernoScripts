using GTA;
using System.Linq;
using System.Reactive.Linq;
using System;
using System.Reactive;
using System.Reactive.Subjects;

using Inferno.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Inferno.InfernoScripts.Event;
using Inferno.InfernoScripts.InfernoCore.Coroutine;
using Reactive.Bindings;


namespace Inferno
{
    /// <summary>
    /// インフェルノスクリプトの基底
    /// </summary>
    public abstract class InfernoScript : Script
    {
        protected Random Random = new Random();

        private readonly ReactiveProperty<bool> _isActiveReactiveProperty = new ReactiveProperty<bool>(false);

        protected virtual string ConfigFileName => null;

        protected ulong FrameCount { get; private set; }

        private InfernoScheduler infernoScheduler;

        private InfernoSynchronizationContext _infernoSynchronizationContext;

        private InfernoSynchronizationContext InfernoSynchronizationContext
            => _infernoSynchronizationContext ??= new InfernoSynchronizationContext();

        protected IScheduler InfernoScriptScheduler
            => infernoScheduler ??= new InfernoScheduler();

        /// <summary>
        /// 設定ファイルをロードする
        /// </summary>
        protected T LoadConfig<T>() where T : InfernoConfig, new()
        {
            if (string.IsNullOrEmpty(ConfigFileName))
            {
                throw new Exception("設定ファイル名が設定されていません");
            }

            var loader = new InfernoConfigLoader<T>();
            var dto = loader.LoadSettingFile(ConfigFileName);
            //バリデーションに引っかかったらデフォルト値を返す
            return dto.Validate() ? dto : new T();
        }

        /// <summary>
        /// スクリプトが動作中であるか
        /// </summary>
        protected bool IsActive
        {
            get { return _isActiveReactiveProperty.Value; }
            set { _isActiveReactiveProperty.Value = value; }
        }

        /// <summary>
        /// IsActiveが変化したことを通知する
        /// </summary>
        protected IObservable<bool> IsActiveAsObservable =>
            _isActiveReactiveProperty.AsObservable().DistinctUntilChanged();

        #region Chace

        /// <summary>
        /// プレイヤのped
        /// </summary>
        public Ped PlayerPed => cahcedPlayerPed ?? Game.Player.Character;

        private Ped cahcedPlayerPed;
        public readonly ReactiveProperty<Vehicle> PlayerVehicle = new();


        /// <summary>
        /// キャッシュされたプレイヤ周辺の市民
        /// </summary>
        public Ped[] CachedPeds => InfernoCore.Instance.PedsNearPlayer.Value;

        /// <summary>
        /// キャッシュされたプレイヤ周辺の車両
        /// </summary>
        public Vehicle[] CachedVehicles => InfernoCore.Instance.VehicleNearPlayer.Value;

        #endregion Chace

        #region forEvents

        /// <summary>
        /// 100ms間隔のTickイベント
        /// </summary>
        public IObservable<Unit> OnThinnedTickAsObservable { get; private set; }

        /// <summary>
        /// たぶん16msごとに実行されるイベント
        /// </summary>
        public IObservable<Unit> OnTickAsObservable { get; private set; }

        /// <summary>
        /// 描画用のTickイベント
        /// </summary>
        public IObservable<Unit> OnDrawingTickAsObservable { get; private set; }

        private IObservable<KeyEventArgs> _onKeyDownAsObservable;

        public IObservable<KeyEventArgs> OnKeyDownAsObservable
        {
            get
            {
                if (_onKeyDownAsObservable != null) return _onKeyDownAsObservable;
                _onKeyDownAsObservable =
                    Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(h => h.Invoke, h => KeyDown += h,
                            h => KeyDown -= h)
                        .Select(e => e.EventArgs)
                        .Publish()
                        .RefCount();
                return _onKeyDownAsObservable;
            }
        }

        public IObservable<Unit> OnAllOnCommandObservable { get; private set; }

        /// <summary>
        /// InfernoEvent
        /// </summary>
        protected IObservable<IEventMessage> OnRecievedInfernoEvent
            => InfernoCore.OnRecievedEventMessage.ObserveOn(InfernoScriptScheduler);

        /// <summary>
        /// 入力文字列に応じて反応するIObservableを生成する
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        protected IObservable<Unit> CreateInputKeywordAsObservable(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                throw new Exception("Keyword is empty.");
            }

            return OnKeyDownAsObservable
                .Select(e => e.KeyCode.ToString())
                .Buffer(keyword.Length, 1)
                .Select(x => x.Aggregate((p, c) => p + c))
                .Where(x => x == keyword.ToUpper()) //入力文字列を比較
                .Select(_ => Unit.Default)
                .Take(1)
                .Repeat() //1回動作したらBufferをクリア
                .Publish()
                .RefCount();
        }

        /// <summary>
        /// 任意のTickObservableを生成する
        /// <returns></returns>
        protected IObservable<Unit> CreateTickAsObservable(TimeSpan timeSpan)
        {
            return OnTickAsObservable.ThrottleFirst(timeSpan, InfernoScriptScheduler).Publish().RefCount();
        }

        #endregion forEvents

        #region forAbort

        /// <summary>
        /// ゲーム中断時に自動開放する対象リスト
        /// </summary>
        private List<Entity> _autoReleaseEntities = new List<Entity>();

        /// <summary>
        /// ゲーム中断時に自動開放する
        /// </summary>
        /// <param name="entity"></param>
        protected void AutoReleaseOnGameEnd(Entity entity)
        {
            _autoReleaseEntities.RemoveAll(x => !x.IsSafeExist());
            _autoReleaseEntities.Add(entity);
        }

        private IObservable<Unit> _onAbortObservable;

        /// <summary>
        /// ゲームが中断した時に実行される
        /// </summary>
        protected IObservable<Unit> OnAbortAsync
        {
            get
            {
                return _onAbortObservable ??= Observable.FromEventPattern<EventHandler, EventArgs>(h => h.Invoke,
                        h => Aborted += h,
                        h => Aborted -= h)
                    .Select(_ => Unit.Default);
            }
        }

        #endregion

        #region forCoroutine

        private CoroutinePool _coroutinePool;

        protected uint StartCoroutine(IEnumerable<Object> coroutine)
        {
            return _coroutinePool.RegisterCoroutine(coroutine);
        }

        protected void StopCoroutine(uint id)
        {
            _coroutinePool.RemoveCoroutine(id);
        }

        protected void StopAllCoroutine()
        {
            _coroutinePool.RemoveAllCoroutine();
        }


        /// <summary>
        /// 指定秒数待機するIEnumerable
        /// </summary>
        /// <param name="secound">秒</param>
        /// <returns></returns>
        protected IEnumerable WaitForSeconds(float secound)
        {
            return Awaitable.ToCoroutine(TimeSpan.FromSeconds(secound));
        }

        /// <summary>
        /// 0-10回待機する
        /// </summary>
        /// <returns></returns>
        protected IEnumerable RandomWait()
        {
            var waitLoopCount = Random.Next(0, 10);
            for (var i = 0; i < waitLoopCount; i++)
            {
                yield return i;
            }
        }

        #endregion forCoroutine

        #region forTaks

        protected async Task DelayFrame(int frame, CancellationToken ct = default)
        {
            await OnTickAsObservable
                .Take(frame)
                .ToTask(ct);
        }

        #endregion

        #region forDraw

        /// <summary>
        /// テキスト表示
        /// </summary>
        /// <param name="text">表示したい文字列</param>
        /// <param name="time">時間[s]</param>
        public void DrawText(string text, float time = 3.0f)
        {
            ToastTextDrawing.Instance.DrawDebugText(text, time);
        }

        /// <summary>
        /// ProgressBarを描画登録する
        /// </summary>
        public void RegisterProgressBar(ProgressBarData data)
        {
            ProgressBarDrawing.Instance.RegisterProgressBar(data);
        }

        #endregion forDraw

        #region forTimer

        private List<ICounter> _counterList = new List<ICounter>();

        /// <summary>
        /// カウンタを登録して自動カウントさせる
        /// カウンタのUpdateにはIntervalの数値が渡される
        /// </summary>
        public void RegisterCounter(ICounter counter)
        {
            _counterList.Add(counter);
        }

        #endregion forTimer

        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected InfernoScript()
        {
            // 毎フレーム実行
            Interval = 0;

            //初期化をちょっと遅延させる
            Observable.Interval(TimeSpan.FromMilliseconds(10))
                .Where(_ => InfernoCore.Instance != null)
                .Take(1)
                .Subscribe(_ =>
                {
                    InfernoCore.Instance.PlayerPed.Subscribe(x => cahcedPlayerPed = x);
                    InfernoCore.Instance.PlayerVehicle.Subscribe(x => PlayerVehicle.Value = x);
                });

            OnTickAsObservable =
                Observable.FromEventPattern<EventHandler, EventArgs>(h => h.Invoke, h => Tick += h, h => Tick -= h)
                    .Select(_ => Unit.Default)
                    .Publish().RefCount(); //Subscribeされたらイベントハンドラを登録する

            OnThinnedTickAsObservable =
                OnTickAsObservable.ThrottleFirst(TimeSpan.FromMilliseconds(100), InfernoScriptScheduler)
                    .Publish().RefCount();

            OnDrawingTickAsObservable = DrawingCore.OnDrawingTickAsObservable;

            OnAllOnCommandObservable = CreateInputKeywordAsObservable("allon");

            //スケジューラ実行
            OnTickAsObservable.Subscribe(_ =>
            {
                FrameCount++;


                try
                {
                    infernoScheduler?.Run();
                }
                catch
                {
                    // ignore
                }

                try
                {
                    InfernoSynchronizationContext.Update();
                }
                catch
                {
                    // ignore
                }
            });

            //タイマのカウント
            OnThinnedTickAsObservable
                .Where(_ => _counterList.Any())
                .Subscribe(_ =>
                {
                    foreach (var c in _counterList)
                    {
                        c.Update(100);
                    }

                    //完了状態にあるタイマを全て削除
                    _counterList.RemoveAll(x => x.IsCompleted);
                });

            _coroutinePool = new CoroutinePool(5);

            //コルーチンを実行する
            CreateTickAsObservable(TimeSpan.FromMilliseconds(_coroutinePool.ExpectExecutionInterbalMillSeconds))
                .Subscribe(_ => _coroutinePool.Run());


            OnAbortAsync.Subscribe(_ =>
            {
                IsActive = false;
                foreach (var e in _autoReleaseEntities.Where(x => x.IsSafeExist()))
                {
                    e.MarkAsNoLongerNeeded();
                }

                _autoReleaseEntities.Clear();
            });

            // Setupは最初のTickタイミングまで遅らせる
            // OnTickAsObservableとは独立させて実行しないとイベント登録順で
            // 怪しい挙動をする可能性がある
            Observable
                .FromEventPattern<EventHandler, EventArgs>(h => h.Invoke, h => Tick += h, h => Tick -= h)
                .Select(_ => Unit.Default)
                .Take(1)
                .Subscribe(_ =>
                {
                    try
                    {
                        // SynchronizationContextはこのタイミングで設定しないといけない
                        SynchronizationContext.SetSynchronizationContext(InfernoSynchronizationContext);
                        Setup();
                    }
                    catch (Exception e)
                    {
                        LogWrite(e.ToString());
                    }
                });
        }

        /// <summary>
        /// 初期化処理はここに書く
        /// </summary>
        protected abstract void Setup();

        #region Debug

        /// <summary>
        /// ログを吐く
        /// </summary>
        /// <param name="message">ログメッセージ</param>
        public void LogWrite(string message, bool stackTrace = false)
        {
            InfernoCore.Instance.LogWrite(message + "\n");
            if (stackTrace)
            {
                InfernoCore.Instance.LogWrite(Environment.StackTrace + "\n");
            }
        }

        #endregion Debug
    }
}