using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using GTA.Native;

namespace Inferno.ChaosMode
{
    /// <summary>
    /// カオスモード用設定ファイル
    /// </summary>
    public class ChaosModeSetting
    {
        public int Radius { get; private set; }
        public int Interval { get; private set; }
        public bool IsChangeMissionCharacterWeapon { get; private set; }
        public MissionCharacterTreatmentType DefaultMissionCharacterTreatment { get; private set; }
        public bool IsAttackPlayerCorrectionEnabled { get; private set; }
        public int AttackPlayerCorrectionProbabillity { get; private set; }
        public Weapon[] WeaponList { get; private set; }
        public bool IsStupidShooting { get; private set; }
        public int ShootAccuracy { get; private set; }

        /// <summary>
        /// カオスモード設定ファイルを生成
        /// </summary>
        /// <param name="dto">DTOから生成する</param>
        public ChaosModeSetting(ChaosModeSettingDTO dto)
        {
            if (dto == null) { dto = new ChaosModeSettingDTO();}

            //バリデーション処理
            Radius = dto.Radius.Clamp(1, 3000);
            Interval = dto.Interval.Clamp(10, 60000);
            IsChangeMissionCharacterWeapon = dto.IsChangeMissionCharacterWeapon;
            IsAttackPlayerCorrectionEnabled = dto.IsAttackPlayerCorrectionEnabled;
            IsStupidShooting = dto.IsStupidShooting;
            AttackPlayerCorrectionProbabillity = dto.AttackPlayerCorrectionProbabillity.Clamp(0, 100);
            ShootAccuracy = dto.ShootAccuracy.Clamp(0, 100);

            //ミッションキャラクタの扱い
            DefaultMissionCharacterTreatment =
                !Enum.IsDefined(typeof (MissionCharacterTreatmentType), dto.DefaultMissionCharacterTreatment)
                    ? MissionCharacterTreatmentType.ExcludeUniqueCharacter //定義範囲外の数値ならExcludeUniqueCharacterにする
                    : (MissionCharacterTreatmentType) dto.DefaultMissionCharacterTreatment;

            WeaponList = EnableWeaponListFilter(dto.WeaponList);
        }

        /// <summary>
        /// weaponlistから有効な武器のみを抽出する
        /// weaponListが空の場合は全ての武器リストとして返す
        /// </summary>
        /// <param name="weaponList"></param>
        /// <returns></returns>
        protected Weapon[] EnableWeaponListFilter(string[] weaponList)
        {
            var allWeapons = ((Weapon[]) Enum.GetValues(typeof (Weapon)));
            if (weaponList == null || weaponList.Length == 0)
            {
                return allWeapons;
            }
            var enableWeapons = allWeapons.Where(x => weaponList.Contains(x.ToString())).ToArray();
            return enableWeapons.Length > 0 ? enableWeapons : allWeapons;
        }
    }
}
