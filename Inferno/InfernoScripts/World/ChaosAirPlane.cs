using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.ChaosMode;

namespace Inferno
{
    class ChaosAirPlane : InfernoScript
    {
        protected override void Setup()
        {
            CreateInputKeywordAsObservable("angryplane")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("AngryPlane:" + IsActive,3.0f);

                    if (IsActive)
                    {
                        //戦闘機を３つ出す
                        StartCoroutine(PlaneManageCoroutine());
                        StartCoroutine(PlaneManageCoroutine());
                        StartCoroutine(PlaneManageCoroutine());
                    }
                });
        }

        /// <summary>
        /// 戦闘機を管理するコルーチン
        /// </summary>
        private IEnumerable<object> PlaneManageCoroutine()
        {
            var plane = default(Vehicle);
            var ped = default(Ped);
            yield return RandomWait();
            while (IsActive)
            {
                if (!IsPlaneActive(plane, ped))
                {
                    //戦闘機が行動不能になっていたら再生成
                    var spawn = SpawnAirPlane();
                    if (spawn != null)
                    {
                        plane = spawn.Item1;
                        ped = spawn.Item2;
                        //戦闘機稼働
                        StartCoroutine(AirPlaneCoroutine(plane, ped));
                    }
                }
                //5秒ごとにチェック
                yield return WaitForSeconds(5);
            }
        }

        private Tuple<Vehicle,Ped> SpawnAirPlane()
        {
            var model = new Model(VehicleHash.Lazer);
            //戦闘機生成
            var plane = GTA.World.CreateVehicle(model, PlayerPed.Position.AroundRandom2D(100) + new Vector3(0, 0, 100));
            if (!plane.IsSafeExist()) return null;
            plane.MarkAsNoLongerNeeded();

            //パイロットのラマー召喚
            var ped = plane.CreatePedOnSeat(VehicleSeat.Driver, new Model(PedHash.LamarDavis));
            if (!ped.IsSafeExist())  return null;
            ped.MarkAsNoLongerNeeded();
            //戦闘範囲
            ped.SetCombatRange(1000);
            //攻撃を受けたら反撃する
            ped.RegisterHatedTargetsAroundPed(1000);
            ped.SetNotChaosPed(true);

            return new Tuple<Vehicle, Ped>(plane, ped);
        }

        /// <summary>
        /// 戦闘機のコルーチン
        /// </summary>
        private IEnumerable<object> AirPlaneCoroutine(Vehicle plane, Ped ped)
        {
            while (IsActive && IsPlaneActive(plane, ped))
            {
                var target = GetRandomPed();
                if (target.IsSafeExist())
                {
                    //周辺市民をターゲットにする
                    ped.Task.ClearAll();
                    ped.Task.FightAgainst(target);
                    yield return null;
                }

                //しばらく待つ
                foreach (var s in WaitForSeconds(15))
                {
                    //ターゲットが死亡していたらターゲット変更
                    if(!target.IsSafeExist() || target.IsDead) break;
                    yield return null;
                }

            }

            if (plane.IsSafeExist())
            {
                plane.PetrolTankHealth = -1;
            }
        }

        //キャッシュ市民から一人選出
        private Ped GetRandomPed()
        {
            var peds = CachedPeds.Where(x => x.IsSafeExist() && x.IsAlive).Concat(new[]{PlayerPed}).ToArray();
            return peds.Length > 0 ? peds[Random.Next(peds.Length)] : null;
        }

        //戦闘機が動作可能な状態であるか
        private bool IsPlaneActive(Vehicle plane, Ped ped)
        {
            return ped.IsSafeExist() && plane.IsSafeExist() && ped.IsAlive && plane.IsAlive;
        }
    }
}
