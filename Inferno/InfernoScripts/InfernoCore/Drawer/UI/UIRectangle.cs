//
// Copyright (C) 2015 crosire & kagikn & contributors
// License: https://github.com/scripthookvdotnet/scripthookvdotnet#license
//

using GTA.Native;
using System.Drawing;

namespace GTA
{
    public class UIRectangle : UIElement
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

        public virtual bool Enabled { get; set; }
        public virtual Point Position { get; set; }
        public Size Size { get; set; }
        public virtual Color Color { get; set; }

        public virtual void Draw()
        {
            Draw(new Size());
        }

        public virtual void Draw(Size offset)
        {
            if (!Enabled)
            {
                return;
            }

            float w = (float)Size.Width / UI.Screen.Width;
            float h = (float)Size.Height / UI.Screen.Height;
            float x = (float)(Position.X + offset.Width) / UI.Screen.Width + w * 0.5f;
            float y = (float)(Position.Y + offset.Height) / UI.Screen.Height + h * 0.5f;

            Function.Call(Hash.DRAW_RECT, x, y, w, h, Color.R, Color.G, Color.B, Color.A);
        }
    }
}