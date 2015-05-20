using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Forms;
using GTA;
using GTA.Native;
using Reactive.Bindings;

namespace Inferno.ChaosMode
{
    internal class ChaosMode : InfernoScript
    {
        private readonly string Keyword = "chaos";

        public ReactiveProperty<bool> _isActive = new ReactiveProperty<bool>(Scheduler.Immediate);
        private CharacterChaosChecker chaosChecker;
        private HashSet<int> chaosedPedList = new HashSet<int>();
        private Ped[] cachedPedForChaos = new Ped[0];
        private WeaponProvider weaponProvider;

        //デフォルトは全員除外
        private MissionCharacterTreatmentType currentTreatType =
            MissionCharacterTreatmentType.ExcludeUniqueCharacter;

        private MissionCharacterTreatmentType nextTreatType;

        protected override int TickInterval => 1000;

        protected override void Setup()
        {
            chaosChecker = new CharacterChaosChecker(MissionCharacterTreatmentType.ExcludeAllMissionCharacter, true);
            weaponProvider = new WeaponProvider();

            //キーワードが入力されたらON／OFFを切り替える
            CreateInputKeywordAsObservable(Keyword)
                .Subscribe(_ =>
                {
                    _isActive.Value = !_isActive.Value;
                    chaosedPedList.Clear();
                    if (_isActive.Value)
                    {
                        DrawText("ChaosMode:On/" + currentTreatType.ToString(), 3.0f);
                    }
                    else
                    {
                        DrawText("ChaosMode:Off", 3.0f);
                    }

                });

            nextTreatType = currentTreatType;

            //F7でキャラカオスの切り替え（暫定
            OnKeyDownAsObservable
                .Where(x=> _isActive.Value && x.KeyCode == Keys.F7)
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
                });

            //interval間隔で市民をカオス化する
            OnTickAsObservable
                .Where(_ => _isActive.Value)
                .Subscribe(_ => CitizenChaos());

            OnTickAsObservable
                .Select(_ => this.GetPlayer().IsDead)
                .DistinctUntilChanged()
                .Where(x => x)
                .Subscribe(_ => chaosedPedList.Clear());

        }

        private void CitizenChaos()
        {

            cachedPedForChaos = World.GetNearbyPeds(this.GetPlayer(), 3000);
            foreach (var ped in cachedPedForChaos.Where(x =>x.IsSafeExist() && !chaosedPedList.Contains(x.Handle)))
            {
                chaosedPedList.Add(ped.Handle);
                StartCoroutine(ChaosPedAction(ped));
            }
        }

        /// <summary>
        /// 市民一人ひとりについて回るコルーチン
        /// </summary>
        /// <param name="ped"></param>
        /// <returns></returns>
        private IEnumerable<Object>  ChaosPedAction(Ped ped)
        {
            var pedId = ped.Handle;

            //武器を与える
            Weapon equipedWeapon = GiveWeaponTpPed(ped);

            //ここでカオス化して良いか検査する
            if (!chaosChecker.IsPedChaosAvailable(ped))
            {
               yield break;
            }
            
            if (!ped.IsRequiredForMission())
            {
                ped.MaxHealth = 2000;
                ped.Health = 2000;
                ped.SetPedFiringPattern((int) FiringPattern.FullAuto);
                ped.SetPedShootRate(100);
                ped.SetAlertness(0);
                ped.SetCombatAbility(100);
                ped.SetCombatRange(100);
                ped.RegisterHatedTargetsAroundPed(100);

            }

            //以下ループ
            do
            {
                if (!ped.IsSafeExist())
                {
                    yield break;
                }

                if (!chaosChecker.IsPedChaosAvailable(ped))
                {
                    break;
                }

                if (Random.Next(0, 100) < 30)
                {
                    //たまに武器を変える
                    equipedWeapon = GiveWeaponTpPed(ped);
                }


                //攻撃する
                PedRiot(ped, equipedWeapon);

                //適当に待機
                yield return WaitForSeconds(1 + (float) Random.NextDouble()*5);

            } while (ped.IsSafeExist() && ped.IsAlive);

            chaosedPedList.Remove(pedId);
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
                //ターゲットになりうる市民
                var nearPeds =
                    cachedPedForChaos.Concat(new Ped[] {this.GetPlayer()}).Where(
                        x => x.IsSafeExist() && !x.IsSameEntity(ped) && x.IsAlive && (ped.Position - x.Position).Length() < 50)
                        .ToArray();

                if (nearPeds.Length == 0)
                {
                    return;
                }

                var randomindex = Random.Next(nearPeds.Length);
                var target = nearPeds[randomindex];

                ped.Task.ClearAll();

                if (ped.IsInVehicle())
                {
                    //TODO:車から投擲物を投げる方法を調べる
                   ped.TaskDriveBy(target,FiringPattern.BurstFireDriveby);
                }
                else
                {
                    //車に乗っていないなら周辺を攻撃
                    if (weaponProvider.IsProjectileWeapon(equipWeapon))
                    {
                        ped.ThrowProjectile(target.Position);
                    }
                    else if (weaponProvider.IsShootWeapon(equipWeapon))
                    {
                        ped.Task.ShootAt(target, 10000);
                    }
                    else
                    {
                        ped.Task.FightAgainst(target, 1000);
                    }
                }
                ped.SetPedKeepTask(true);
            }
            catch (Exception e)
            {
                LogWrite(e.ToString());
                LogWrite(e.StackTrace);
            }
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
                //車に乗っているなら車用の武器を渡す
                var weapon =  ped.IsInVehicle()
                    ? weaponProvider.GetRandomInVehicleWeapon()
                    : weaponProvider.GetRandomWeaponExcludeClosedWeapon();

                var weaponhash = (int)weapon;

                ped.SetDropWeaponWhenDead(false); //武器を落とさない
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
