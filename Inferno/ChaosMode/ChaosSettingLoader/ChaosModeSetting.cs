using System;
using System.Linq;

namespace Inferno.ChaosMode
{
    /// <summary>
    /// カオスモード用設定ファイル
    /// </summary>
    public class ChaosModeSetting
    {
        /// <summary>
        /// カオスモード設定ファイルを生成
        /// </summary>
        /// <param name="dto">DTOから生成する</param>
        public ChaosModeSetting(ChaosModeSettingDTO dto)
        {
            if (dto == null) dto = new ChaosModeSettingDTO();

            //バリデーション処理
            Radius = dto.Radius.Clamp(1, 3000);
            IsChangeMissionCharacterWeapon = dto.IsChangeMissionCharacterWeapon;
            IsAttackPlayerCorrectionEnabled = dto.IsAttackPlayerCorrectionEnabled;
            AttackPlayerCorrectionProbabillity = dto.AttackPlayerCorrectionProbabillity.Clamp(0, 100);
            ShootAccuracy = dto.ShootAccuracy.Clamp(0, 100);
            WeaponChangeProbabillity = dto.WeaponChangeProbabillity.Clamp(0, 100);

            //ミッションキャラクタの扱い
            DefaultMissionCharacterTreatment =
                !Enum.IsDefined(typeof(MissionCharacterTreatmentType), dto.DefaultMissionCharacterTreatment)
                    ? MissionCharacterTreatmentType.ExcludeUniqueCharacter //定義範囲外の数値ならExcludeUniqueCharacterにする
                    : (MissionCharacterTreatmentType)dto.DefaultMissionCharacterTreatment;

            WeaponList = EnableWeaponListFilter(dto.WeaponList);
            WeaponListForDriveBy = EnableWeaponListFilter(dto.WeaponListForDriveBy);
        }

        /// <summary>
        /// カオスモードの有効半径
        /// </summary>
        public int Radius { get; private set; }

        /// <summary>
        /// 市民をカオス化する間隔
        /// </summary>
        public int Interval { get; private set; }

        /// <summary>
        /// ミッションキャラクタの武器を上書きするか（DefaultMissionCharacterTreatment設定は無視して上書きされる）
        /// </summary>
        public bool IsChangeMissionCharacterWeapon { get; private set; }

        /// <summary>
        /// ミッションキャラクタの扱い
        /// </summary>
        public MissionCharacterTreatmentType DefaultMissionCharacterTreatment { get; private set; }

        /// <summary>
        /// 市民がプレイヤを優先して狙うようにするか
        /// </summary>
        public bool IsAttackPlayerCorrectionEnabled { get; private set; }

        /// <summary>
        /// 市民がプレイヤをどれくらいの割合で狙ってくるか（0～100%）
        /// IsAttackPlayerCorrectionEnabledがTrueの場合のみ有効
        /// </summary>
        public int AttackPlayerCorrectionProbabillity { get; private set; }

        /// <summary>
        /// 乗車中ではない市民が使用する武器リスト
        /// </summary>
        public Weapon[] WeaponList { get; private set; }

        /// <summary>
        /// ドライブバイで使用する武器リスト
        /// </summary>
        public Weapon[] WeaponListForDriveBy { get; private set; }
        
        /// <summary>
        /// 攻撃の命中精度(0-100%)
        /// </summary>
        public int ShootAccuracy { get; private set; }

        /// <summary>
        /// 市民の武器を変更する確率
        /// </summary>
        public int WeaponChangeProbabillity { get; private set; }

        /// <summary>
        /// weaponlistから有効な武器のみを抽出する
        /// </summary>
        /// <param name="weaponList"></param>
        /// <returns></returns>
        protected Weapon[] EnableWeaponListFilter(string[] weaponList)
        {
            var allWeapons = (Weapon[])Enum.GetValues(typeof(Weapon));
            if (weaponList == null || weaponList.Length == 0) return new Weapon[0];
            var enableWeapons = allWeapons.Where(x => weaponList.Contains(x.ToString())).ToArray();
            return enableWeapons.Length > 0 ? enableWeapons : allWeapons;
        }
    }
}