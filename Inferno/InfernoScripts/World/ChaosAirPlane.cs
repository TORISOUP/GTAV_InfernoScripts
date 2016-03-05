using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.ChaosMode;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Inferno
{
    internal class ChaosAirPlane : InfernoScript
    {
        /// <summary>
        /// 各戦闘機が狙っている場所
        /// </summary>
        private Dictionary<int, Vector3?> targetArea = new Dictionary<int, Vector3?>();

        //攻撃半径
        private float attackRadius = 350;

        protected override void Setup()
        {
            CreateInputKeywordAsObservable("abomb")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("ChaosPlane:" + IsActive, 3.0f);
                });

            OnAllOnCommandObservable
                .Subscribe(_ =>
                {
                    IsActive = true;
                });

            IsActiveAsObservable.Where(x => x)
                .Subscribe(_ => StartCoroutine(StartChaosPlanes()));

            //ターゲット描画
            OnDrawingTickAsObservable
                .Where(_ => IsActive && targetArea.Count > 0)
                .Subscribe(_ =>
                {
                    var insensity = 5;

                    var array = targetArea.Values
                        .Where(x => x != null)
                        .Select(x => x.Value);

                    foreach (var point in array)
                    {
                        NativeFunctions.CreateLight(point, 255, 30, 30, 10.0f, insensity);
                    }
                });
        }

        //時間差で戦闘機を出現させる
        private IEnumerable<object> StartChaosPlanes()
        {
            foreach (var i in Enumerable.Range(0, 4))
            {
                StartCoroutine(PlaneManageCoroutine(i));
                yield return WaitForSeconds(1);
            }
        }

        /// <summary>
        /// 戦闘機を管理するコルーチン
        /// </summary>
        /// <param name="id">戦闘機に割り振るID</param>
        /// <returns></returns>
        private IEnumerable<object> PlaneManageCoroutine(int id)
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
                        targetArea[id] = null;
                        StartCoroutine(AirPlaneCoroutine(plane, ped, id));
                    }
                }
                //5秒ごとにチェック
                yield return WaitForSeconds(5);
            }
        }

        private System.Tuple<Vehicle, Ped> SpawnAirPlane()
        {
            var model = new Model(VehicleHash.Lazer);
            //戦闘機生成
            var plane = GTA.World.CreateVehicle(model, PlayerPed.Position.AroundRandom2D(300) + new Vector3(0, 0, 150));
            if (!plane.IsSafeExist()) return null;
            plane.Speed = 500;
            plane.PetrolTankHealth = 10;
            //パイロットのラマー召喚
            var ped = plane.CreatePedOnSeat(VehicleSeat.Driver, new Model(PedHash.LamarDavis));
            if (!ped.IsSafeExist()) return null;
            ped.SetNotChaosPed(true);

            return new Tuple<Vehicle, Ped>(plane, ped);
        }

        /// <summary>
        /// 戦闘機のコルーチン
        /// </summary>
        private IEnumerable<object> AirPlaneCoroutine(Vehicle plane, Ped ped, int id)
        {
            var speed = (float)Random.Next(50, 500);
            while (IsActive && IsPlaneActive(plane, ped))
            {
                var target = GetRandomTarget();
                targetArea[id] = null;
                if (target.IsSafeExist())
                {
                    ped.Task.ClearAll();
                    //周辺市民をターゲットにする
                    SetPlaneTask(plane, ped, target, speed);

                    //少しづつ耐久値を削る
                    plane.PetrolTankHealth -= 1.0f;
                    yield return null;
                }

                //しばらく待つ
                foreach (var s in WaitForSeconds(10))
                {
                    if (!IsPlaneActive(plane, ped))
                    {
                        break; ;
                    }

                    //ターゲットが死亡していたらターゲット変更
                    if (!target.IsSafeExist() || target.IsDead || !IsActive) break;

                    if (target.IsInRangeOf(plane.Position, attackRadius) && Random.Next(0, 100) < 5)
                    {
                        //たまに攻撃
                        targetArea[id] = target.Position;
                        yield return AttackCoroutine(plane, ped, target);
                        yield return WaitForSeconds(5);
                        targetArea[id] = null;
                        break;
                    }

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

        private void SetPlaneTask(Vehicle plane, Ped ped, Entity target, float planeSpeed)
        {
            var tarPos = target.Position;
            Function.Call(Hash.TASK_PLANE_MISSION, ped, plane, 0, 0, tarPos.X, tarPos.Y, tarPos.Z, 4, planeSpeed, -1.0, -1.0, 100, -500);
        }

        //キャッシュ市民から一人選出
        private Entity GetRandomTarget()
        {
            var playerVehicle = PlayerPed.CurrentVehicle;

            //プレイヤの近くの市民
            var targetPeds = CachedPeds
                .Where(x => x.IsSafeExist() && x.IsHuman && x.IsAlive && x.IsInRangeOf(PlayerPed.Position, 150)
                            && (!x.IsInVehicle() || x.CurrentVehicle != playerVehicle)).Concat(new Ped[] { PlayerPed });

            var targetVehicles = CachedVehicles
                .Where(x => x.IsSafeExist() && x.IsAlive && x.IsInRangeOf(PlayerPed.Position, 150)
                            && playerVehicle != x);

            var targets = targetPeds.Concat(targetVehicles.Cast<Entity>()).ToArray();

            return targets.Length > 0 ? targets[Random.Next(targets.Length)] : null;
        }

        //戦闘機が動作可能な状態であるか
        private bool IsPlaneActive(Vehicle plane, Ped ped)
        {
            return ped.IsSafeExist() && plane.IsSafeExist() && ped.IsAlive && plane.IsAlive;
        }

        /// <summary>
        /// 爆撃コルーチン
        /// </summary>
        private IEnumerable<object> AttackCoroutine(Vehicle plane, Ped driver, Entity target)
        {
            var num = Random.Next(3, 5);
            var targetArea = target.Position;
            var speed = (target == PlayerPed || target == PlayerVehicle.Value) ? 150 : 500;
            while (num-- > 0)
            {
                if (!plane.IsSafeExist() || !driver.IsSafeExist() || !target.IsSafeExist()) yield break;
                ShootAt(plane, driver, targetArea, speed);
                yield return WaitForSeconds(0.8f);
            }
        }

        /// <summary>
        /// 爆撃する
        /// </summary>
        private void ShootAt(Vehicle plane, Ped driver, Vector3 targetArea, float speed)
        {
            var startPosition = plane.GetOffsetFromEntityInWorldCoords(0, 0, -1.0f);
            var targetPosition = targetArea.AroundRandom2D(10.0f);

            NativeFunctions.ShootSingleBulletBetweenCoords(
                startPosition, targetPosition, 100, WeaponHash.RPG, driver, speed);
        }
    }
}
