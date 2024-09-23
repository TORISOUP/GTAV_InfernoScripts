using System;
using System.Collections.Generic;
using GTA;
using Inferno.ChaosMode.WeaponProvider;
using Inferno.InfernoScripts.InfernoCore.UI;
using LemonUI;
using LemonUI.Menus;

namespace Inferno.ChaosMode
{
    public sealed class ChaosModeUIBuilder : IDisposable
    {
        private readonly ChaosModeSetting _chaosModeSetting;
        private bool IsLangJpn => Game.Language == Language.Japanese;

        public Action OnChangeWeaponSetting { get; set; }

        public ChaosModeUIBuilder(ChaosModeSetting chaosModeSetting)
        {
            _chaosModeSetting = chaosModeSetting;
        }

        public void OnUiMenuConstruct(ObjectPool pool, NativeMenu subMenu)
        {
            subMenu.Width = 780;

            // Radius
            subMenu.AddSlider(
                $"Radius:{_chaosModeSetting.Radius}[m]",
                IsLangJpn
                    ? "カオス化する市民の探索範囲"
                    : "Search range of citizens to be riot",
                _chaosModeSetting.Radius,
                1000,
                x => x.Multiplier = 10, item =>
                {
                    _chaosModeSetting.Radius = item.Value;
                    item.Title = $"Radius:{_chaosModeSetting.Radius}[m]";
                });

            // IsMissionCharacterChangeWeapon
            subMenu.AddCheckbox(
                "Override Mission Character Weapon",
                IsLangJpn
                    ? "ミッション関係キャラクターの武器を上書きするか\nMissionCharacterBehaviourの設定とは独立して機能します"
                    : "Override mission character's weapons.\nWorks independently of 'MissionCharacterBehaviour' settings",
                item => { item.Checked = _chaosModeSetting.OverrideMissionCharacterWeapon; },
                x => { _chaosModeSetting.OverrideMissionCharacterWeapon = x; });

            // MissionCharacterBehaviour
            subMenu.AddEnumSlider(
                "MissionCharacterBehaviour: " + _chaosModeSetting.MissionCharacterBehaviour,
                IsLangJpn ? "ミッション関係キャラクターに影響を与えるか" : "Whether it affects mission-related characters",
                _chaosModeSetting.MissionCharacterBehaviour,
                x =>
                {
                    x.Title = "MissionCharacterBehaviour :" + _chaosModeSetting.MissionCharacterBehaviour;
                    x.Value = (int)_chaosModeSetting.MissionCharacterBehaviour;
                }, x =>
                {
                    _chaosModeSetting.MissionCharacterBehaviour = (MissionCharacterBehaviour)x.Value;
                    x.Title = "MissionCharacterBehaviour: " + _chaosModeSetting.MissionCharacterBehaviour;
                });

            // IsAttackPlayerCorrectionEnabled
            var isAttackPlayerCorrectionCheckbox = subMenu.AddCheckbox(
                "Attack Player Correction",
                IsLangJpn
                    ? "プレイヤーの狙われやすさを補正するか\nOFFの場合は市民との位置関係に応じてプレイヤーが狙われます"
                    : "Adjust the player's targetability.\nIf OFF, the player will be targeted depending on the position relationship with the ped",
                item => { item.Checked = _chaosModeSetting.IsAttackPlayerCorrectionEnabled; },
                x => { _chaosModeSetting.IsAttackPlayerCorrectionEnabled = x; });

            // AttackPlayerCorrectionProbability
            var attackPlayerCorrectionSlider = subMenu.AddSlider(
                $"Attack Player Correction Probability:{_chaosModeSetting.AttackPlayerCorrectionProbability}%",
                IsLangJpn
                    ? "プレイヤーが狙われる確率"
                    : "Probability of player being targeted",
                _chaosModeSetting.AttackPlayerCorrectionProbability,
                100,
                x =>
                {
                    x.Enabled = _chaosModeSetting.IsAttackPlayerCorrectionEnabled;
                    x.Multiplier = 5;
                }, item =>
                {
                    _chaosModeSetting.AttackPlayerCorrectionProbability = item.Value;
                    item.Title =
                        $"Attack Player Correction Probability:{_chaosModeSetting.AttackPlayerCorrectionProbability}%";
                });

            isAttackPlayerCorrectionCheckbox.CheckboxChanged += (_, _) =>
            {
                attackPlayerCorrectionSlider.Enabled = isAttackPlayerCorrectionCheckbox.Checked;
            };

            // StupidShootingRate
            subMenu.AddSlider(
                $"Stupid Shooting Rate:{_chaosModeSetting.StupidShootingRate}%",
                IsLangJpn
                    ? "市民が射線が通っているかを無視して銃を乱射する割合"
                    : "Probability of peds firing a gun without regard to whether the line of fire is through or not",
                _chaosModeSetting.StupidShootingRate,
                100,
                x => x.Multiplier = 5, item =>
                {
                    _chaosModeSetting.StupidShootingRate = item.Value;
                    item.Title = $"Stupid Shooting Rate:{_chaosModeSetting.StupidShootingRate}%";
                });

            // ShootAccuracy
            subMenu.AddSlider(
                $"Shoot Accuracy:{_chaosModeSetting.ShootAccuracy}%",
                IsLangJpn
                    ? "市民の攻撃の命中精度"
                    : "Accuracy of ped's attack",
                _chaosModeSetting.ShootAccuracy,
                100,
                x => x.Multiplier = 1, item =>
                {
                    _chaosModeSetting.ShootAccuracy = item.Value;
                    item.Title = $"Shoot Accuracy:{_chaosModeSetting.ShootAccuracy}%";
                });

            // WeaponChangeProbability
            subMenu.AddSlider(
                $"Weapon Change Probability:{_chaosModeSetting.WeaponChangeProbability}%",
                IsLangJpn
                    ? "市民が武器を変更する確率"
                    : "Probability of peds changing weapon",
                _chaosModeSetting.WeaponChangeProbability,
                100,
                x => x.Multiplier = 5, item =>
                {
                    _chaosModeSetting.WeaponChangeProbability = item.Value;
                    item.Title = $"Weapon Change Probability:{_chaosModeSetting.WeaponChangeProbability}%";
                });

            // ForceExplosiveWeaponProbability
            subMenu.AddSlider(
                $"Force Explosive Weapon Probability:{_chaosModeSetting.ForceExplosiveWeaponProbability}%",
                IsLangJpn
                    ? "爆発系武器が強制的に選択される確率"
                    : "Probability of explosive weapons being forcibly selected",
                _chaosModeSetting.ForceExplosiveWeaponProbability,
                100,
                x => x.Multiplier = 5, item =>
                {
                    _chaosModeSetting.ForceExplosiveWeaponProbability = item.Value;
                    item.Title =
                        $"Force Explosive Weapon Probability:{_chaosModeSetting.ForceExplosiveWeaponProbability}%";
                });

            // WeaponDropProbability
            subMenu.AddSlider(
                $"Weapon Drop Probability:{_chaosModeSetting.WeaponDropProbability}%",
                IsLangJpn
                    ? "市民が武器を落とす確率"
                    : "Probability of peds dropping weapon",
                _chaosModeSetting.WeaponDropProbability,
                100,
                x => x.Multiplier = 5, item =>
                {
                    _chaosModeSetting.WeaponDropProbability = item.Value;
                    item.Title = $"Weapon Drop Probability:{_chaosModeSetting.WeaponDropProbability}%";
                });

            // WeaponList
            var weaponListMenu = WeaponListMenu();
            subMenu.AddSubMenu(weaponListMenu);
            pool.Add(weaponListMenu);
            
            // WeaponListForDriveBy
            var weaponListForDriveByMenu = DriveByWeaponListMenu();
            subMenu.AddSubMenu(weaponListForDriveByMenu);
            pool.Add(weaponListForDriveByMenu);
        }

