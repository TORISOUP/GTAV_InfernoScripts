using System.Drawing;

namespace Inferno
{
    /// <summary>
    /// バーの描画方向
    /// </summary>
    public enum DrawType
    {
        RightToLeft,
        LeftToRight,
        TopToBottom,
        BottomToTop
    }

    /// <summary>
    /// 描画に使うデータ詰めたやつ
    /// </summary>
    public class ProgressBarData
    {
        public ProgressBarData(IProgressBar barStatus,
            Point position,
            Color mainColor,
            Color backGroundColor,
            DrawType drawType,
            int width,
            int height,
            int mergin = 10)
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

        public Point Position { get; }
        public Color MainColor { get; }
        public Color BackgorondColor { get; }
        public int Width { get; }
        public int Height { get; }
        public IProgressBar ProgressBarStatus { get; }
        public int Mergin { get; }
        public DrawType DrawType { get; }
    }
}