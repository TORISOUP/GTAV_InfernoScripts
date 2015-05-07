using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using GTA;
using Reactive.Bindings;

namespace Inferno.ChaosMode
{
    class ChaosMode : InfernoScript
    {
        private readonly string Keyword = "chaos";

        public ReactiveProperty<bool> _isActive = new ReactiveProperty<bool>(Scheduler.Immediate);

        private CharacterChaosChecker chaosChecker;

        private HashSet<int> chaosedPedList = new HashSet<int>();

        protected override int TickInterval
        {
            get { return 1000; }
        }

        protected override void Setup()
        {
            chaosChecker = new CharacterChaosChecker(false,true);

            //キーワードが入力されたらON／OFFを切り替える
            CreateInputKeywordAsObservable(Keyword)
                .Subscribe(_ =>
                {
                    _isActive.Value = !_isActive.Value;
                    chaosedPedList.Clear();
                });

            //interval間隔で市民をカオス化する
            OnTickAsObservable
                .Where(_ => _isActive.Value)
                .Subscribe(_ => CitizenChaos());
        }

        private void CitizenChaos()
        {
            foreach (var ped in CachedPeds.Where(x=>chaosChecker.IsPedChaosAvailable(x) && !chaosedPedList.Contains(x.ID)))
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
            GiveWeaponTpPed(ped);

            //以下2秒間隔でループ
            do
            {
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
                    GiveWeaponTpPed(ped);
                }

                if (!ped.IsInVehicle() && !ped.IsWeaponReloading())
                {
                    //リロード中でなく車から降りているなら攻撃する
                    PedRiot(ped);
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
        /// 市民に攻撃をさせる
        /// </summary>
        private void PedRiot(Ped ped)
        {
            try
            {
                var target = CachedPeds.ElementAt(Random.Next(CachedPeds.Count()));
                if (target.IsSameEntity(ped)) return;
                ped.Task.ClearSecondary();
                ped.Task.ShootAt(target, 100000);
                ped.SetPedFiringPattern((int) FiringPattern.FullAuto);
                ped.SetPedShootRate(100);

            }
            catch (Exception e)
            {
                LogWrite("SetRiotError!" + e.Message + "\r\n");
            }
        }


        /// <summary>
        /// 市民に武器をもたせる
        /// </summary>
        private void GiveWeaponTpPed(Ped ped)
        {
            try
            {
                var weaponhash = GetRandomWeaponHash();

                ped.SetDropWeaponWhenDead(false); //武器を落とさない
                ped.GiveWeapon(weaponhash, 1000); //指定武器所持
                ped.EquipWeapon(weaponhash); //武器装備

            }
            catch (Exception e)
            {

                LogWrite("AttachPedWeaponError!" + e.Message + "\r\n");
            }
        }

        /// <summary>
        /// ランダムな武器を取得する
        /// </summary>
        /// <returns></returns>
        private int GetRandomWeaponHash()
        {
            return Enum.GetValues(typeof (Weapon))
                .Cast<Weapon>()
                .OrderBy(c => Random.Next())
                .Select(x => (int) x)
                .FirstOrDefault();
        }

    }
}
