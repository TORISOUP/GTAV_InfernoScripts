using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.UI;
using Inferno.InfernoScripts.Event.Isono;
using Inferno.InfernoScripts.InfernoCore.UI;
using Inferno.Utilities;
using LemonUI;
using LemonUI.Menus;

namespace Inferno.InfernoScripts.Parupunte
{
    internal class ParupunteCore : InfernoScript
    {
        /// <summary>
        /// NoLongerNeededを遅延して設定する対象リスト
        /// </summary>
        private readonly List<Entity> _autoReleaseEntitiesList = new();

        private readonly Vector2 _mainTextPositionScale = new(0.5f, 0.8f);
        private readonly Stopwatch _stopWatch = Stopwatch.StartNew();
        private readonly Vector2 _subTextPositionScale = new(0.0f, 0.0f);
        private readonly object _gate = new object();

        /// <summary>
        /// デバッグ用
        /// </summary>
        private Type[] _debugParuputeScripts;

        /// <summary>
        /// いその用パルプンテ
        /// </summary>
        private Dictionary<string, Type> _isonoParupunteScripts;

        private ContainerElement _mainTextUiContainer;

        private Dictionary<string, ParupunteConfigElement> _parupunteConfigs;

        /// <summary>
        /// パルプンテスクリプト一覧
        /// </summary>
        private Type[] _parupunteScritpts;

        private int _screenHeight;
        private int _screenWidth;
        private ContainerElement _subTextUiContainer;
        private TimerUiTextManager timerText;
        private ParupunteScript _currentScript;

        private Dictionary<string, Type> IsonoParupunteScripts
        {
            get
            {
                if (_isonoParupunteScripts != null)
                {
                    return _isonoParupunteScripts;
                }

                _isonoParupunteScripts =
                    _parupunteScritpts
                        .Select(x => new { type = x, isono = x.GetCustomAttribute<ParupunteIsono>() })
                        .Where(x => !string.IsNullOrEmpty(x.isono?.Command))
                        .ToDictionary(x => x.isono.Command, x => x.type);
                return _isonoParupunteScripts;
            }
        }

        private TimeSpan Time => _stopWatch.Elapsed;

        protected override void Setup()
        {
            Interval = 0;
            IsActive = false;
            timerText = new TimerUiTextManager(this);

            #region ParunteScripts

            //RefrectionでParupunteScriptを継承しているクラスをすべて取得する
            _parupunteScritpts =
                Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .Where(type => type.BaseType != null && type.BaseType == typeof(ParupunteScript))
                    .Where(x =>
                    {
                        var attribute = x.GetCustomAttribute<ParupunteDebug>();
                        return attribute == null || !attribute.IsIgnore;
                    })
                    .ToArray();

            _debugParuputeScripts = _parupunteScritpts.Where(x =>
                {
                    var attribute = x.GetCustomAttribute<ParupunteDebug>();
                    return attribute != null && attribute.IsDebug;
                })
                .ToArray();

            #endregion ParunteScripts

            #region Config

            SetConfigData(_parupunteScritpts);

            #endregion

            #region EventHook

            CreateInputKeywordAsObservable("rnt")
                .Where(_ => !IsActive)
                .Subscribe(_ => { ParupunteStart(ChooseParupounteScript(), DestroyCancellationToken); });

            CreateInputKeywordAsObservable("snt")
                .Where(_ => IsActive)
                .Subscribe(_ => ParupunteStop());


            OnKeyDownAsObservable
                .Where(x => x.KeyCode is Keys.NumPad0 or Keys.PageDown)
                .ThrottleFirst(TimeSpan.FromSeconds(2f), base.InfernoScheduler)
                .Subscribe(_ =>
                {
                    if (IsActive)
                    {
                        ParupunteStop();
                    }
                    else
                    {
                        ParupunteStart(ChooseParupounteScript(), DestroyCancellationToken);
                    }
                });

            //パルプンテが停止したタイミングで開放
            IsActiveRP
                .Where(x => !x)
                .Subscribe(_ =>
                {
                    foreach (var entity in _autoReleaseEntitiesList.Where(entity => entity.IsSafeExist()))
                    {
                        entity.MarkAsNoLongerNeeded();
                    }

                    _autoReleaseEntitiesList.Clear();
                });

            var nextIsonoTime = Time;

            OnReceivedInfernoEvent
                .OfType<IsonoMessage>()
                .ObserveOn(InfernoScheduler)
                .Where(_ => (nextIsonoTime - Time).Ticks <= 0)
                .Retry()
                .Subscribe(c =>
                {
                    var r = IsonoMethod(c.Command);
                    if (r)
                    {
                        nextIsonoTime = Time.Add(TimeSpan.FromSeconds(4));
                    }
                });

            OnAbortAsync.Subscribe(_ =>
            {
                _currentScript?.OnFinishedCore();
                _currentScript = null;
            });

            #endregion EventHook

            #region Drawer

            var screenResolution = NativeFunctions.GetScreenResolution();
            _screenHeight = (int)screenResolution.Y;
            _screenWidth = (int)screenResolution.X;
            _mainTextUiContainer = new ContainerElement(
                new Point(0, 0), new Size(_screenWidth, _screenHeight));
            _subTextUiContainer = new ContainerElement(
                new Point(0, 0), new Size(_screenWidth, _screenHeight));

            //テキストが更新されたら詰め直す
            timerText.OnSetTextAsObservable.Subscribe(_ =>
            {
                _mainTextUiContainer.Items.Clear();
                _mainTextUiContainer.Items.Add(timerText.Text);
            });
            //テキストが時間切れしたら消す
            OnThinnedTickAsObservable.Select(_ => timerText.IsEnabled)
                .DistinctUntilChanged()
                .Where(x => !x)
                .Subscribe(_ => _mainTextUiContainer.Items.Clear());

            OnDrawingTickAsObservable
                .Where(_ => _mainTextUiContainer.Items.Any() || _subTextUiContainer.Items.Any())
                .Subscribe(_ =>
                {
                    _mainTextUiContainer.Draw();
                    _subTextUiContainer.Draw();
                });

            #endregion Drawer
        }

