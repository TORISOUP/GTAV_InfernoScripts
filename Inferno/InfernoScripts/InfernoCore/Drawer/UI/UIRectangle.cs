//
// Copyright (C) 2015 crosire & kagikn & contributors
// License: https://github.com/scripthookvdotnet/scripthookvdotnet#license
//

using GTA.Native;
using System.Drawing;
using GTA.UI;

namespace GTA
{
    public class UIRectangle : IElement
    {
        public UIRectangle() : this(new Point(), new Size((int)UI.Screen.Width, (int)UI.Screen.Height),
            Color.Transparent)
        {
        }

        public UIRectangle(Point position, Size size) : this(position, size, Color.Transparent)
        {
        }

        public UIRectangle(Point position, Size size, Color color)
        {
            Enabled = true;
            Position = position;
            Size = size;
            Color = color;
        }

        public void ScaledDraw(SizeF offset)
        {
            throw new System.NotImplementedException();
        }

        public virtual bool Enabled { get; set; }
        public PointF Position { get; set; }
        public bool Centered { get; set; }
        public Size Size { get; set; }
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

            float w = Size.Width / UI.Screen.Width;
            float h = Size.Height / UI.Screen.Height;
            float x = (Position.X + offset.Width) / UI.Screen.Width + w * 0.5f;
            float y = (Position.Y + offset.Height) / UI.Screen.Height + h * 0.5f;

            Function.Call(Hash.DRAW_RECT, x, y, w, h, Color.R, Color.G, Color.B, Color.A);
        }
    }
}