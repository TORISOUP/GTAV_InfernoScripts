using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using GTA;
using GTA.UI;

namespace Inferno
{
    /// <summary>
    /// プログレスバーの表示管理
    /// </summary>
    public class ProgressBarDrawing : InfernoScript
    {
        private readonly object _lockObject = new();

        private readonly List<ProgressBarData> _progressBarDataList = new();
        private ContainerElement _container;

        public static ProgressBarDrawing Instance { get; private set; }

        protected override void Setup()
        {
            Instance = this;
            //描画エリア
            _container = new ContainerElement(new Point(0, 0), new Size(500, 20));

            //バー表示が設定されていたら描画
            OnDrawingTickAsObservable
                .Where(_ => _progressBarDataList != null && !Game.IsPaused && Game.Player.IsAlive && _progressBarDataList.Any()) //Readだし排他ロックいらないかなという判断
                .Subscribe(_ =>
                {
                    _container.Items.Clear();
                    ProgressBarData[] datas;

                    //ここは排他ロック必要
                    lock (_lockObject)
                    {
                        //完了しているものは除外する
                        _progressBarDataList.RemoveAll(x => x.ProgressBarStatus.IsCompleted);
                        datas = _progressBarDataList.ToArray();
                    }

                    foreach (var progressBarData in datas)
                    {
                        AddProgressBarToContainer(progressBarData);
                    }

                    _container.Draw();
                });
        }

        /// <summary>
        /// ProgressBarを描画登録
        /// </summary>
        public new void RegisterProgressBar(ProgressBarData data)
        {
            lock (_lockObject)
            {
                _progressBarDataList.Add(data);
            }
        }

        /// <summary>
        /// プログレスバーの描画コンテナを追加
        /// </summary>
        private void AddProgressBarToContainer(ProgressBarData data)
        {
            var pos = data.Position;
            var width = data.Width;
            var height = data.Height;
            var margin = data.Mergin;

            var barLength = 0;
            var barPosition = default(Point);
            var barSize = default(Size);

            switch (data.DrawType)
            {
                case DrawType.RightToLeft:
                    barLength = (int)(width * data.ProgressBarStatus.Rate);
                    barPosition = new Point(pos.X, pos.Y);
                    barSize = new Size(barLength, height);
                    break;

                case DrawType.LeftToRight:
                    barLength = (int)(width * data.ProgressBarStatus.Rate);
                    barPosition = new Point(pos.X + width - barLength, pos.Y);
                    barSize = new Size(barLength, height);
                    break;

                case DrawType.TopToBottom:
                    barLength = (int)(height * data.ProgressBarStatus.Rate);
                    barPosition = new Point(pos.X, pos.Y + height - barLength);
                    barSize = new Size(width, barLength);
                    break;

                case DrawType.BottomToTop:
                    barLength = (int)(height * data.ProgressBarStatus.Rate);
                    barPosition = new Point(pos.X, pos.Y);
                    barSize = new Size(width, barLength);
                    break;
            }

            _container.Items.Add(new UIRectangle(new Point(pos.X - margin, pos.Y - margin),
                new Size(width + margin * 2, height + margin * 2), data.BackgorondColor));
            _container.Items.Add(new UIRectangle(barPosition, barSize, data.MainColor));
        }
    }
}