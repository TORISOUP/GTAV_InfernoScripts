using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Inferno.InfernoScripts.Parupunte
{
    class ParupunteCore : InfernoScript
    {
        /// <summary>
        /// パルプンテスクリプト一覧
        /// </summary>
        private Type[] _parupunteScritpts;

        /// <summary>
        /// デバッグ用
        /// </summary>
        private Type[] _debugParuputeScripts;

        protected override int TickInterval { get; } = 100;

        protected override void Setup()
        {
            IsActive = false;

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

            CreateInputKeywordAsObservable("rnt")
                .Where(_ => !IsActive)
                .Subscribe(_ => ParupunteStart());
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

            script.OnStart();
            while (script.IsActive)
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
        /// コルーチンの実行をCoreに委託する
        /// </summary>
        public uint RegisterCoroutine(IEnumerable<object> coroutine )
        {
            return StartCoroutine(coroutine);
        }

        /// <summary>
        /// コルーチンの実行を終了する
        /// </summary>
        /// <param name="id"></param>
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
    }
}
