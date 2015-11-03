using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;

namespace Inferno.InfernoScripts.Parupunte
{
    class ParupunteCore : InfernoScript
    {

        TCPManager tcpManager = new TCPManager();
        /// <summary>
        /// パルプンテスクリプト一覧
        /// </summary>
        private Type[] _parupunteScritpts;

        /// <summary>
        /// デバッグ用
        /// </summary>
        private Type[] _debugParuputeScripts;
        protected override int TickInterval { get; } = 100;

        private UIContainer _mContainer;
        private int _screenHeight;
        private int _screenWidth;
        private Vector2 _textPositionScale = new Vector2(0.5f, 0.8f);

        protected override void Setup()
        {
            tcpManager.ServerStart();
            IsActive = false;

            #region ParunteScripts
            //RefrectionでParupunteScriptを継承しているクラスをすべて取得する
            _parupunteScritpts =
                Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .Where(type => type.BaseType != null && type.BaseType == typeof (ParupunteScript))
                    .ToArray();

            _debugParuputeScripts = _parupunteScritpts.Where(x =>
            {
                var attribute = x.GetCustomAttribute<ParupunteDebug>();
                return attribute != null && attribute.IsDebug;
            }).ToArray();

            #endregion

            #region EventHook
            CreateInputKeywordAsObservable("rnt")
                .Where(_ => !IsActive)
                .Subscribe(_ => ParupunteStart());

            CreateInputKeywordAsObservable("snt")
                .Where(_ => IsActive)
                .Subscribe(_ => ParupunteStop());
            #endregion

            #region Drawer
            var screenResolution = NativeFunctions.GetScreenResolution();
            _screenHeight = (int)screenResolution.Y;
            _screenWidth = (int)screenResolution.X;
            _mContainer = new UIContainer(
                new Point(0, 0), new Size(_screenWidth, _screenHeight));
            this.OnDrawingTickAsObservable
                .Where(_ => _mContainer.Items.Any())
                .Subscribe(_ => _mContainer.Draw());

            #endregion

        }



        /// <summary>
        /// パルプンテの実行を開始する
        /// </summary>
        private void ParupunteStart()
        {
            IsActive = true;

            //抽選
            var scriptType = ChooseParupounteScript();
            //インスタンス化
            var scriptInstance = Activator.CreateInstance(scriptType, this) as ParupunteScript;
            //コルーチン開始
            StartCoroutine(ParupunteCoreCoroutine(scriptInstance));

        }

        /// <summary>
        /// パルプンテを中断する
        /// </summary>
        private void ParupunteStop()
        {
            IsActive = false;
        }

        /// <summary>
        /// パルプンテスクリプトから抽選する
        /// </summary>
        private Type ChooseParupounteScript()
        {
            if (_debugParuputeScripts.Any())
            {
                //デバッグ指定のやつがあるならそっち優先で取り出す
                return _debugParuputeScripts[Random.Next(0, _debugParuputeScripts.Length)];
            }
            return _parupunteScritpts[Random.Next(0, _parupunteScritpts.Length)];
        }

        /// <summary>
        /// パルプンテのメインコルーチン
        /// </summary>
        /// <returns></returns>
        private IEnumerable<object> ParupunteCoreCoroutine(ParupunteScript script)
        {
            if (script == null)
            {
                IsActive = false;
                yield break;
            }

            //名前を出してスタート
            StartCoroutine(ParupunteDrawCoroutine(GetPlayerName() + "はパルプンテを唱えた!",script.Name));
            yield return WaitForSeconds(2);

            script.OnStart();
            while (script.IsActive && IsActive)
            {
                try
                {
                    //スクリプトのUpdateを実行
                    script.OnUpdate();
                }
                catch (Exception e)
                {
                    LogWrite(e.ToString());
                    script.OnFinished();
                    IsActive = false;
                    yield break;
                }

                yield return null; //100ms待機
            }

            script.OnFinished();
            IsActive = false;
        }

        /// <summary>
        /// 画面に名前とかを出す
        /// </summary>
        private IEnumerable<object> ParupunteDrawCoroutine(string callString ,string scriptname)
        {
           _mContainer.Items.Clear();

            //○はパルプンテを唱えた！の部分
            _mContainer.Items.Add(CreateUIText(callString));
            
            var mess = new RequestDataPackage(callString);
            tcpManager.SendToAll(mess.ToJson());
            //2秒画面に出す
            yield return WaitForSeconds(2);
            //消す
            _mContainer.Items.Clear();

            //効果名
            _mContainer.Items.Add(CreateUIText(scriptname));
            mess = new RequestDataPackage(scriptname);
            tcpManager.SendToAll(mess.ToJson());

            //3秒画面に出す
            yield return WaitForSeconds(2);
            //消す
            _mContainer.Items.Clear();

        }

        private UIText CreateUIText(string text)
        {
            return new UIText(text,new Point((int)(_screenWidth * _textPositionScale.X), (int)(_screenHeight * _textPositionScale.Y)),
               0.8f, Color.White, 0, true);
        }

        private string GetPlayerName()
        {
            var hash = (PedHash) PlayerPed.Model.Hash;
            switch (hash)
            {
                case PedHash.Trevor:
                    return Game.GetGXTEntry("BLIP_TREV");
                case PedHash.Michael:
                    return Game.GetGXTEntry("BLIP_MICHAEL");
                case PedHash.Franklin:
                    return Game.GetGXTEntry("BLIP_FRANKLIN");
                default:
                    return hash.ToString();
            }
        }


        /// <summary>
        /// コルーチンの実行をCoreに委託する
        /// </summary>
        public uint RegisterCoroutine(IEnumerable<object> coroutine )
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

        public void AddProgressBar(ReduceCounter reduceCounter)
        {
            var prgoressbarData = new ProgressBarData(reduceCounter,
                new Point(_screenWidth -10, _screenHeight -100), //画面右下
                Color.FromArgb(200, 0, 127, 255),
                Color.FromArgb(128, 0, 0, 0),
                DrawType.TopToBottom, 10, 100, 2);
            RegisterProgressBar(prgoressbarData);
            RegisterCounter(reduceCounter);
        }
    }
}
