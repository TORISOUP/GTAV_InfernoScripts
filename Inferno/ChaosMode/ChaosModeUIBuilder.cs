using System;
using System.Collections.Generic;
using GTA;
using Inferno.ChaosMode.WeaponProvider;
using Inferno.InfernoScripts.InfernoCore.UI;
using Inferno.Properties;
using LemonUI;
using LemonUI.Menus;

namespace Inferno.ChaosMode
{
    public sealed class ChaosModeUIBuilder : IDisposable
    {
        private readonly ChaosModeSetting _chaosModeSetting;

        public string DisplayName => ChaosModeLocalize.DisplayName;
        public string Description => ChaosModeLocalize.Description;

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
                ChaosModeLocalize.Radius,
                _chaosModeSetting.Radius,
                1000,
                x =>
                {
                    x.Value = _chaosModeSetting.Radius;
                    x.Multiplier = 10;
                }, item =>
                {
                    _chaosModeSetting.Radius = item.Value;
                    item.Title = $"Radius:{_chaosModeSetting.Radius}[m]";
                });


            // IsMissionCharacterChangeWeapon
            subMenu.AddCheckbox(
                "Override Mission Character Weapon",
                ChaosModeLocalize.OverrideMissionCharacterWeapon,
                item => { item.Checked = _chaosModeSetting.OverrideMissionCharacterWeapon; },
                x => { _chaosModeSetting.OverrideMissionCharacterWeapon = x; });

            // MissionCharacterBehaviour
            subMenu.AddEnumSlider(
                "Mission Character Behaviour: " + _chaosModeSetting.MissionCharacterBehaviour,
                ChaosModeLocalize.MissionCharacterBehaviour,
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
                "Attack Players Correction",
                ChaosModeLocalize.AttackPlayerCorrection,
                item => { item.Checked = _chaosModeSetting.IsAttackPlayerCorrectionEnabled; },
                x => { _chaosModeSetting.IsAttackPlayerCorrectionEnabled = x; });

            // AttackPlayerCorrectionProbability
            var attackPlayerCorrectionSlider = subMenu.AddSlider(
                $"Attack Players Correction Probability:{_chaosModeSetting.AttackPlayerCorrectionProbability}%",
                ChaosModeLocalize.AttackPlayerCorrectionProbability,
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
                        $"Attack Players Correction Probability:{_chaosModeSetting.AttackPlayerCorrectionProbability}%";
                });

            isAttackPlayerCorrectionCheckbox.CheckboxChanged += (_, _) =>
            {
                attackPlayerCorrectionSlider.Enabled = isAttackPlayerCorrectionCheckbox.Checked;
            };

            // StupidShootingRate
            subMenu.AddSlider(
                $"Stupid Shooting Rate:{_chaosModeSetting.StupidShootingRate}%",
                ChaosModeLocalize.StupidShootingRate,
                _chaosModeSetting.StupidShootingRate,
                100,
                x =>
                {
                    x.Value = _chaosModeSetting.StupidShootingRate;
                    x.Multiplier = 5;
                }, item =>
                {
                    _chaosModeSetting.StupidShootingRate = item.Value;
                    item.Title = $"Stupid Shooting Rate:{_chaosModeSetting.StupidShootingRate}%";
                });

            // ShootAccuracy
            subMenu.AddSlider(
                $"Shoot Accuracy:{_chaosModeSetting.ShootAccuracy}%",
                ChaosModeLocalize.ShootAccuracy,
                _chaosModeSetting.ShootAccuracy,
                100,
                x =>
                {
                    x.Multiplier = 1;
                    x.Value = _chaosModeSetting.ShootAccuracy;
                }, item =>
                {
                    _chaosModeSetting.ShootAccuracy = item.Value;
                    item.Title = $"Shoot Accuracy:{_chaosModeSetting.ShootAccuracy}%";
                });

            // WeaponChangeProbability
            subMenu.AddSlider(
                $"Weapon Change Probability:{_chaosModeSetting.WeaponChangeProbability}%",
                ChaosModeLocalize.WeaponChangeProbability,
                _chaosModeSetting.WeaponChangeProbability,
                100,
                x =>
                {
                    x.Value = _chaosModeSetting.WeaponChangeProbability;
                    x.Multiplier = 5;
                }, item =>
                {
                    _chaosModeSetting.WeaponChangeProbability = item.Value;
                    item.Title = $"Weapon Change Probability:{_chaosModeSetting.WeaponChangeProbability}%";
                });

            // ForceExplosiveWeaponProbability
            subMenu.AddSlider(
                $"Force Explosive Weapon Probability:{_chaosModeSetting.ForceExplosiveWeaponProbability}%",
                ChaosModeLocalize.ForceExplosiveWeaponProbability,
                _chaosModeSetting.ForceExplosiveWeaponProbability,
                100,
                x =>
                {
                    x.Value = _chaosModeSetting.ForceExplosiveWeaponProbability;
                    x.Multiplier = 5;
                }, item =>
                {
                    _chaosModeSetting.ForceExplosiveWeaponProbability = item.Value;
                    item.Title =
                        $"Force Explosive Weapon Probability:{_chaosModeSetting.ForceExplosiveWeaponProbability}%";
                });


            // WeaponDropProbability
            subMenu.AddSlider(
                $"Weapon Drop Probability:{_chaosModeSetting.WeaponDropProbability}%",
                ChaosModeLocalize.WeaponDropProbability,
                _chaosModeSetting.WeaponDropProbability,
                100,
                x =>
                {
                    x.Value = _chaosModeSetting.WeaponDropProbability;
                    x.Multiplier = 5;
                }, item =>
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


            subMenu.AddCheckbox("Melee Weapon Only", ChaosModeLocalize.Yakyu,
                item => { item.Checked = _chaosModeSetting.MeleeWeaponOnly; },
                x => { _chaosModeSetting.MeleeWeaponOnly = x; });
        }

        private NativeMenu WeaponListMenu()
        {
            var weaponListMenu = new NativeMenu("Weapon List", "Weapon List")
            {
                Visible = false
            };
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
            var weaponListDriveByMenu = new NativeMenu("Weapon List for Drive-By", "Weapon List for Drive-By")
            {
                Visible = false
            };
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