        private NativeMenu WeaponListMenu()
        {
            var weaponListMenu = new NativeMenu("Weapon List", "Weapon List");
            var isChanged = false;

            // ボタンだけ先に追加
            var allOnButton = new NativeItem("All ON", "");
            weaponListMenu.Add(allOnButton);

            var allOffButton = new NativeItem("All OFF", "");
            weaponListMenu.Add(allOffButton);

            var list = new List<NativeCheckboxItem>();

            foreach (var weapon in ChaosModeWeapons.AllWeapons)
            {
                var checkbox = weaponListMenu.AddCheckbox(
                    weapon.ToString(),
                    weapon.ToString(),
                    item => { item.Checked = _chaosModeSetting.WeaponList.Contains(weapon); },
                    x =>
                    {
                        isChanged = true;
                        if (x)
                        {
                            _chaosModeSetting.WeaponList.Add(weapon);
                        }
                        else
                        {
                            _chaosModeSetting.WeaponList.Remove(weapon);
                        }
                    });
                list.Add(checkbox);
            }

            weaponListMenu.Opening += (_, _) => { isChanged = false; };

            weaponListMenu.Closed += (_, _) =>
            {
                if (isChanged)
                {
                    OnChangeWeaponSetting?.Invoke();
                }
            };

            allOnButton.Activated += (_, _) =>
            {
                isChanged = true;
                _chaosModeSetting.WeaponList.Clear();
                _chaosModeSetting.WeaponList.UnionWith(ChaosModeWeapons.AllWeapons);
                foreach (var item in list)
                {
                    item.Checked = true;
                }
            };
            allOffButton.Activated += (_, _) =>
            {
                isChanged = true;
                _chaosModeSetting.WeaponList.Clear();
                foreach (var item in list)
                {
                    item.Checked = false;
                }
            };


            return weaponListMenu;
        }

