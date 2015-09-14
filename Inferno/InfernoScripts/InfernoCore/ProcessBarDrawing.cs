using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
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

        protected override void Setup()
        {
            Instance = this;
            //描画エリア
            _mContainer = new UIContainer(new Point(0, 0), new Size(500, 20));

            //バー表示が設定されていたら描画
            this.OnDrawingTickAsObservable
                .Where(_ => _mContainer.Items.Count > 0)
                .Subscribe(_ => _mContainer.Draw());
        }

        /// <summary>
        /// 指定位置に徐々に増加するバーの表示（時間指定）
        /// </summary>
        /// <param name="pos">表示させたい座標</param>
        /// <param name="time">ゲージが満タンになるまでの時間[s]</param>
        /// <param name="barColor">バー本体の色</param>
        /// <param name="backgroundColor">バーの背景色</param>
        public void DrawIncreaseProgressBar(Point pos, float time, Color barColor, Color backgroundColor)
        {
            StartCoroutine(DrawProgressBarEnumerator(pos, time, true, barColor, backgroundColor));
        }

        /// <summary>
        /// 指定位置に徐々に減少するバーの表示（時間指定）
        /// </summary>
        /// <param name="pos">表示させたい座標</param>
        /// <param name="time">ゲージが0になるまでの時間[s]</param>
        /// <param name="barColor">バー本体の色</param>
        /// <param name="backgroundColor">バーの背景色</param>
        public void DrawReduceProgressBar(Point pos, float time, Color barColor, Color backgroundColor)
        {
            StartCoroutine(DrawProgressBarEnumerator(pos, time, false, barColor, backgroundColor));
        }

        /// <summary>
        /// バーの表示（時間指定）
        /// </summary>
        /// <param name="pos">表示座標</param>
        /// <param name="time">表示時間</param>
        /// <param name="isBarAdd">true：増加するfalse:減少するバーに</param>
        /// <param name="barColor">バー本体の色</param>
        /// <param name="backgroundColor">バーの背景色</param>
        /// <returns></returns>
        private IEnumerable<Object> DrawProgressBarEnumerator(Point pos, float time, bool isBarAdd, Color barColor, Color backgroundColor)
        {
            //表示を消すまで残りループ回数
            var currentTickCounter = 0;
            currentTickCounter = (int)(time * 10);
            var barSize = isBarAdd ? 0 : 200;
            var minorityTmp = 0.0f;

            while (--currentTickCounter > 0)
            {
                _mContainer.Items.Add(new UIRectangle(new Point(pos.X, pos.Y - 5), new Size(210, 30), backgroundColor));
                _mContainer.Items.Add(new UIRectangle(new Point(pos.X + 5, pos.Y), new Size(barSize, 20), barColor));
                minorityTmp += (20 / time) % 1.0f;
                barSize += (20 / (int)time);
                if (minorityTmp >= 1.0f)
                {
                  
                    barSize += isBarAdd ? 1 : -1;
                    minorityTmp = minorityTmp % 1.0f;
                }
                yield return currentTickCounter;
            }

            _mContainer.Items.Clear();
        }

        //ToDo:各ProgressBarの中断機能つける
    }
}
