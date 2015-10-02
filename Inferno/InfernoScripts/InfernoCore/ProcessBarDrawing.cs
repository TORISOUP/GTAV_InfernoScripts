using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using GTA;

namespace Inferno
{
    /// <summary>
    /// プログレスバーの表示管理
    /// </summary>
    public class ProcessBarDrawing : InfernoScript
    {
        private UIContainer _mContainer = null;

        public static ProcessBarDrawing Instance { get; private set; }

        private List<uint> _coroutineIds = new List<uint>();

        private CountTimer _countTimer;

        protected override void Setup()
        {
            Instance = this;
            //描画エリア
            _mContainer = new UIContainer(new Point(0, 0), new Size(500, 20));

            //バー表示が設定されていたら描画
            this.OnDrawingTickAsObservable
            .Where(_ => _mContainer.Items.Count > 0)
            .Subscribe(_ => _mContainer.Draw());
            
            //ICountTimerのTimerUpdate()の定期呼び出し
            this.OnTickAsObservable
                .Where(_ => _countTimer != null)
                .Subscribe(_ => _countTimer.TimerUpdate());
        }

        /// <summary>
        /// 指定位置にゲージの表示（時間指定）
        /// </summary>
        /// <param name="pos">表示させたい座標</param>
        /// <param name="time">ゲージが満タンになるまでの時間[s]</param>
        /// <param name="barColor">バー本体の色</param>
        /// <param name="backgroundColor">バーの背景色</param>
        /// <param name="progressBarType">増加or減少するゲージの指定</param>
        public void DrawProgressBar(Point pos, float time, Color barColor, Color backgroundColor, ProgressBarType progressBarType)
        {
            _countTimer = new CountTimer(time);
            lock (this)
            {
                var id = StartCoroutine(DrawProgressBarEnumerator(pos, time, progressBarType, barColor, backgroundColor));
                _coroutineIds.Add(id);
            }
        }

        /// <summary>
        /// バーの表示（時間指定）
        /// </summary>
        /// <param name="pos">表示座標</param>
        /// <param name="time">表示時間</param>
        /// <param name="progressBarType">増加or減少するゲージの指定</param>
        /// <param name="barColor">バー本体の色</param>
        /// <param name="backgroundColor">バーの背景色</param>
        /// <returns></returns>
        private IEnumerable<Object> DrawProgressBarEnumerator(Point pos, float time, ProgressBarType progressBarType, Color barColor, Color backgroundColor)
        {
            var isBarAdd = (progressBarType == 0);
            var barSize = isBarAdd ? 0 : 200;
            while (_countTimer.TickCount > 0)
            {
                var counterRate = _countTimer.CounterRate;
                var  barSizeAdd = (20 / (int)time) + (int)counterRate;
                barSize += isBarAdd ? barSizeAdd : -barSizeAdd;

                _mContainer.Items.Add(new UIRectangle(new Point(pos.X, pos.Y - 5), new Size(210, 30), backgroundColor));
                _mContainer.Items.Add(new UIRectangle(new Point(pos.X + 5, pos.Y), new Size(barSize, 20), barColor));
                yield return _countTimer.TickCount;
            }

            _mContainer.Items.Clear();
        }

        /// <summary>
        /// プログレスバーを全削除
        /// </summary>
        public void StopAllProgressBarCoroutine()
        {
            foreach (var id in _coroutineIds)
            {
                StopCoroutine(id);
            }
            _coroutineIds.Clear();
            _mContainer.Items.Clear();
        }
    }
}
