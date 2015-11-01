using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;


namespace Inferno
{
    internal class Fuluton : InfernoScript
    {
        protected override int TickInterval { get; } = 100;

        /// <summary>
        /// フルトン回収のコルーチン対象になっているEntity
        /// </summary>
        private HashSet<int> fulutonedEntityList = new HashSet<int>(); 

        private Queue<PedHash> motherBasePeds = new Queue<PedHash>(30);
        private Queue<GTA.Native.VehicleHash> motherbaseVeh = new Queue<GTA.Native.VehicleHash>(30); 

        protected override void Setup()
        {
            CreateInputKeywordAsObservable("fuluton")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("Fuluton:" + IsActive, 3.0f);
                });

            OnAllOnCommandObservable.Subscribe(_ => IsActive = true);

            OnTickAsObservable
                .Where(_ => IsActive)
                .Subscribe(_ => FulutonUpdate());

            //プレイヤが死んだらリストクリア
            OnTickAsObservable
                .Select(_ => PlayerPed.IsDead)
                .DistinctUntilChanged()
                .Where(x => x)
                .Subscribe(_ => fulutonedEntityList.Clear());

        }

        private void FulutonUpdate()
        {
            foreach (var entity in CachedPeds.Concat(CachedVehicles.Cast<Entity>()).Where(
                x=>x.IsSafeExist()
                && x.IsInRangeOf(PlayerPed.Position,10.0f)
                && !fulutonedEntityList.Contains(x.Handle)
                ))
            {
                if (!entity.HasBeenDamagedBy(Weapon.UNARMED)) continue;
                fulutonedEntityList.Add(entity.Handle);
                StartCoroutine(FulutonCoroutine(entity));
            }
        }

        private IEnumerable<object> FulutonCoroutine(Entity entity)
        {
            //Entityが消え去った後に処理したいので先に情報を保存しておく
            int hash = -1;
            bool isPed = false;

            var upForce = new Vector3(0, 0, 1);
            if (entity is Ped)
            {
                var p = entity as Ped;
                p.SetToRagdoll(10*1000);

                isPed = true;

            }
            hash = entity.Model.Hash;
            


            foreach (var s in WaitForSeconds(3))
            {
                if(!entity.IsSafeExist() || entity.IsDead) yield break;

                entity.ApplyForce(upForce * 1.1f);

                yield return s;
            }

            if (entity.IsRequiredForMission())
            {
                fulutonedEntityList.Remove(entity.Handle);
                yield break;
            }

            foreach (var s in WaitForSeconds(7))
            {
                if (!entity.IsSafeExist())
                {
                    if (isPed)
                    {
                        motherBasePeds.Enqueue((PedHash) hash);
                    }
                    else
                    {
                        motherbaseVeh.Enqueue((GTA.Native.VehicleHash)hash);
                    }
                    DrawText("回収完了", 3.0f);
                    yield break;
                }

                if (entity.IsDead) yield break;
                
                entity.ApplyForce(upForce * 10.0f);

                yield return s;
            }
        }
    }
}
