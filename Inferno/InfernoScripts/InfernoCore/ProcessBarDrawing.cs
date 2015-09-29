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

        protected override void Setup()
        {
            Instance = this;
            //描画エリア
            _mContainer = new UIContainer(new Point(0, 0), new Size(500, 20));

            //バー表示が設定されていたら描画
            lock (_mContainer)
            {
                this.OnDrawingTickAsObservable
                .Where(_ => _mContainer.Items.Count > 0)
                .Subscribe(_ => _mContainer.Draw());
            }
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
            var id = StartCoroutine(DrawProgressBarEnumerator(pos, time, progressBarType, barColor, backgroundColor));
            _coroutineIds.Add(id);
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
            //表示を消すまで残りループ回数
            var currentTickCounter = 0;
            currentTickCounter = (int)(time * 10);
            var isBarAdd = progressBarType == 0;
            //バーの初期サイズ
            var barSize = isBarAdd ? 0 : 200;
            //サイズ補正用
            var minorityTmp = 0.0f;
            //使い回す計算結果
            var minorityTmpAdd = (20/time)%1.0f;
            var barSizeAdd = (20/(int) time);

            while (--currentTickCounter > 0)
            {
                _mContainer.Items.Add(new UIRectangle(new Point(pos.X, pos.Y - 5), new Size(210, 30), backgroundColor));
                _mContainer.Items.Add(new UIRectangle(new Point(pos.X + 5, pos.Y), new Size(barSize, 20), barColor));
                minorityTmp += minorityTmpAdd;
                barSize += isBarAdd ? barSizeAdd : -barSizeAdd;
                if (minorityTmp >= 1.0f)
                {
                    barSize += isBarAdd ? 1 : -1;
                    minorityTmp = minorityTmp % 1.0f;
                }
                yield return currentTickCounter;
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