        // Configファイルの設定を行う
        private void SetConfigData(Type[] parupunteScripts)
        {
            var configRepository = new ParupunteConfigRepository();

            // jsonから読み込んだConfig値
            var loadConfig = configRepository.LoadSettingFile();

            // デフォルト値
            var defaultConfig = parupunteScripts
                .Select(x => new { x.Name, Attribute = x.GetCustomAttribute<ParupunteConfigAttribute>() })
                .Where(x => x.Attribute != null)
                .ToDictionary(x => x.Name, x =>
                {
                    var a = x.Attribute;
                    return new ParupunteConfigElement(a.DefaultStartMessage, a.DefaultSubMessage, a.DefaultEndMessage);
                });

            // 合成したConfig値
            var mergedConfig = new Dictionary<string, ParupunteConfigElement>();

            // デフォルト値のConfを、読み込んだConf値で上書きする
            foreach (var kv in defaultConfig)
            {
                var value = kv.Value;
                if (loadConfig.ContainsKey(kv.Key))
                {
                    value = loadConfig[kv.Key];
                }

                mergedConfig[kv.Key] = value;
            }

            // 最終的なconfをファイルに書き出す
            configRepository.SaveSettings(mergedConfig).Forget();

            // 設定完了
            _parupunteConfigs = mergedConfig;
        }


        private bool IsonoMethod(string command)
        {
            var c = command;
            if (c.Contains("とまれ"))
            {
                ParupunteStop();
                return true;
            }

            if (IsActive)
            {
                return false;
            }

            if (c.Contains("ぱるぷんて"))
            {
                ParupunteStart(ChooseParupounteScript(), DestroyCancellationToken);
                return true;
            }

            var result = IsonoParupunteScripts.Keys.FirstOrDefault(command.Contains);
            if (string.IsNullOrEmpty(result) || !IsonoParupunteScripts.ContainsKey(result))
            {
                return false;
            }

            ParupunteStart(IsonoParupunteScripts[result], DestroyCancellationToken);
            return true;
        }


