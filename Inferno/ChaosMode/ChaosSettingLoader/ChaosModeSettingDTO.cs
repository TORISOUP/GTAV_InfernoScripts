﻿using System;

namespace Inferno.ChaosMode
{
    /// <summary>
    /// jsonの読み込み結果をマッピングするDTO
    /// </summary>
    [Serializable]
    public class ChaosModeSettingDTO
    {
        public int Radius = 500;
        public bool IsChangeMissionCharacterWeapon = true;
        public int DefaultMissionCharacterTreatment = 1;
        public bool IsAttackPlayerCorrectionEnabled = false;
        public int AttackPlayerCorrectionProbability = 100;
        public string[] WeaponList = new[] { "" };
        public string[] WeaponListForDriveBy = new[] { "" };
        public int StupidPedRate = 50;
        public int ShootAccuracy = 10;
        public int WeaponChangeProbability = 100;
        public int ForceExplosiveWeaponProbability = 30;
    }
}