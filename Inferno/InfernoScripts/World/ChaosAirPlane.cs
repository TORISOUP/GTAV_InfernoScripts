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
            CreateInputKeywordAsObservable("abomb")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("AngryPlane:" + IsActive,3.0f);

                    if (IsActive)
                    {
                        StartCoroutine(StartChaosPlanes());
                    }
                });

            OnAllOnCommandObservable
                .Subscribe(_ =>
                {
                    if (!IsActive)
                    {
                        IsActive = true;
                        StartCoroutine(StartChaosPlanes());
                    }
                });
        }

        //時間差で戦闘機を出現させる
        private IEnumerable<object> StartChaosPlanes()
        {
            foreach (var i in Enumerable.Range(0,7))
            {
                StartCoroutine(PlaneManageCoroutine());
                yield return WaitForSeconds(1);
            }
          
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
            var plane = GTA.World.CreateVehicle(model, PlayerPed.Position.AroundRandom2D(300) + new Vector3(0, 0, 150));
            if (!plane.IsSafeExist()) return null;
            plane.Speed = 50;
            plane.Quaternion = Quaternion.RotationAxis(Vector3.WorldUp, (float) (Random.NextDouble()*Math.PI)) * plane.Quaternion;
            plane.AddBlip().Color = BlipColor.White;
            //パイロットのラマー召喚
            var ped = plane.CreatePedOnSeat(VehicleSeat.Driver, new Model(PedHash.LamarDavis));
            if (!ped.IsSafeExist())  return null;
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
                    ped.Task.FightAgainst(target);
                    yield return null;
                }

                //しばらく待つ
                foreach (var s in WaitForSeconds(25))
                {
                    //ターゲットが死亡していたらターゲット変更
                    if(!target.IsSafeExist() || target.IsDead || !IsActive) break;
                    yield return null;
                }
                yield return null;
            }

            if (plane.IsSafeExist())
            {
                plane.PetrolTankHealth = -1;
                plane.MarkAsNoLongerNeeded();
            }

            if (ped.IsSafeExist())
            {
                ped.MarkAsNoLongerNeeded();
            }
        }

        //キャッシュ市民から一人選出
        private Ped GetRandomPed()
        {
            //プレイヤの近くの市民
            var targetPeds = CachedPeds
                .Where(x => x.IsSafeExist() && x.IsHuman && x.IsAlive && x.IsInRangeOf(PlayerPed.Position, 100));

            if (PlayerPed.IsInVehicle())
            {
                //プレイヤが車に乗っているなら対象に追加する
                targetPeds = targetPeds.Concat(new[]{PlayerPed});
            }

            var peds = targetPeds.ToArray();

            return peds.Length > 0 ? peds[Random.Next(peds.Length)] : null;
        }

        //戦闘機が動作可能な状態であるか
        private bool IsPlaneActive(Vehicle plane, Ped ped)
        {
            return ped.IsSafeExist() && plane.IsSafeExist() && ped.IsAlive && plane.IsAlive;
        }
    }
}
