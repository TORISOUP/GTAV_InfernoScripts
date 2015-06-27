using System;
using GTA;
using GTA.Native;

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
            return ped.IsSafeExist() && ped.IsAlive && !ped.IsPlayer && !ped.IsNotChaosPed() && IsChaosableMissionCharacter(ped);
        }

        /// <summary>
        /// 武器を取り替えて良いか
        /// </summary>
        /// <param name="ped">市民</param>
        /// <returns>trueで武器を取り替えて良い</returns>
        public bool IsPedChangebalWeapon(Ped ped)
        {
            return ped.IsSafeExist() && ped.IsAlive && !ped.IsPlayer 
                && (!ped.IsRequiredForMission() || IsChangeMissonCharacterWeapon);
        }

        /// <summary>
        /// 対象の市民がユニークキャラであるか
        /// </summary>
        /// <param name="pedHash">ハッシュ値</param>
        /// <returns>trueでユニークキャラ</returns>
        public bool IsUniqueCharacter(uint pedHash)
        {
            if (!Enum.IsDefined(typeof (PedHash), pedHash))
            {
                //判定できない時はfalseにする
                return false;
            }

            var pedType = (PedHash)pedHash;

            //プレイヤキャラならtrue
            if (pedType == PedHash.Michael || pedType == PedHash.Franklin || pedType == PedHash.Trevor)
            {
                return true;
            }

            //IG/CSから始まるものはユニークキャラ（？）
            var pedTypeName = pedType.ToString();
            return pedTypeName.StartsWith("Ig") || pedTypeName.StartsWith("Cs");
        }


        /// <summary>
        /// 対象の市民がミッションキャラだった時にカオス化してよいか判定する
        /// </summary>
        /// <param name="ped">市民</param>
        /// <returns>trueでカオス化</returns>
        private bool IsChaosableMissionCharacter(Entity ped)
        {
            if (!ped.IsSafeExist()) return false;
            //ミッションキャラじゃないならtrue
            if (!ped.IsRequiredForMission()) return true;

            switch (MissionCharacterTreatment)
            {
                case MissionCharacterTreatmentType.AffectAllCharacter:
                    return true;
                case MissionCharacterTreatmentType.ExcludeUniqueCharacter:
                    return !IsUniqueCharacter((uint)ped.Model.Hash); //ユニークキャラじゃないならカオス化
                case MissionCharacterTreatmentType.ExcludeAllMissionCharacter:
                    return false;
                default:
                    return false;
            }

        }

    }
}
