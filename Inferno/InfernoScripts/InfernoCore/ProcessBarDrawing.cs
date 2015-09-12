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
        //表示を消すまで残りループ回数
        private int _currentTickCounter = 0;

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
        /// 指定位置に進行状況を示すバーの表示
        /// </summary>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="time"></param>
        public void DrawProgressBar(Point pos, float time)
        {
            StartCoroutine(DrawProgressBarEnumerator(pos, time));
        }

        private IEnumerable<Object> DrawProgressBarEnumerator(Point pos, float time)
        {
            _currentTickCounter = (int)(time * 10);
            var barSize = 0;
            var minorityTmp = 0.0f;

            while (--_currentTickCounter > 0)
            {
                _mContainer.Items.Clear();
                _mContainer.Items.Add(new UIRectangle(new Point(pos.X, pos.Y - 5), new Size(210, 30), Color.Black));
                _mContainer.Items.Add(new UIRectangle(new Point(pos.X + 5, pos.Y), new Size(barSize, 20), Color.LightGreen));
                minorityTmp += (20 / time) % 1.0f;
                barSize += (20 / (int)time);
                if (minorityTmp >= 1.0f)
                {
                    barSize++;
                    minorityTmp = minorityTmp % 1.0f;
                }
                yield return _currentTickCounter;
            }

            _mContainer.Items.Clear();
        }
    }
}