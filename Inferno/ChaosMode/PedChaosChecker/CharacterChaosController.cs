using GTA;
using System.Linq;
using System.Reactive.Linq;
using System;
using System.Reactive;
using System.Reactive.Subjects;


namespace Inferno.ChaosMode
{
    /// <summary>
    /// スクリプト的にカオス化を行いたくない市民の設定管理
    /// </summary>
    public static class CharacterChaosController
    {
        /// <summary>
        /// スクリプトからカオス化を止めたい市民を設定する
        /// </summary>
        /// <param name="ped">市民</param>
        /// <param name="toggle">trueでカオス化しない</param>
        public static void SetNotChaosPed(this Ped ped, bool toggle)
        {
            if (ped.IsSafeExist())
            {
                //所持金が555だとカオス化しないキャラクタという意味にする
                ped.SetPedMoney(toggle ? 555 : 0);
            }
        }

        /// <summary>
        /// スクリプト的にカオス化したくない市民であるか
        /// </summary>
        /// <param name="ped">市民</param>
        /// <returns>trueでカオス化"したくない"市民</returns>
        public static bool IsNotChaosPed(this Ped ped)
        {
            return ped.GetPedMoney() == 555;
        }
    }
}
