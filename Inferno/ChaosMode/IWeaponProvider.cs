namespace Inferno.ChaosMode
{
    internal interface IWeaponProvider
    {
        /// <summary>
        /// 遠距離攻撃系の武器を取得する
        /// </summary>
        Weapon GetRandomWeaponExcludeClosedWeapon();

        /// <summary>
        /// ドライブバイ用の武器を取得する
        /// </summary>
        Weapon GetRandomDriveByWeapon();

        /// <summary>
        /// 射撃系の武器であるか
        /// </summary>
        bool IsShootWeapon(Weapon weapon);

        /// <summary>
        /// 近接系の武器であるか
        /// </summary>
        bool IsCloseWeapon(Weapon weapon);

        /// <summary>
        /// 投擲用の武器であるか
        /// </summary>
        bool IsProjectileWeapon(Weapon weapon);
    }
}
