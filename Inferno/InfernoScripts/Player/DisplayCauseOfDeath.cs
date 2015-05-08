using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;

namespace Inferno
{
    /// <summary>
    /// 死因表示
    /// </summary>
    class DisplayCauseOfDeath : InfernoScript
    {
        protected override int TickInterval
        {
            get { return 300; }
        }

        protected override void Setup()
        {
            //TODO: 死亡時に画面に表示する
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
