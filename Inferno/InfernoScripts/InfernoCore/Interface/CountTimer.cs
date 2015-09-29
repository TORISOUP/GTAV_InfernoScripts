using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inferno
{

    /// <summary>
    /// カウンターのインターフェース定義
    /// </summary>
    interface ICountTimer
    {
        void TimerUpdate();
    }

    class CountTimer : ICountTimer
    {
        //表示を消すまで残りループ回数
        private int currentTickCounter;
        private int barSize;
        //タイマー変数
        private float timer;
        //サイズ補正用
        private float minorityTmp;

        CountTimer(float setTimer)
        {
            initializationTimer(setTimer);
        }

        /// <summary>
        /// 初期化
        /// </summary>
        private void initializationTimer(float setTimer)
        {
            timer = 0.0f;
            minorityTmp = 0.0f;
            currentTickCounter = (int)(setTimer * 10);
        }

        /// <summary>
        /// タイマーの更新
        /// </summary>
        public void TimerUpdate()
        {
           
        }
    }
}