        private NativeMenu DriveByWeaponListMenu()
        {
            var weaponListDriveByMenu = new NativeMenu("Weapon List for Drive-By", "Weapon List for Drive-By");
            var isChanged = false;

            // ボタンだけ先に追加
            var allOnButton = new NativeItem("All ON", "");
            weaponListDriveByMenu.Add(allOnButton);

            var allOffButton = new NativeItem("All OFF", "");
            weaponListDriveByMenu.Add(allOffButton);
            
            var filterButton = new NativeItem("Filter by main weapon list", "");
            weaponListDriveByMenu.Add(filterButton);

            var list = new Dictionary<Weapon, NativeCheckboxItem>();
            
            // 各チェックボックス生成
            foreach (var weapon in ChaosModeWeapons.DriveByWeapons)
            {
                var checkbox = weaponListDriveByMenu.AddCheckbox(
                    weapon.ToString(),
                    weapon.ToString(),
                    item => { item.Checked = _chaosModeSetting.WeaponListForDriveBy.Contains(weapon); },
                    x =>
                    {
                        isChanged = true;
                        if (x)
                        {
                            _chaosModeSetting.WeaponListForDriveBy.Add(weapon);
                        }
                        else
                        {
                            _chaosModeSetting.WeaponListForDriveBy.Remove(weapon);
                        }
                    });
                list[weapon] = checkbox;
            }

            // 閉じるときにイベント発行
            weaponListDriveByMenu.Opening += (_, _) => { isChanged = false; };
            weaponListDriveByMenu.Closed += (_, _) =>
            {
                if (isChanged)
                {
                    OnChangeWeaponSetting?.Invoke();
                }
            };

            // ボタンを押したときの操作
            allOnButton.Activated += (_, _) =>
            {
                isChanged = true;
                _chaosModeSetting.WeaponListForDriveBy.Clear();
                _chaosModeSetting.WeaponListForDriveBy.UnionWith(ChaosModeWeapons.DriveByWeapons);
                foreach (var item in list)
                {
                    item.Value.Checked = true;
                }
            };
            allOffButton.Activated += (_, _) =>
            {
                isChanged = true;
                _chaosModeSetting.WeaponListForDriveBy.Clear();
                foreach (var item in list)
                {
                    item.Value.Checked = false;
                }
            };

            filterButton.Activated += (_, _) =>
            {
                // WeaponListForDriveByのうち、WeaponListに含まれるものだけを残す
                _chaosModeSetting.WeaponListForDriveBy
                    .RemoveWhere(x => !_chaosModeSetting.WeaponList.Contains(x));
                
                foreach (var item in list)
                {
                    item.Value.Checked = _chaosModeSetting.WeaponListForDriveBy.Contains(item.Key);
                }
            };
            
            return weaponListDriveByMenu;
        }

        public void Dispose()
        {
        }
    }
}