        /// <summary>
        /// パルプンテの実行を開始する
        /// </summary>
        private void ParupunteStart(Type script, CancellationToken ct)
        {
            lock (_gate)
            {
                if (IsActive)
                {
                    return;
                }

                IsActive = true;
            }

            var conf = _parupunteConfigs.TryGetValue(script.Name, out var config)
                ? config
                : ParupunteConfigElement.Default;

            try
            {
                // スクリプトのインスタンスを生成
                _currentScript = Activator.CreateInstance(script, this, conf) as ParupunteScript;
                ParupunteCoreLoopAsync(_currentScript, ct).Forget();
            }
            catch (Exception)
            {
                IsActive = false;
            }
        }

        /// <summary>
        /// パルプンテを中断する
        /// </summary>
        private void ParupunteStop()
        {
            DrawText("Parupunte:Abort");
            IsActive = false;
        }

        /// <summary>
        /// パルプンテスクリプトから抽選する
        /// </summary>
        private Type ChooseParupounteScript()
        {
            if (_debugParuputeScripts.Any())
                //デバッグ指定のやつがあるならそっち優先で取り出す
            {
                return _debugParuputeScripts[Random.Next(0, _debugParuputeScripts.Length)];
            }

            return _parupunteScritpts[Random.Next(0, _parupunteScritpts.Length)];
        }

        /// <summary>
        /// パルプンテのメインコルーチン
        /// </summary>
        /// <returns></returns>
        private async ValueTask ParupunteCoreLoopAsync(ParupunteScript script, CancellationToken ct)
        {
            try
            {
                await YieldAsync(ct);

                if (!IsActive)
                {
                    return;
                }

                if (script == null)
                {
                    IsActive = false;
                    return;
                }

                try
                {
                    script.OnSetUp();
                    script.OnSetNames();
                }
                catch (Exception e)
                {
                    LogWrite(e.ToString());
                    script.OnFinishedCore();
                    IsActive = false;
                    return;
                }

                //名前を出してスタート
                ParupunteDrawAsync(GetPlayerName() + "はパルプンテを唱えた!", script.Name, ct).Forget();
                await DelaySecondsAsync(2, ct);

                try
                {
                    script.OnStart();
                }
                catch (Exception e)
                {
                    LogWrite(e.ToString());
                    script.OnFinishedCore();
                    IsActive = false;
                    _subTextUiContainer.Items.Clear();
                    return;
                }

                //サブタイトルを出す
                var subTitle = string.IsNullOrEmpty(script.SubName) ? script.Name : script.SubName;
                _subTextUiContainer.Items.Clear();
                _subTextUiContainer.Items.Add(CreateSubText(subTitle));

                while (script.IsActive && IsActive)
                {
                    try
                    {
                        //スクリプトのUpdateを実行
                        script.OnUpdateCore();
                    }
                    catch (Exception e)
                    {
                        LogWrite(e.ToString());
                        script.OnFinishedCore();
                        IsActive = false;
                        _subTextUiContainer.Items.Clear();
                        return;
                    }

                    // 1フレーム待機
                    await YieldAsync(ct);
                }

                try
                {
                    script.OnFinishedCore();
                }
                catch (Exception e)
                {
                    LogWrite(e.ToString());
                }
                finally
                {
                    IsActive = false;
                    _subTextUiContainer.Items.Clear();
                }
            }
            finally
            {
                script?.OnFinishedCore();
                _currentScript = null;
            }
        }

        /// <summary>
        /// 画面に名前とかを出す
        /// </summary>
        private async ValueTask ParupunteDrawAsync(string callString, string scriptname, CancellationToken ct)
        {
            //○はパルプンテを唱えた！の部分
            timerText.Set(CreateMainText(callString), 2.0f);

            //2秒画面に出す
            await DelaySecondsAsync(2, ct);

            //効果名
            timerText.Set(CreateMainText(scriptname), 3.0f);

            //3秒画面に出す
            await DelaySecondsAsync(3, ct);
        }

        /// <summary>
        /// テキストを表示する(ParupunteScript用)
        /// </summary>
        public void DrawParupunteText(string text, float duration)
        {
            timerText.Set(CreateMainText(text), duration);
        }

        private TextElement CreateMainText(string text)
        {
            return new TextElement(text,
                new Point((int)(_screenWidth * _mainTextPositionScale.X),
                    (int)(_screenHeight * _mainTextPositionScale.Y)),
                0.8f, Color.White, 0, Alignment.Center, false, true);
        }

