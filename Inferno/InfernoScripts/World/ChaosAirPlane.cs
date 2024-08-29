using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.ChaosMode;
using Inferno.Utilities;

namespace Inferno
{
    internal class ChaosAirPlaneConfig : InfernoConfig
    {
        public int AirPlaneCount { get; set; } = 2;

        public override bool Validate()
        {
            //AirPlaneの数は1～30の範囲である
            if (AirPlaneCount <= 0 || AirPlaneCount > 30)
            {
                return false;
            }

            return true;
        }
    }

    internal class ChaosAirPlane : InfernoScript
    {
        //攻撃半径
        private readonly float attackRadius = 350;

        /// <summary>
        /// 各戦闘機が狙っている場所
        /// </summary>
        private readonly Dictionary<int, Vector3?> targetArea = new();

        protected ChaosAirPlaneConfig config;

        protected override string ConfigFileName { get; } = "ChaosAirPlane.conf";

        protected int AirPlaneCount => config?.AirPlaneCount ?? 2;

        protected override void Setup()
        {
            config = LoadConfig<ChaosAirPlaneConfig>();
            CreateInputKeywordAsObservable("abomb")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("ChaosPlane:" + IsActive);
                });

            OnAllOnCommandObservable
                .Subscribe(_ => { IsActive = true; });

            IsActiveAsObservable
                .Where(x => x)
                .Subscribe(_ => StartChaosPlanesAsync(ActivationCancellationToken).Forget());

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
        private async ValueTask StartChaosPlanesAsync(CancellationToken ct)
        {
            foreach (var i in Enumerable.Range(0, AirPlaneCount))
            {
                PlaneManageLoopAsync(i, ct).Forget();
                await DelayAsync(TimeSpan.FromSeconds(1), ct);
            }
        }

        /// <summary>
        /// 戦闘機を管理するコルーチン
        /// </summary>
        /// <param name="id">戦闘機に割り振るID</param>
        /// <returns></returns>
        private async ValueTask PlaneManageLoopAsync(int id, CancellationToken ct)
        {
            var plane = default(Vehicle);
            var ped = default(Ped);
            await DelayRandomFrameAsync(0, 10, ct);
            while (IsActive)
            {
                if (!IsPlaneActive(plane, ped))
                {
                    //戦闘機が行動不能になっていたら再生成
                    var spawn = SpawnAirPlane();
                    if (spawn != null)
                    {
                        (plane, ped) = spawn.Value;
                        //戦闘機稼働
                        targetArea[id] = null;
                        AirPlaneAsync(plane, ped, id, ct).Forget();
                    }
                }

                //5秒ごとにチェック
                await DelayAsync(TimeSpan.FromSeconds(5), ct);
            }
        }

        private (Vehicle v, Ped p)? SpawnAirPlane()
        {
            var model = new Model(VehicleHash.Lazer);
            //戦闘機生成
            var plane = World.CreateVehicle(model, PlayerPed.Position.AroundRandom2D(300) + new Vector3(0, 0, 150));
            if (!plane.IsSafeExist())
            {
                return null;
            }

            AutoReleaseOnGameEnd(plane);
            plane.Speed = 500;
            plane.PetrolTankHealth = 10;
            //パイロットのラマー召喚
            var ped = plane.CreatePedOnSeat(VehicleSeat.Driver, new Model(PedHash.LamarDavis));
            if (!ped.IsSafeExist())
            {
                return null;
            }

            AutoReleaseOnGameEnd(ped);
            ped.SetNotChaosPed(true);

            return (plane, ped);
        }

        /// <summary>
        /// 戦闘機の制御
        /// </summary>
        private async ValueTask AirPlaneAsync(Vehicle plane, Ped ped, int id, CancellationToken ct)
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
                    ped.SetCombatAttributes(53, true);

                    //少しづつ耐久値を削る
                    plane.PetrolTankHealth -= 1.0f;
                    await YieldAsync(ct);
                }

                //しばらく待つ
                foreach (var _ in WaitForSeconds(10))
                {
                    if (!IsPlaneActive(plane, ped))
                    {
                        break;
                        ;
                    }

                    //ターゲットが死亡していたらターゲット変更
                    if (!target.IsSafeExist() || target.IsDead || !IsActive)
                    {
                        break;
                    }

                    if (target.IsInRangeOf(plane.Position, attackRadius) && Random.Next(0, 100) < 5)
                    {
                        //たまに攻撃
                        targetArea[id] = target.Position;
                        await AttackAsync(plane, ped, target, ct);
                        await DelayAsync(TimeSpan.FromSeconds(5), ct);
                        targetArea[id] = null;
                        break;
                    }

                    await YieldAsync(ct);
                }

                await YieldAsync(ct);
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
            Function.Call(Hash.TASK_PLANE_MISSION, ped, plane, 0, 0, tarPos.X, tarPos.Y, tarPos.Z, 4, planeSpeed, -1.0,
                -1.0, 100, -500);
        }

        //キャッシュ市民から一人選出
        private Entity GetRandomTarget()
        {
            var playerVehicle = PlayerPed.CurrentVehicle;

            //プレイヤの近くの市民
            var targetPeds = CachedPeds
                .Where(x => x.IsSafeExist() && x.IsHuman && x.IsAlive && x.IsInRangeOf(PlayerPed.Position, 150)
                            && (!x.IsInVehicle() || x.CurrentVehicle != playerVehicle))
                .Concat(new[] { PlayerPed });

            var targetVehicles = CachedVehicles
                .Where(x => x.IsSafeExist() && x.IsAlive && x.IsInRangeOf(PlayerPed.Position, 150)
                            && playerVehicle != x);

            var targets = targetPeds.Concat(targetVehicles.Cast<Entity>()).ToArray();


            var target = targets.Length > 5 ? targets[Random.Next(targets.Length)] : null;

            //ターゲットが周りにいない場合は誰も攻撃しない
            if (target == null)
            {
                return null;
            }

            if (CachedMissionEntities.Value.Any(x => x.Position.DistanceTo2D(target.Position) < 30.0f))
            {
                // ミッションキャラクタ付近が選択された場合は除外
                return null;
            }

            return target;
        }

        //戦闘機が動作可能な状態であるか
        private bool IsPlaneActive(Vehicle plane, Ped ped)
        {
            return ped.IsSafeExist() && plane.IsSafeExist() && ped.IsAlive && plane.IsAlive;
        }

        /// <summary>
        /// 爆撃
        /// </summary>
        private async ValueTask AttackAsync(Vehicle plane, Ped driver, Entity target, CancellationToken ct)
        {
            var num = Random.Next(3, 5);
            var targetArea = target.Position;
            var speed = target == PlayerPed || target == PlayerVehicle.Value ? 150 : 500;
            while (num-- > 0)
            {
                if (!plane.IsSafeExist() || !driver.IsSafeExist() || !target.IsSafeExist())
                {
                    return;
                }

                ShootAt(plane, driver, targetArea, speed);
                await DelayAsync(TimeSpan.FromSeconds(0.8f), ct);
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