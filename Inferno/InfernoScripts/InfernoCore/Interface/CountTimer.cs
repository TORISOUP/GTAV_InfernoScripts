
namespace Inferno
{
    /// <summary>
    /// カウンターのインターフェース定義
    /// </summary>
    interface ICountTimer
    {
        int TickCount { get; }
        float CounterRate { get; }
        void Initialization(float setTime);
        void TimerUpdate();
    }

    class CountTimer : ICountTimer
    {
        //表示を消すまで残りループ回数
        private int currentTickCounter;
        //使い回す計算結果
        private float counterAdd;
        //カウンター
        private float counter;
        //少数部一時保存用
        private float counterMinority;

        /// <summary>
        /// 時間を指定し、インスタンスの初期化をする
        /// </summary>
        /// <param name="setTime">表示させたい時間</param>
        public CountTimer(float setTime)
        {
            Initialization(setTime);
        }

        public int TickCount { get { return currentTickCounter; } }
        public float CounterRate { get { return counter; } }

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="setTime">表示させたい時間</param>
        public void Initialization(float setTime)
        {
            currentTickCounter = ((int)setTime * 10);
            counterAdd = (20.0f / setTime) % 1.0f;
            counterMinority = 0.0f;
            counter = 0.0f;
            counterMinority = 0.0f;
        }

        /// <summary>
        /// タイマーの更新
        /// </summary>
        public void TimerUpdate()
        {
            counter = counterMinority;
            counter += counterAdd; 
            counterMinority = counter % 1.0f;
            currentTickCounter--;
        }
    }
}
