using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA;
using Inferno.InfernoScripts.Event;
using Inferno.InfernoScripts.InfernoCore.UI;
using Inferno.Utilities;
using Inferno.Utilities.Awaiters;
using LemonUI;
using LemonUI.Menus;
using Reactive.Bindings;

namespace Inferno
{
    /// <summary>
    /// インフェルノスクリプトの基底
    /// </summary>
    public abstract class InfernoScript : Script, IScriptUiBuilder
    {
        private readonly AsyncSubject<Unit> _disposeSubject = new();
        private readonly ReactiveProperty<bool> _isActiveReactiveProperty = new(false);

        private readonly List<StepAwaiter> _stepAwaiters = new(8);
        private readonly List<TimeAwaiter> _timeAwaiters = new(8);
        protected readonly Random Random = new();
        private CancellationTokenSource _activationCancellationTokenSource;
        private readonly CancellationTokenSource _destroyCancellationTokenSource = new();
        private CancellationTokenSource _linkedCancellationTokenSource;

        private InfernoSynchronizationContext _infernoSynchronizationContext;
        private InfernoScheduler infernoScheduler;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected InfernoScript()
        {
            // 毎フレーム実行
            Interval = 0;

            // StepAwaiterの初期化
            for (var i = 0; i < 4; i++)
            {
                _stepAwaiters.Add(new StepAwaiter());
            }

            // TimeAwaiterの初期化
            for (var i = 0; i < 4; i++)
            {
                _timeAwaiters.Add(new TimeAwaiter());
            }

            //初期化をちょっと遅延させる
            Observable.Interval(TimeSpan.FromMilliseconds(10))
                .Where(_ => InfernoCore.Instance != null)
                .Take(1)
                .TakeUntil(_disposeSubject)
                .Subscribe(_ =>
                {
                    InfernoCore.Instance.PlayerPed.Subscribe(x => cahcedPlayerPed = x);
                    InfernoCore.Instance.PlayerVehicle.Subscribe(x => PlayerVehicle.Value = x);
                })
                .AddTo(CompositeDisposable);

            OnTickAsObservable =
                Observable.FromEventPattern<EventHandler, EventArgs>(h => h.Invoke, h => Tick += h, h => Tick -= h)
                    .Select(_ => Unit.Default)
                    .TakeUntil(_disposeSubject)
                    .Publish()
                    .RefCount(); //Subscribeされたらイベントハンドラを登録する

            OnThinnedTickAsObservable =
                OnTickAsObservable.ThrottleFirst(TimeSpan.FromMilliseconds(100), InfernoScheduler)
                    .Publish()
                    .RefCount();

            OnDrawingTickAsObservable = DrawingCore.OnDrawingTickAsObservable;

            OnAllOnCommandObservable = CreateInputKeywordAsObservable("allon");

            //スケジューラなどの定期実行系
            OnTickAsObservable.Subscribe(_ =>
                {
                    FrameCount++;
                    DeltaTime = Game.LastFrameTime;
                    ElapsedTime += DeltaTime;

                    try
                    {
                        if (_counterList.Any())
                        {
                            foreach (var c in _counterList)
                            {
                                c.Update((int)(DeltaTime * 1000));
                            }

                            //完了状態にあるタイマを全て削除
                            _counterList.RemoveAll(x => x.IsCompleted);
                        }
                    }
                    catch
                    {
                        //
                    }


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

                    try
                    {
                        lock (_stepAwaiters)
                        {
                            foreach (var stepAwaiter in _stepAwaiters)
                            {
                                if (stepAwaiter is { IsActive: true })
                                {
                                    stepAwaiter.Step();
                                }
                            }

                            foreach (var stepAwaiter in _stepAwaiters)
                            {
                                if (stepAwaiter is { IsActive: true })
                                {
                                    stepAwaiter.Check();
                                }
                            }
                        }
                    }
                    catch
                    {
                        // ignore
                    }

                    try
                    {
                        lock (_timeAwaiters)
                        {
                            foreach (var timeAwaiter in _timeAwaiters)
                            {
                                if (timeAwaiter is { IsActive: true })
                                {
                                    timeAwaiter.Step(DeltaTime);
                                }
                            }

                            foreach (var timeAwaiter in _timeAwaiters)
                            {
                                if (timeAwaiter is { IsActive: true })
                                {
                                    timeAwaiter.Check();
                                }
                            }
                        }
                    }
                    catch
                    {
                        // ignore
                    }
                })
                .AddTo(CompositeDisposable);

            OnAbortAsync.Subscribe(_ =>
                {
                    Destroy();
                    IsActive = false;
                    foreach (var e in _autoReleaseEntities.Where(x => x.IsSafeExist()))
                    {
                        e.MarkAsNoLongerNeeded();
                    }

                    _autoReleaseEntities.Clear();
                })
                .AddTo(CompositeDisposable);
            ;

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

                    try
                    {
                        // UIの構築
                        if (UseUI)
                        {
                            InfernoUi.Instance.RegisterInfernoBuilder(this);
                        }
                    }
                    catch (Exception e)
                    {
                        LogWrite($"[{this.GetType()}] " + e.ToString());
                    }
                })
                .AddTo(CompositeDisposable);
        }

        protected CompositeDisposable CompositeDisposable { get; } = new();

        protected virtual string ConfigFileName => null;

        public ulong FrameCount { get; private set; }
        public float ElapsedTime { get; private set; }

        public float DeltaTime { get; private set; }

        protected InfernoSynchronizationContext InfernoSynchronizationContext
            => _infernoSynchronizationContext ??= new InfernoSynchronizationContext();

        public IScheduler InfernoScheduler
            => infernoScheduler ??= new InfernoScheduler();


        /// <summary>
        /// スクリプトが動作中であるか
        /// </summary>
        protected bool IsActive
        {
            get => _isActiveReactiveProperty.Value;
            set
            {
                if (!value)
                {
                    try
                    {
                        _linkedCancellationTokenSource?.Cancel();
                        _linkedCancellationTokenSource?.Dispose();
                        _linkedCancellationTokenSource = null;
                        _activationCancellationTokenSource?.Cancel();
                        _activationCancellationTokenSource?.Dispose();
                        _activationCancellationTokenSource = null;
                    }
                    catch
                    {
                        // ignore
                    }
                }

                _isActiveReactiveProperty.Value = value;
            }
        }


        /// <summary>
        /// IsActiveが変化したことを通知する
        /// </summary>
        public IReadOnlyReactiveProperty<bool> IsActiveRP => _isActiveReactiveProperty;

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
        /// 初期化処理はここに書く
        /// </summary>
        protected abstract void Setup();

        protected virtual void Destroy()
        {
            try
            {
                CompositeDisposable.Dispose();
                _disposeSubject.OnNext(Unit.Default);
                _disposeSubject.OnCompleted();
                _disposeSubject.Dispose();
                _destroyCancellationTokenSource.Cancel();
                _destroyCancellationTokenSource.Dispose();
                _activationCancellationTokenSource?.Cancel();
                _activationCancellationTokenSource?.Dispose();
                _activationCancellationTokenSource = null;
                _isActiveReactiveProperty.Dispose();
                lock (_stepAwaiters)
                {
                    foreach (var stepAwaiter in _stepAwaiters)
                    {
                        stepAwaiter?.Dispose();
                    }

                    _stepAwaiters.Clear();
                }

                lock (_timeAwaiters)
                {
                    foreach (var timeAwaiter in _timeAwaiters)
                    {
                        timeAwaiter?.Dispose();
                    }

                    _timeAwaiters.Clear();
                }
            }
            catch
            {
                //
            }
        }

        #region forTaks

        protected async ValueTask DelaySecondsAsync(float seconds, CancellationToken ct = default)
        {
            await DelayAsync(TimeSpan.FromSeconds(seconds), ct);
        }

        protected async ValueTask DelayAsync(TimeSpan timeSpan, CancellationToken ct = default)
        {
            TimeAwaiter timeAwaiter;
            lock (_timeAwaiters)
            {
                // 使用可能なStepAwaiterを探す
                timeAwaiter = _timeAwaiters.FirstOrDefault(x => !x.IsActive);
                // 存在しなければ新規作成
                if (timeAwaiter == null)
                {
                    timeAwaiter = new TimeAwaiter();
                    _timeAwaiters.Add(timeAwaiter);
                }

                timeAwaiter.Reset(timeSpan, ct);
            }

            try
            {
                await timeAwaiter;
                ct.ThrowIfCancellationRequested();
            }
            finally
            {
                timeAwaiter.Release();
            }
        }

        protected async ValueTask DelayFrameAsync(int frame, CancellationToken ct = default)
        {
            StepAwaiter stepAwaiter;
            lock (_stepAwaiters)
            {
                // 使用可能なStepAwaiterを探す
                stepAwaiter = _stepAwaiters.FirstOrDefault(x => !x.IsActive);
                // 存在しなければ新規作成
                if (stepAwaiter == null)
                {
                    stepAwaiter = new StepAwaiter();
                    _stepAwaiters.Add(stepAwaiter);
                }

                stepAwaiter.Reset(frame, ct);
            }

            try
            {
                await stepAwaiter;
                ct.ThrowIfCancellationRequested();
            }
            finally
            {
                stepAwaiter.Release();
            }
        }

        protected ValueTask YieldAsync(CancellationToken ct = default)
        {
            return DelayFrameAsync(1, ct);
        }

        protected ValueTask DelayRandomFrameAsync(int min, int max, CancellationToken ct)
        {
            var waitLoopCount = Random.Next(min, max);
            return DelayFrameAsync(waitLoopCount, ct);
        }

        protected ValueTask DelayRandomSecondsAsync(float min, float max, CancellationToken ct)
        {
            var waitSeconds = Random.NextDouble() * (max - min) + min;
            return DelayAsync(TimeSpan.FromSeconds(waitSeconds), ct);
        }


        protected CancellationToken ActivationCancellationToken
        {
            get
            {
                if (!IsActive)
                {
                    throw new Exception("Script is not active.");
                }

                if (_linkedCancellationTokenSource != null)
                {
                    return _linkedCancellationTokenSource.Token;
                }

                _activationCancellationTokenSource ??= new CancellationTokenSource();

                var at = _activationCancellationTokenSource.Token;
                _linkedCancellationTokenSource =
                    CancellationTokenSource.CreateLinkedTokenSource(at, _destroyCancellationTokenSource.Token);

                return _linkedCancellationTokenSource.Token;
            }
        }

        protected CancellationToken DestroyCancellationToken => _destroyCancellationTokenSource.Token;

        #endregion

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

        public void LogWrite(object message, bool stackTrace = false)
        {
            InfernoCore.Instance.LogWrite(message + "\n");
            if (stackTrace)
            {
                InfernoCore.Instance.LogWrite(Environment.StackTrace + "\n");
            }
        }

        #endregion Debug

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
        public Vehicle[] CachedVehicles => InfernoCore.Instance.VehiclesNearPlayer.Value;

        public Entity[] CachedEntities => InfernoCore.Instance.EntitiesNearPlayer.Value;

        /// <summary>
        /// Not thread safe.
        /// </summary>
        protected IReadOnlyReactiveProperty<Entity[]> CachedMissionEntities => InfernoCore.Instance.MissionEntities;

        #endregion Chace

        #region forEvents

        /// <summary>
        /// 100ms間隔のTickイベント
        /// </summary>
        public IObservable<Unit> OnThinnedTickAsObservable { get; }

        /// <summary>
        /// たぶん16msごとに実行されるイベント
        /// </summary>
        public IObservable<Unit> OnTickAsObservable { get; }

        /// <summary>
        /// 描画用のTickイベント
        /// </summary>
        public IObservable<Unit> OnDrawingTickAsObservable { get; private set; }

        private IObservable<KeyEventArgs> _onKeyDownAsObservable;

        public IObservable<KeyEventArgs> OnKeyDownAsObservable
        {
            get
            {
                if (_onKeyDownAsObservable != null)
                {
                    return _onKeyDownAsObservable;
                }

                _onKeyDownAsObservable =
                    Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(h => h.Invoke, h => KeyDown += h,
                            h => KeyDown -= h)
                        .Select(e => e.EventArgs)
                        .TakeUntil(_disposeSubject)
                        .Publish()
                        .RefCount();
                return _onKeyDownAsObservable;
            }
        }

        public IObservable<Unit> OnAllOnCommandObservable { get; private set; }

        /// <summary>
        /// InfernoEvent
        /// </summary>
        protected IObservable<IEventMessage> OnReceivedInfernoEvent
            => InfernoCore.OnReceivedEventMessage.ObserveOn(InfernoScheduler);

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
                .TakeUntil(_disposeSubject)
                .Publish()
                .RefCount();
        }

        /// <summary>
        /// 任意のTickObservableを生成する
        /// <returns></returns>
        protected IObservable<Unit> CreateTickAsObservable(TimeSpan timeSpan)
        {
            return OnTickAsObservable.ThrottleFirst(timeSpan, InfernoScheduler).Publish().RefCount();
        }

        #endregion forEvents

        #region forAbort

        /// <summary>
        /// ゲーム中断時に自動開放する対象リスト
        /// </summary>
        private readonly List<Entity> _autoReleaseEntities = new();

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

        public void DrawText(object text, float time = 3.0f)
        {
            ToastTextDrawing.Instance.DrawDebugText(text.ToString(), time);
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

        private readonly List<ICounter> _counterList = new();

        /// <summary>
        /// カウンタを登録して自動カウントさせる
        /// カウンタのUpdateにはIntervalの数値が渡される
        /// </summary>
        public void RegisterCounter(ICounter counter)
        {
            _counterList.Add(counter);
        }

        #endregion forTimer

        #region UI

        protected bool IsLangJpn => Game.Language == Language.Japanese;

        public virtual bool UseUI => false;
        public virtual string DisplayText => GetType().Name;
        public virtual MenuIndex MenuIndex => MenuIndex.Root;

        public virtual bool CanChangeActive => false;

        public virtual void OnUiMenuConstruct(ObjectPool pool, NativeMenu menu)
        {
        }

        bool IScriptUiBuilder.IsActive
        {
            get => IsActive;
            set { _infernoSynchronizationContext.Post(_ => { IsActive = value; }, null); }
        }

        #endregion
    }
}