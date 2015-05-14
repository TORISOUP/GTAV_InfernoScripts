using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Native;

namespace Inferno
{
    /// <summary>
    /// 死因表示
    /// </summary>
    class DisplayCauseOfDeath : InfernoScript
    {
        private UIContainer _mContainer;
        private int ScreenHeight;
        private int ScreenWeight;

        protected override int TickInterval
        {
            get { return 300; }
        }

        protected override void Setup()
        {
           
            _mContainer = new UIContainer(new Point(0, 0), new Size(500, 20));
        }

        /// <summary>
        /// 最後にダメージを受けた武器を取得する
        /// </summary>
        /// <param name="ped">市民</param>
        /// <returns>最後に受けたダメージの武器</returns>
        private Weapon? getLastDamageWeapon(Ped ped)
        {
            foreach (Weapon w in Enum.GetValues(typeof(Weapon)))
            {
                if (ped.HasBeenDamagedBy(w))
                {
                    return w;
                }
            }
            return null;
        }
    }
}
