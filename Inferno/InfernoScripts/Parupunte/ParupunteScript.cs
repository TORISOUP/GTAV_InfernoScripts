using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inferno.InfernoScripts.Parupunte
{
    abstract class ParupunteScript
    {

        /// <summary>
        /// パルプンテの処理が実行中であるか
        /// </summary>
        public bool IsActive { get; private set; } = true;

        /// <summary>
        /// コア
        /// </summary>
        protected ParupunteCore core;

        protected ParupunteScript(ParupunteCore core)
        {
            this.core = core;
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
        }
    }
}
