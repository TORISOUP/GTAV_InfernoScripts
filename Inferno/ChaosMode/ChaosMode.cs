﻿using GTA;
using GTA.Native;
using Inferno.ChaosMode.WeaponProvider;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Inferno.InfernoScripts.Event;
using Inferno.InfernoScripts.Event.ChasoMode;
using UniRx;

namespace Inferno.ChaosMode
{
    internal class ChaosMode : InfernoScript
    {
        private readonly string Keyword = "chaos";
        private CharacterChaosChecker chaosChecker;

        /// <summary>
        /// カオス化済み市民一覧
        /// </summary>
        private HashSet<int> chaosedPedList = new HashSet<int>();

        private List<uint> coroutineIds = new List<uint>();

        /// <summary>
        /// WeaponProvider
        /// </summary>
        private IWeaponProvider defaultWeaponProvider;
        private SingleWeaponProvider singleWeaponProvider;

        private IWeaponProvider CurrentWeaponProvider => singleWeaponProvider ?? defaultWeaponProvider;

        readonly private uint[] fishHashes = {
           (uint) PedHash.Fish,
           (uint) PedHash.HammerShark,
           (uint) PedHash.Dolphin,
           (uint) PedHash.Humpback,
           (uint) PedHash.KillerWhale,
           (uint) PedHash.Stingray,
           (uint) PedHash.TigerShark
        };

        /// <summary>
        /// 設定
        /// </summary>
        private ChaosModeSetting chaosModeSetting;

        private MissionCharacterTreatmentType currentTreatType =
            MissionCharacterTreatmentType.ExcludeUniqueCharacter;

        private MissionCharacterTreatmentType nextTreatType;

        private bool _isBaseball = false;

        private int chaosRelationShipId;

        protected override void Setup()
        {
            //敵対関係のグループを作成
            chaosRelationShipId = World.AddRelationshipGroup("Inferno:ChaosPeds");

            var chaosSettingLoader = new ChaosModeSettingLoader();
            chaosModeSetting = chaosSettingLoader.LoadSettingFile(@"ChaosMode_Default.conf");

            chaosChecker = new CharacterChaosChecker(chaosModeSetting.DefaultMissionCharacterTreatment,
                chaosModeSetting.IsChangeMissionCharacterWeapon);

            defaultWeaponProvider = new CustomWeaponProvider(chaosModeSetting.WeaponList, chaosModeSetting.WeaponListForDriveBy);

            //キーワードが入力されたらON／OFFを切り替える
            CreateInputKeywordAsObservable(Keyword)
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    chaosedPedList.Clear();
                    StopAllChaosCoroutine();
                    if (IsActive)
                    {
                        DrawText("ChaosMode:On/" + currentTreatType.ToString(), 3.0f);
                        World.SetRelationshipBetweenGroups(Relationship.Hate, chaosRelationShipId, PlayerPed.RelationshipGroup);
                    }
                    else
                    {
                        DrawText("ChaosMode:Off", 3.0f);
                        World.SetRelationshipBetweenGroups(Relationship.Neutral, chaosRelationShipId, PlayerPed.RelationshipGroup);
                    }
                });

            nextTreatType = currentTreatType;

            //F7でキャラカオスの切り替え（暫定
            OnKeyDownAsObservable
                .Where(x => IsActive && x.KeyCode == Keys.F7)
                .Do(_ =>
                {
                    nextTreatType = (MissionCharacterTreatmentType)(((int)nextTreatType + 1) % 3);
                    DrawText("CharacterChaos:" + nextTreatType.ToString(), 1.1f);
                })
                .Throttle(TimeSpan.FromSeconds(1))
                .Subscribe(_ =>
                {
                    currentTreatType = nextTreatType;
                    chaosChecker.MissionCharacterTreatment = nextTreatType;
                    DrawText("CharacterChaos:" + currentTreatType.ToString() + "[OK]", 3.0f);
                    chaosedPedList.Clear();
                    StopAllChaosCoroutine();
                });

            CreateInputKeywordAsObservable("yakyu")
                .Subscribe(_ =>
                {
                    _isBaseball = !_isBaseball;
                    if (_isBaseball)
                    {
                        DrawText("BaseBallMode:On", 3.0f);
                    }
                    else
                    {
                        DrawText("BaseBallMode:Off", 3.0f);
                    }
                });

            //interval設定
            Interval = chaosModeSetting.Interval;

            var oneSecondTich = CreateTickAsObservable(TimeSpan.FromSeconds(1));

            //市民をカオス化する
            oneSecondTich
                .Where(_ => IsActive && PlayerPed.IsSafeExist() && PlayerPed.IsAlive)
                .Subscribe(_ => CitizenChaos());

            //プレイヤが死んだらリセット
            oneSecondTich
                 .Where(_ => PlayerPed.IsSafeExist())
                .Select(_ => PlayerPed.IsDead)
                .DistinctUntilChanged()
                .Where(x => x)
                .Subscribe(_ =>
                {
                    chaosedPedList.Clear();
                    StopAllChaosCoroutine();
                });

