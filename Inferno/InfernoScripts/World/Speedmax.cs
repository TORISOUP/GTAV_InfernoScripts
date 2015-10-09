using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;

namespace Inferno
{
    internal class Speedmax : InfernoScript
    {
        protected override int TickInterval
        {
            get { return 100; }
        }

        private bool isT = false;

        protected override void Setup()
        {
            CreateInputKeywordAsObservable("max")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("SpeedMax:" + IsActive.ToString(), 3.0f);
                });

            CreateInputKeywordAsObservable("mat")
                .Subscribe(_ =>
                {
                    isT = !isT;
                    DrawText("HitMax:" + isT.ToString(), 3.0f);
                });

            OnTickAsObservable
                .Where(_ => IsActive)
                .Skip(2).Take(1).Repeat()
                .Subscribe(_ =>
                {
                    var pos = playerPed.Position;
                    foreach (var vec in CachedVehicles.Where(x => x.IsSafeExist()
                                                                  &&
                                                                  (x.GetPedOnSeat(VehicleSeat.Driver).IsSafeExist() ||
                                                                   x.IsDead)
                                                                  && x != playerPed.CurrentVehicle
                                                                  && x.IsInRangeOf(pos, 150)).Take(10))
                    {

                        vec.Speed = 300;

                    }
                });

            OnTickAsObservable.Where(_ => IsActive).Subscribe(_ =>
            {
                var v = playerPed.CurrentVehicle;
                if (v.IsSafeExist())
                {
                    v.IsInvincible = true;
                }
            });


            CreateTickAsObservable(3000)
                .Where(_ => isT)
                .Subscribe(_ =>
                {
                    var targetPos = playerPed.Position;
                    foreach (var ped in CachedPeds.Where(x => x.IsSafeExist() && x != playerPed && x.IsInRangeOf(targetPos, 100)))
                    {
                        var length = (ped.Position - targetPos).Length();


                        var dic = (targetPos - ped.Position);
                        dic.Normalize();
                        
                        ped.CanRagdoll = true;
                        ped.SetToRagdoll(1000);
                        ped.ApplyForce(dic * (100));
                    }

                });
        }
    }
}