        private TextElement CreateSubText(string text)
        {
            return new TextElement(text,
                new Point((int)(_screenWidth * _subTextPositionScale.X),
                    (int)(_screenHeight * _subTextPositionScale.Y)),
                0.4f, Color.White, 0, Alignment.Left, false, true);
        }


        private string GetPlayerName()
        {
            var hash = (PedHash)PlayerPed.Model.Hash;
            switch (hash)
            {
                case PedHash.Trevor:
                    return NativeFunctions.GetGXTEntry("BLIP_TREV");

                case PedHash.Michael:
                    return NativeFunctions.GetGXTEntry("BLIP_MICHAEL");

                case PedHash.Franklin:
                    return NativeFunctions.GetGXTEntry("BLIP_FRANKLIN");

                default:
                    return hash.ToString();
            }
        }

        public void AddProgressBar(ReduceCounter reduceCounter)
        {
            var prgoressbarData = new ProgressBarData(reduceCounter,
                new Point(0, 15),
                Color.FromArgb(200, 0, 127, 255),
                Color.FromArgb(128, 0, 0, 0),
                DrawType.RightToLeft, 100, 10, 2);
            RegisterProgressBar(prgoressbarData);
            RegisterCounter(reduceCounter);
        }


        /// <summary>
        /// パルプンテが終了した時に自動的に開放してくれる
        /// </summary>
        public void AutoReleaseOnParupunteEnd(Entity entity)
        {
            if (entity.IsSafeExist())
            {
                _autoReleaseEntitiesList.Add(entity);
            }

            //中断時にも対応させる
            base.AutoReleaseOnGameEnd(entity);
        }

        public new void AutoReleaseOnGameEnd(Entity entity)
        {
            base.AutoReleaseOnGameEnd(entity);
        }

        #region forTask

        public new ValueTask DelaySecondsAsync(float seconds, CancellationToken ct = default)
        {
            return DelayAsync(TimeSpan.FromSeconds(seconds), ct);
        }

        public new ValueTask DelayAsync(TimeSpan timeSpan, CancellationToken ct = default)
        {
            return base.DelayAsync(timeSpan, ct);
        }

        public new ValueTask DelayFrameAsync(int frame, CancellationToken ct = default)
        {
            return base.DelayFrameAsync(frame, ct);
        }

        public new ValueTask YieldAsync(CancellationToken ct = default)
        {
            return base.YieldAsync(ct);
        }

        public new ValueTask DelayRandomFrameAsync(int min, int max, CancellationToken ct)
        {
            return base.DelayRandomFrameAsync(min, max, ct);
        }

        public new ValueTask DelayRandomSecondsAsync(float min, float max, CancellationToken ct)
        {
            return base.DelayRandomSecondsAsync(min, max, ct);
        }

        #endregion

        #region UI

        public override bool UseUI => true;
        public override string DisplayText => IsLangJpn ? "パルプンテ" : "Parupunte";

        public override bool CanChangeActive => false;

        public override MenuIndex MenuIndex => MenuIndex.Root;

        public override void OnUiMenuConstruct(ObjectPool pool, NativeMenu subMenu)
        {
            var context = InfernoSynchronizationContext;

            // パルプンテのランダム実行
            subMenu.AddButton("Start", IsLangJpn ? "ランダムに実行" : "Start random",
                () => { context.Post(_ => ParupunteStart(ChooseParupounteScript(), DestroyCancellationToken), null); });

            // パルプンテの停止
            subMenu.AddButton("Stop", IsLangJpn ? "実行中のパルプンテを停止" : "Stop the running parupunte",
                () => { context.Post(_ => ParupunteStop(), null); });

            // リスト一覧作成
            var listMenu = new NativeMenu("Effect list", "Effect list");
            {
                foreach (var parupunteScritpt in _parupunteScritpts)
                {
                    var script = parupunteScritpt;
                    var name = script.Name;
                    var item = new NativeItem(name);
                    item.Activated += (_, _) =>
                    {
                        context.Post(_ =>
                        {
                            ParupunteStart(script, DestroyCancellationToken);
                        }, null);
                    };
                    listMenu.Add(item);
                }
            }
            subMenu.AddSubMenu(listMenu);
            pool.Add(listMenu);
        }
        

        #endregion
    }
}