            oneSecondTich
                .Where(_ => IsActive)
                .Subscribe(_ => NativeFunctions.SetAllRandomPedsFlee(Game.Player, false));

            //イベントが来たら武器を変更する
            OnRecievedInfernoEvent
                .OfType<IEventMessage, ChasoModeEvent>()
                .Subscribe(e =>
                {
                    if (e is ChangeToDefaultEvent)
                    {
                        singleWeaponProvider = null;
                    }
                    else if (e is ChangeWeaponEvent)
                    {
                        var s = (ChangeWeaponEvent)e;
                        singleWeaponProvider = new SingleWeaponProvider(s.Weapon);
                    }
                });

        }

        private void CitizenChaos()
        {
            if (!PlayerPed.IsSafeExist()) return;
            
            //まだ処理をしていない市民に対してコルーチンを回す
            var nearPeds =
                CachedPeds.Where(x => x.IsSafeExist() && x.IsInRangeOf(PlayerPed.Position, chaosModeSetting.Radius));

            foreach (var ped in nearPeds.Where(x => x.IsSafeExist() && !chaosedPedList.Contains(x.Handle)))
            {
                chaosedPedList.Add(ped.Handle);
                var id = StartCoroutine(ChaosPedAction(ped));
                coroutineIds.Add(id);
            }
        }

        /// <summary>
        /// 全てのカオスモードの用のコルーチンを停止する
        /// </summary>
        private void StopAllChaosCoroutine()
        {
            foreach (var id in coroutineIds)
            {
                StopCoroutine(id);
            }
            coroutineIds.Clear();

        }

        /// <summary>
        /// 市民一人ひとりについて回るコルーチン
        /// </summary>
        /// <param name="ped"></param>
        /// <returns></returns>
        private IEnumerable<Object> ChaosPedAction(Ped ped)
        {

            //魚なら除外する
            var m = (uint)ped.Model.Hash;
            if (fishHashes.Contains(m)) yield break;

            if (!ped.IsSafeExist()) yield break;
            var pedId = ped.Handle;

            //市民の武器を交換する（内部でミッションキャラクタの判定をする）
            var equipedWeapon = GiveWeaponTpPed(ped);

            //ここでカオス化して良いか検査する
            if (!chaosChecker.IsPedChaosAvailable(ped))
            {
                chaosedPedList.Remove(pedId);
                yield break;
            }

            if (!ped.IsRequiredForMission())
            {
                //ミッション関係じゃないキャラならパラメータ変更
                SetPedStatus(ped);
                PreventToFlee(ped);
            }
            else
            {
                var playerGroup = Game.Player.GetPlayerGroup();
                if (!ped.IsPedGroupMember(playerGroup))
                {
                    ped.NeverLeavesGroup = false;
                    ped.RemovePedFromGroup();
                }
            }
            //以下ループ
            do
            {
                if (!ped.IsSafeExist() || ped.IsDead || !PlayerPed.IsSafeExist())
                {
                    break;
                }

                if (!ped.IsInRangeOf(PlayerPed.Position, chaosModeSetting.Radius + 10))
                {
                    break;
                }

                if (!chaosChecker.IsPedChaosAvailable(ped))
                {
                    break;
                }

                //武器を変更する
                if (Random.Next(0, 100) < chaosModeSetting.WeaponChangeProbabillity)
                {
                    equipedWeapon = GiveWeaponTpPed(ped);
                }

                yield return RandomWait();

                if (!ped.IsSafeExist() || ped.IsDead)
                {
                    break;
                }

                //攻撃する
                PedRiot(ped, equipedWeapon);

                //適当に待機
                foreach (var s in WaitForSeconds(5 + (float)Random.NextDouble() * 3))
                {
                    if (ped.IsSafeExist() && ped.IsFleeing())
                    {
                        //市民が攻撃をやめて逃げ始めたら再度セットする
                        break;
                    }
                    yield return s;
                }

            } while (ped.IsSafeExist() && ped.IsAlive);

            chaosedPedList.Remove(pedId);
        }


        /// <summary>
        /// カオス化時の攻撃対象を取得する
        /// </summary>
        /// <param name="ped"></param>
        /// <returns></returns>
        private Ped GetTargetPed(Ped ped)
        {
            if (!ped.IsSafeExist() || !PlayerPed.IsSafeExist()) return null;

            //プレイヤへの攻撃補正が設定されているならプレイヤを攻撃対象にする
            if (chaosModeSetting.IsAttackPlayerCorrectionEnabled &&
                Random.Next(0, 100) < chaosModeSetting.AttackPlayerCorrectionProbabillity)
            {
                return PlayerPed;
            }

            //100m以内の市民
            var aroundPeds =
                CachedPeds.Concat(new[] { PlayerPed }).Where(
                    x => x.IsSafeExist() && !x.IsSameEntity(ped) && x.IsAlive && ped.IsInRangeOf(x.Position, 100))
                    .ToArray();

            //100m以内の市民のうち、より近い人を選出
            var nearPeds = aroundPeds.OrderBy(x => (ped.Position - x.Position).Length()).Take(5).ToArray();

            if (nearPeds.Length == 0) return null;
            var randomindex = Random.Next(nearPeds.Length);
            return nearPeds[randomindex];
        }

        private void SetPedStatus(Ped ped)
        {
            if (!ped.IsSafeExist()) return;
            //FIBミッションからのコピペ（詳細不明）
            ped.SetCombatAttributes(9, false);
            ped.SetCombatAttributes(1, false);
            ped.SetCombatAttributes(3, true);
            ped.SetCombatAttributes(29, true);

            ped.MaxHealth = 2000;
            ped.Health = 2000;
            ped.SetPedShootRate(100);
            ped.Accuracy = chaosModeSetting.ShootAccuracy;
            //戦闘能力？
            ped.SetCombatAbility(1000);
            //戦闘範囲
            ped.SetCombatRange(30);
            //攻撃を受けたら反撃する
            ped.RegisterHatedTargetsAroundPed(20);

            //プレイヤと敵対状態にする
            ped.RelationshipGroup = chaosRelationShipId;
        }

        /// <summary>
        /// 市民を暴徒化する
        /// </summary>
        /// <param name="ped">市民</param>
        /// <param name="equipWeapon">装備中の武器（最終的には直接取得できるようにしたい)</param>
        private void PedRiot(Ped ped, Weapon equipWeapon)
        {
            try
            {
                if (!ped.IsSafeExist()) return;
                var target = GetTargetPed(ped);
                if (!target.IsSafeExist()) return;
                ped.TaskSetBlockingOfNonTemporaryEvents(true);
                ped.SetPedKeepTask(true);
                ped.AlwaysKeepTask = true;
                ped.IsVisible = true;
                if (ped.IsInVehicle())
                {
                    //TODO:車から投擲物を投げる方法を調べる
                    ped.Task.ClearAll();
                    ped.TaskDriveBy(target, FiringPattern.BurstFireDriveby);

                }
                else
                {
                    ped.Task.ClearAllImmediately();
                    if (chaosModeSetting.IsStupidShooting)
                    {
                        if (equipWeapon.IsProjectileWeapon())
                        {
                            ped.ThrowProjectile(target.Position);
                        }
                        else if (equipWeapon.IsShootWeapon())
                        {
                            ped.Task.ShootAt(target, 10000);
                        }
                        else
                        {
                            ped.Task.FightAgainst(target, 60000);
                        }
                    }
                    else
                    {
                        ped.Task.FightAgainst(target, 60000);
                    }
                }

                ped.SetPedFiringPattern((int)FiringPattern.FullAuto);
            }
            catch (Exception e)
            {
                LogWrite(e.ToString());
                LogWrite(e.StackTrace);
            }
        }

        /// <summary>
        /// 市民が逃げないようにパラメータ設定する
        /// </summary>
        /// <param name="ped"></param>
        private void PreventToFlee(Ped ped)
        {
            ped.SetFleeAttributes(0, 0);
            ped.SetCombatAttributes(46, true);
            //非武装（近接武器）で武装した市民を攻撃できるか
            ped.SetCombatAttributes(5, true);
            Function.Call(Hash.SET_PED_RELATIONSHIP_GROUP_HASH, ped, this.GetGTAObjectHashKey("cougar"));
        }

        /// <summary>
        /// 市民に武器をもたせる
        /// </summary>
        /// <param name="ped">市民</param>
        /// <returns>装備した武器</returns>
        private Weapon GiveWeaponTpPed(Ped ped)
        {
            try
            {
                if (!ped.IsSafeExist()) return Weapon.UNARMED;
                //市民の武器を変更して良いか調べる
                if (!chaosChecker.IsPedChangebalWeapon(ped)) return Weapon.UNARMED;

                //車に乗っているなら車用の武器を渡す
                var weapon = Weapon.UNARMED;
                if (_isBaseball)
                {
                    weapon = CurrentWeaponProvider.GetRandomCloseWeapons();
                }
                else
                {
                    weapon = ped.IsInVehicle()
                        ? CurrentWeaponProvider.GetRandomDriveByWeapon()
                        : CurrentWeaponProvider.GetRandomAllWeapons();
                }

                var weaponhash = (int)weapon;

                ped.SetDropWeaponWhenDead(false); //武器を落とさない
                ped.Weapons.RemoveAll();
                ped.GiveWeapon(weaponhash, 1000); //指定武器所持
                ped.EquipWeapon(weaponhash); //武器装備
                return weapon;
            }
            catch (Exception e)
            {
                LogWrite("AttachPedWeaponError!" + e.Message);
            }
            return Weapon.UNARMED;
        }
    }
}
