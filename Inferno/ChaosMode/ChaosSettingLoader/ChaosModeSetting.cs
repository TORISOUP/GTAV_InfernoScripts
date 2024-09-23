using System;
using System.Collections.Generic;
using System.Linq;
using Inferno.Utilities;

namespace Inferno.ChaosMode
{
    /// <summary>
    /// カオスモード用設定ファイル
    /// </summary>
    public class ChaosModeSetting
    {
        private int _radius;

        /// <summary>
        /// カオスモードの有効半径
        /// </summary>
        public int Radius
        {
            get => _radius;
            set => _radius = value.Clamp(1, 3000);
        }

        /// <summary>
        /// ミッションキャラクタの武器を上書きするか（DefaultMissionCharacterTreatment設定は無視して上書きされる）
        /// </summary>
        public bool OverrideMissionCharacterWeapon { get; set; }

        /// <summary>
        /// ミッションキャラクタの扱い
        /// </summary>
        public MissionCharacterBehaviour MissionCharacterBehaviour { get; set; }

        /// <summary>
        /// 市民がプレイヤを優先して狙うようにするか
        /// </summary>
        public bool IsAttackPlayerCorrectionEnabled { get; set; }

        /// <summary>
        /// 市民がプレイヤをどれくらいの割合で狙ってくるか（0～100%）
        /// IsAttackPlayerCorrectionEnabledがTrueの場合のみ有効
        /// </summary>
        public int AttackPlayerCorrectionProbability { get; set; }

        /// <summary>
        /// 乗車中ではない市民が使用する武器リスト
        /// </summary>
        public HashSet<Weapon> WeaponList { get; set; }

        /// <summary>
        /// ドライブバイで使用する武器リスト
        /// </summary>
        public HashSet<Weapon> WeaponListForDriveBy { get; set; }

        /// <summary>
        /// 棒立ちで銃を乱射する割合
        /// </summary>
        public int StupidShootingRate { get; set; }

        /// <summary>
        /// 攻撃の命中精度(0-100%)
        /// </summary>
        public int ShootAccuracy { get; set; }

        /// <summary>
        /// 市民の武器を変更する確率
        /// </summary>
        public int WeaponChangeProbability { get; set; }

        /// <summary>
        /// 爆発系武器が強制的に選択される確率
        /// </summary>
        public int ForceExplosiveWeaponProbability { get; set; }

        /// <summary>
        /// 市民が武器を落とす確率
        /// </summary>
        public int WeaponDropProbability { get; set; }

        /// <summary>
        /// カオスモード設定ファイルを生成
        /// </summary>
        /// <param name="dto">DTOから生成する</param>
        public ChaosModeSetting(ChaosModeSettingDTO dto)
        {
            if (dto == null)
            {
                dto = new ChaosModeSettingDTO();
            }

            Radius = dto.Radius;
            OverrideMissionCharacterWeapon = dto.OverrideMissionCharacterWeapon;
            IsAttackPlayerCorrectionEnabled = dto.IsAttackPlayerCorrectionEnabled;
            StupidShootingRate = dto.StupidShootingRate.Clamp(0, 100);
            AttackPlayerCorrectionProbability = dto.AttackPlayerCorrectionProbability.Clamp(0, 100);
            ShootAccuracy = dto.ShootAccuracy.Clamp(0, 100);
            WeaponChangeProbability = dto.WeaponChangeProbability.Clamp(0, 100);
            ForceExplosiveWeaponProbability = dto.ForceExplosiveWeaponProbability.Clamp(0, 100);
            WeaponDropProbability = dto.WeaponDropProbability.Clamp(0, 100);

            //ミッションキャラクタの扱い
            MissionCharacterBehaviour =
                !Enum.IsDefined(typeof(MissionCharacterBehaviour), dto.MissionCharacterBehaviour)
                    ? MissionCharacterBehaviour.ExcludeUniqueCharacter //定義範囲外の数値ならExcludeUniqueCharacterにする
                    : (MissionCharacterBehaviour)dto.MissionCharacterBehaviour;

            WeaponList = EnableWeaponListFilter(dto.WeaponList);
            WeaponListForDriveBy = EnableWeaponListFilter(dto.WeaponListForDriveBy);
        }

        /// <summary>
        /// weaponlistから有効な武器のみを抽出する
        /// </summary>
        /// <param name="weaponList"></param>
        /// <returns></returns>
        protected HashSet<Weapon> EnableWeaponListFilter(string[] weaponList)
        {
            var allWeapons = ((Weapon[])Enum.GetValues(typeof(Weapon))).ToHashSet();
            if (weaponList == null || weaponList.Length == 0)
            {
                return new HashSet<Weapon>();
            }

            var upperCaseWeaponList = weaponList.Select(x => x.ToUpper()).ToHashSet();

            var enableWeapons = allWeapons.Where(x => upperCaseWeaponList.Contains(x.ToString().ToUpper())).ToHashSet();
            return enableWeapons.Count > 0 ? enableWeapons : allWeapons;
        }
    }
}