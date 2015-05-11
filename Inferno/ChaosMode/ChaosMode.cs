using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using GTA;
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
            MissionCharacterTreatmentType.ExcludeAllMissionCharacter;

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
            cachedPedForChaos = World.GetNearbyPeds(this.GetPlayer(), 1000);
            foreach (
                var ped in
                    cachedPedForChaos
                        .Where(x => chaosChecker.IsPedChaosAvailable(x)
                                    && chaosChecker.CheckPedTask(x)
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
        private IEnumerator ChaosPedAction(Ped ped)
        {
            var pedId = ped.ID;

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

                if (ped.IsInVehicle())
                {
                    //車にのっているならたまに降りる
                    if (Random.Next(0, 100) < 20)
                    {
                        ped.Task.ClearAll();
                        ped.Task.LeaveVehicle();
                    }
                }

                if (Random.Next(0, 100) < 20)
                {
                    //たまに武器を変える
                    equipedWeapon = GiveWeaponTpPed(ped);
                }

                if (!ped.IsWeaponReloading())
                {
                    //リロード中でなく車から降りているなら攻撃する
                    PedRiot(ped, equipedWeapon);
                }


                //2秒待機
                foreach (var s in WaitForSecond(2.0f))
                {
                    yield return s;
                }

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

                ped.Task.ClearSecondary();
                //ShootAtだとその場で射撃
                //FightAgainstは戦闘状態にして自律AIとして攻撃
                ped.Task.FightAgainst(target, 100000);
                ped.SetPedFiringPattern((int) FiringPattern.FullAuto);
                ped.SetPedShootRate(100);

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
                var weapon = weaponProvider.GetRandomWeapon();
                var weaponhash = (int) weapon;

                ped.SetDropWeaponWhenDead(false); //武器を落とさない
                ped.GiveWeapon(weaponhash, 1000); //指定武器所持
                ped.EquipWeapon(weaponhash); //武器装備
                return weapon;
            }
            catch (Exception e)
            {
                LogWrite("AttachPedWeaponError!" + e.Message + "\r\n");
            }
            return Weapon.UNARMED;
        }

    }
}
