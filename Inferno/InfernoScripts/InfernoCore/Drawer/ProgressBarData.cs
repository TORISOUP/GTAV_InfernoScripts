using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inferno
{
    /// <summary>
    /// バーの描画方向
    /// </summary>
    public enum DrawType
    {
        RightToLeft,
        LeftToRight,
        UpToBottom,
        BottomToUp
    }

    /// <summary>
    /// 描画に使うデータ詰めたやつ
    /// </summary>
    public class ProgressBarData
    {
        public Point Position { get; }
        public Color MainColor { get; }
        public Color BackgorondColor { get; }
        public int Width { get; }
        public int Height { get; }
        public IProgressBar ProgressBarStatus { get; }
        public int Mergin { get; }
        public DrawType DrawType { get; }

        public ProgressBarData(IProgressBar barStatus, Point position, Color mainColor, Color backGroundColor,
            DrawType drawType, int width, int height, int mergin = 10)
        {
            Position = position;
            MainColor = mainColor;
            BackgorondColor = backGroundColor;
            ProgressBarStatus = barStatus;
            Width = width;
            Height = height;
            Mergin = mergin;
            DrawType = drawType;
        }
    }
}
