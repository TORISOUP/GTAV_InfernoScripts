using System;
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
            var scriptType = _parupunteScritpts[Random.Next(0, _parupunteScritpts.Length)];
            //インスタンス化
            var scriptInstance = Activator.CreateInstance(scriptType, this) as ParupunteScript;
            //コルーチン開始
            StartCoroutine(ParupunteCoreCoroutine(scriptInstance));
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
    }
}
