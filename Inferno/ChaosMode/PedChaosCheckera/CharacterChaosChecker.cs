using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;

namespace Inferno.ChaosMode
{
    public class CharacterChaosChecker
    {
        /// <summary>
        /// ミッションキャラの武器を変更するか
        /// </summary>
        public bool IsChangeMissonCharacterWeapon { get; set; }

        /// <summary>
        /// ミッションキャラのカオス化
        /// </summary>
        public bool IsChaosMissionCharacter { get; set; }

        /// <summary>
        /// キャラクタがカオス化対象であるかの判定を行う
        /// </summary>
        /// <param name="isChaosMissionCharacter">ミッションキャラもカオス化するか</param>
        /// <param name="isChangeMissonCharacterWeapon">ミッションキャラの武器を変更するか</param>
        public CharacterChaosChecker(bool isChaosMissionCharacter, bool isChangeMissonCharacterWeapon)
        {
            IsChaosMissionCharacter = isChaosMissionCharacter;
            IsChangeMissonCharacterWeapon = isChangeMissonCharacterWeapon;
        }


        public bool IsPedChaosAvailable(Ped ped)
        {

            return ped.IsSafeExist() && ped.IsAlive && !ped.IsPlayer  &&
                   (!ped.IsPersistent || IsChaosMissionCharacter);
        }

        public bool IsPedChangebalWeapon(Ped ped)
        {
            return ped.IsSafeExist() && ped.IsAlive && !ped.IsPlayer 
                && (!ped.IsPersistent || IsChangeMissonCharacterWeapon);
        }

    }
}
