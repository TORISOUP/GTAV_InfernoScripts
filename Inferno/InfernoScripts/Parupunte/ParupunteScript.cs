using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inferno.InfernoScripts.Parupunte
{
    /// <summary>
    ///　デバッグ用Attribute
    /// </summary>
    public class ParupunteDebug : Attribute
    {
        //trueにするとそのParupunteScriptが優先される
        public bool IsDebug;

        public ParupunteDebug(bool isDebug = false)
        {
            IsDebug = isDebug;
        }
    }

    abstract class ParupunteScript
    {

        public abstract string Name { get; }

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

        protected ParupunteScript(ParupunteCore core)
        {
            this.core = core;
            coroutineIds = new List<uint>();
        }

        /// <summary>
        /// パルプンテ開始前に1回だけ実行される
        /// </summary>
        public abstract void OnStart();

        /// <summary>
        /// 100msごとに実行される
        /// </summary>
        public virtual void OnUpdate()
        {
            ;
        }

        /// <summary>
        /// パルプンテ終了時に実行される
        /// </summary>
        public virtual void OnFinished()
        {
            ;
        }

        /// <summary>
        /// パルプンテ処理を終了する場合に呼び出す
        /// </summary>
        protected void ParupunteEnd()
        {
            IsActive = false;
            foreach (var id in coroutineIds)
            {
                StopCoroutine(id);
            }
            coroutineIds.Clear();
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
        /// コルーチンが有効かどうかチェックする
        /// </summary>
        protected bool IsCoroutineValid(uint id)
        {
            return core.CoroutineExists(id);
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
