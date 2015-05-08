using System;
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
        public MissionCharacterTreatmentType MissionCharacterTreatment { get; set; }


        public CharacterChaosChecker(MissionCharacterTreatmentType missionCharacterTreatment, bool isChangeMissonCharacterWeapon)
        {
            MissionCharacterTreatment = missionCharacterTreatment;
            IsChangeMissonCharacterWeapon = isChangeMissonCharacterWeapon;
        }

        /// <summary>
        /// カオス化対象であるか
        /// </summary>
        /// <param name="ped">市民</param>
        /// <returns>trueでカオス化して良い</returns>
        public bool IsPedChaosAvailable(Ped ped)
        {
            return ped.IsSafeExist() && ped.IsAlive && !ped.IsPlayer && IsChaosableMissionCharacter(ped);
        }

        /// <summary>
        /// 武器を取り替えて良いか
        /// </summary>
        /// <param name="ped">市民</param>
        /// <returns>trueで武器を取り替えて良い</returns>
        public bool IsPedChangebalWeapon(Ped ped)
        {
            return ped.IsSafeExist() && ped.IsAlive && !ped.IsPlayer 
                && (!ped.IsPersistent || IsChangeMissonCharacterWeapon);
        }

        /// <summary>
        /// 対象の市民がユニークキャラであるか
        /// </summary>
        /// <param name="ped"></param>
        /// <returns>trueでユニークキャラ</returns>
        public static bool IsUniqueCharacter(int pedHash)
        {
            if (!Enum.IsDefined(typeof (PedList), pedHash))
            {
                //判定できない時はfalseにする
                return false;
            }

            var pedType = (PedList) pedHash;

            //IG_から始まるものはユニークキャラ（？）
            return pedType.ToString().Contains("IG_");
        }


        /// <summary>
        /// 対象の市民がミッションキャラだった時にカオス化してよいか判定する
        /// </summary>
        /// <param name="ped">市民</param>
        /// <returns>trueでカオス化</returns>
        private bool IsChaosableMissionCharacter(Ped ped)
        {
            //ミッションキャラじゃないならtrue
            if (!ped.IsPersistent) return true;

            switch (MissionCharacterTreatment)
            {
                case MissionCharacterTreatmentType.AllCharacterToChaos:
                    return true;
                case MissionCharacterTreatmentType.ExcludeUniqueCharacter:
                    return !IsUniqueCharacter(ped.GetHashCode()); //ユニークキャラじゃないならカオス化
                case MissionCharacterTreatmentType.ExcludeAllMissionCharacter:
                    return false;
                default:
                    return false;
            }

        }

    }
}
