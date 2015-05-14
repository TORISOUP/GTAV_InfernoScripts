using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
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

        protected override int TickInterval
        {
            get { return 1000; }
        }

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
                    DrawText("ChaosMode:" + _isActive.Value, 3.0f);

                });

            //changeでキャラカオスの切り替え（暫定
            CreateInputKeywordAsObservable("change")
                .Subscribe(_ =>
                {
                    currentTreatType
                        = (MissionCharacterTreatmentType) (((int) currentTreatType + 1)%3);
                    chaosChecker.MissionCharacterTreatment = currentTreatType;
                    DrawText("CharacterChaos:" + currentTreatType.ToString(), 3.0f);
                });

            //interval間隔で市民をカオス化する
            OnTickAsObservable
                .Where(_ => _isActive.Value)
                .Subscribe(_ => CitizenChaos());

        }

        private void CitizenChaos()
        {
            cachedPedForChaos = World.GetNearbyPeds(this.GetPlayer(), 3000);
            foreach (
                var ped in
                    cachedPedForChaos
                        .Where(x => chaosChecker.IsPedChaosAvailable(x)
                                    && !chaosedPedList.Contains(x.ID)))
            {
                chaosedPedList.Add(ped.ID);
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
            var pedId = ped.ID;
            ped.SetPedFiringPattern((int)FiringPattern.FullAuto);
            ped.SetPedShootRate(100);
            ped.SetAlertness(0);
            ped.SetCombatAbility(100);
            ped.SetCombatRange(100);
            ped.RegisterHatedTargetsAroundPed(100);

            if (!ped.IsPersistent)
            {
                ped.MaxHealth = 2000;
                ped.Health = 2000;
            }

            //武器を与える
            Weapon equipedWeapon = GiveWeaponTpPed(ped);

            //以下2秒間隔でループ
            do
            {
                if (!chaosChecker.IsPedChaosAvailable(ped))
                {
                    break;
                    ;
                }

                if (Random.Next(0, 100) < 15)
                {
                    //たまに武器を変える
                    equipedWeapon = GiveWeaponTpPed(ped);
                }


                //攻撃する
                PedRiot(ped, equipedWeapon);

                //2秒待機
                yield return WaitForSecond(2.0f);

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
                var nearPeds =
                    cachedPedForChaos.Concat(new Ped[] {this.GetPlayer()}).Where(
                        x => x.IsSafeExist() && !x.IsSameEntity(ped) && (ped.Position - x.Position).Length() < 50)
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
                    //車に乗っているなら、同じクルマに乗っていない近くの市民がいたら攻撃する
                    var nearestPeople =
                        World.GetNearbyPeds(ped, 50)
                            .FirstOrDefault(x => !x.CurrentVehicle.IsSameEntity(ped.CurrentVehicle));
                    if (nearestPeople != null)
                    {
                        Function.Call(Hash.TASK_COMBAT_PED, ped, nearestPeople, 1, 1);
                    }
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
                        ped.Task.FightAgainst(target, 10000);
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
                var weapon = Random.Next(0, 2) % 2 == 0 
                    ? weaponProvider.GetRandomShootWeapon() 
                    : weaponProvider.GetRandomProjectileWeapon();
                var weaponhash = (int) weapon;

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
