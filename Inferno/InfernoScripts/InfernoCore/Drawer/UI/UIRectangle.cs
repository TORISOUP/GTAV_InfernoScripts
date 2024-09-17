//
// Copyright (C) 2015 crosire & kagikn & contributors
// License: https://github.com/scripthookvdotnet/scripthookvdotnet#license
//

using System;
using System.Drawing;
using GTA.Native;
using GTA.UI;

namespace GTA
{
    public class UIRectangle : IElement
    {
        public UIRectangle(Point position, Size size, Color color)
        {
            Enabled = true;
            Position = position;
            Size = size;
            Color = color;
        }

        public Size Size { get; set; }

        public void ScaledDraw(SizeF offset)
        {
            throw new NotImplementedException();
        }

        public virtual bool Enabled { get; set; }
        public PointF Position { get; set; }
        public bool Centered { get; set; }
        public virtual Color Color { get; set; }

        public virtual void Draw()
        {
            Draw(SizeF.Empty);
        }

        public void ScaledDraw()
        {
            Draw(SizeF.Empty);
        }

        public virtual void Draw(SizeF offset)
        {
            if (!Enabled)
            {
                return;
            }

            var w = Size.Width / Screen.Width;
            var h = Size.Height / Screen.Height;
            var x = (Position.X + offset.Width) / Screen.Width + w * 0.5f;
            var y = (Position.Y + offset.Height) / Screen.Height + h * 0.5f;

            Function.Call(Hash.DRAW_RECT, x, y, w, h, Color.R, Color.G, Color.B, Color.A);
        }
    }
}