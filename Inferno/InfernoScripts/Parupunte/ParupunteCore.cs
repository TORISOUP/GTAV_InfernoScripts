using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.InfernoScripts.Event.Isono;
using Inferno.Utilities;
using Reactive.Bindings.Extensions;

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

        /// <summary>
        /// デバッグ用
        /// </summary>
        private Type[] _debugParuputeScripts;

        /// <summary>
        /// いその用パルプンテ
        /// </summary>
        private Dictionary<string, Type> _isonoParupunteScripts;

        private UIContainer _mainTextUiContainer;

        private Dictionary<string, ParupunteConfigElement> _parupunteConfigs;

        /// <summary>
        /// パルプンテスクリプト一覧
        /// </summary>
        private Type[] _parupunteScritpts;

        private int _screenHeight;
        private int _screenWidth;
        private UIContainer _subTextUiContainer;
        private TimerUiTextManager timerText;

        private Dictionary<string, Type> IsonoParupunteScripts
        {
            get
            {
                if (_isonoParupunteScripts != null) return _isonoParupunteScripts;
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
                .Subscribe(_ => ParupunteStart(ChooseParupounteScript()));

            CreateInputKeywordAsObservable("snt")
                .Where(_ => IsActive)
                .Subscribe(_ => ParupunteStop());

            OnKeyDownAsObservable
                .Where(x => x.KeyCode == Keys.NumPad0)
                .ThrottleFirst(TimeSpan.FromSeconds(2f), InfernoScriptScheduler)
                .Subscribe(_ =>
                {
                    if (IsActive)
                        ParupunteStop();
                    else
                        ParupunteStart(ChooseParupounteScript());
                });

            //パルプンテが停止したタイミングで開放
            IsActiveAsObservable
                .Where(x => !x)
                .Subscribe(_ =>
                {
                    foreach (var entity in _autoReleaseEntitiesList.Where(entity => entity.IsSafeExist()))
                        entity.MarkAsNoLongerNeeded();

                    _autoReleaseEntitiesList.Clear();
                });

            var nextIsonoTime = Time;

            OnRecievedInfernoEvent
                .OfType<IsonoMessage>()
                .Where(_ => (nextIsonoTime - Time).Ticks <= 0)
                .Retry()
                .Subscribe(c =>
                {
                    var r = IsonoMethod(c.Command);
                    if (r) nextIsonoTime = Time.Add(TimeSpan.FromSeconds(4));
                });

            #endregion EventHook

            #region Drawer

            var screenResolution = NativeFunctions.GetScreenResolution();
            _screenHeight = (int)screenResolution.Y;
            _screenWidth = (int)screenResolution.X;
            _mainTextUiContainer = new UIContainer(
                new Point(0, 0), new Size(_screenWidth, _screenHeight));
            _subTextUiContainer = new UIContainer(
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
                if (loadConfig.ContainsKey(kv.Key)) value = loadConfig[kv.Key];

                mergedConfig[kv.Key] = value;
            }

            // 最終的なconfをファイルに書き出す
            configRepository.SaveSettings(mergedConfig);

            // 設定完了
            _parupunteConfigs = mergedConfig;
        }


        private bool IsonoMethod(string command)
        {
            var c = command;

            if (c.Contains("とまれ"))
            {
                //       ParupunteStop();
                //       return true;
            }

            if (IsActive) return false;

            if (c.Contains("ぱるぷんて"))
            {
                ParupunteStart(ChooseParupounteScript());
                return true;
            }

            var result = IsonoParupunteScripts.Keys.FirstOrDefault(x => command.Contains(x));
            if (string.IsNullOrEmpty(result) || !IsonoParupunteScripts.ContainsKey(result)) return false;
            ParupunteStart(IsonoParupunteScripts[result]);
            return true;
        }


        /// <summary>
        /// パルプンテの実行を開始する
        /// </summary>
        private void ParupunteStart(Type script)
        {
            if (IsActive) return;

            IsActive = true;

            var conf = _parupunteConfigs.ContainsKey(script.Name)
                ? _parupunteConfigs[script.Name]
                : ParupunteConfigElement.Default;

            //ThreadPool上で初期化（プチフリ回避）
            Observable.Start(() => Activator.CreateInstance(script, this, conf) as ParupunteScript,
                    Scheduler.ThreadPool)
                .OnErrorRetry((Exception ex) => { LogWrite(ex.ToString()); }, 3, TimeSpan.FromMilliseconds(300))
                .Subscribe(x => StartCoroutine(ParupunteCoreCoroutine(x)), ex =>
                {
                    //       LogWrite(ex.ToString());
                    IsActive = false;
                });
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
                return _debugParuputeScripts[Random.Next(0, _debugParuputeScripts.Length)];

            return _parupunteScritpts[Random.Next(0, _parupunteScritpts.Length)];
        }

        /// <summary>
        /// パルプンテのメインコルーチン
        /// </summary>
        /// <returns></returns>
        private IEnumerable<object> ParupunteCoreCoroutine(ParupunteScript script)
        {
            yield return null;

            if (!IsActive) yield break;

            if (script == null)
            {
                IsActive = false;
                yield break;
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
                yield break;
            }

            //名前を出してスタート
            StartCoroutine(ParupunteDrawCoroutine(GetPlayerName() + "はパルプンテを唱えた!", script.Name));
            yield return WaitForSeconds(2);

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
                yield break;
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
                    yield break;
                }

                yield return null; //100ms待機
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

        /// <summary>
        /// 画面に名前とかを出す
        /// </summary>
        private IEnumerable<object> ParupunteDrawCoroutine(string callString, string scriptname)
        {
            //○はパルプンテを唱えた！の部分
            timerText.Set(CreateMainText(callString), 2.0f);

            //2秒画面に出す
            yield return WaitForSeconds(2);

            //効果名
            timerText.Set(CreateMainText(scriptname), 3.0f);

            //3秒画面に出す
            yield return WaitForSeconds(3);
        }

        /// <summary>
        /// テキストを表示する(ParupunteScript用)
        /// </summary>
        public void DrawParupunteText(string text, float duration)
        {
            timerText.Set(CreateMainText(text), duration);
        }

        private UIText CreateMainText(string text)
        {
            return new UIText(text,
                new Point((int)(_screenWidth * _mainTextPositionScale.X),
                    (int)(_screenHeight * _mainTextPositionScale.Y)),
                0.8f, Color.White, 0, true, false, true);
        }

        private UIText CreateSubText(string text)
        {
            return new UIText(text,
                new Point((int)(_screenWidth * _subTextPositionScale.X),
                    (int)(_screenHeight * _subTextPositionScale.Y)),
                0.4f, Color.White, 0, false, false, true);
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

        /// <summary>
        /// コルーチンの実行をCoreに委託する
        /// </summary>
        public uint RegisterCoroutine(IEnumerable<object> coroutine)
        {
            return StartCoroutine(coroutine);
        }

        /// <summary>
        /// コルーチンの実行を終了する
        /// </summary>
        /// <param scriptname="id"></param>
        public void UnregisterCoroutine(uint id)
        {
            StopCoroutine(id);
        }

        /// <summary>
        /// WaitForSeconsの結果を返す
        /// </summary>
        public IEnumerable CreateWaitForSeconds(float seconds)
        {
            return WaitForSeconds(seconds);
        }

        public IEnumerable CreateRadomWait()
        {
            return RandomWait();
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
            if (entity.IsSafeExist()) _autoReleaseEntitiesList.Add(entity);
            //中断時にも対応させる
            base.AutoReleaseOnGameEnd(entity);
        }

        public new void AutoReleaseOnGameEnd(Entity entity)
        {
            base.AutoReleaseOnGameEnd(entity);
        }
    }
}