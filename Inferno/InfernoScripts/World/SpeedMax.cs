using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using UniRx;
namespace Inferno.InfernoScripts.World
{
    class SpeedMax : InfernoScript
    {
        HashSet<int> vehicleHashSet = new HashSet<int>();

        protected override void Setup()
        {
            CreateInputKeywordAsObservable("smax")
                .Subscribe(_ => IsActive = !IsActive);

            IsActiveAsObservable
                .Subscribe(x => DrawText($"SpeedMax:{IsActive}"));

            OnTickAsObservable
                .Where(_ => IsActive)
                .Subscribe(_ =>
                {
                    foreach (var v in CachedVehicles
                                        .Where(x =>
                                            x.IsSafeExist()
                                            && x.IsInRangeOf(PlayerPed.Position, 100.0f)
                                            && !vehicleHashSet.Contains(x.Handle)
                                        ))
                    {
                        vehicleHashSet.Add(v.Handle);
                        StartCoroutine(VehicleSpeedMaxCorutine(v));
                    }
                });

            OnTickAsObservable
                .Where(_ => IsActive)
                .Select(_ => PlayerPed.IsAlive)
                .DistinctUntilChanged()
                .Where(x => x)
                .Subscribe(_ => vehicleHashSet.Clear());

        }

        private IEnumerable<object> VehicleSpeedMaxCorutine(Vehicle v)
        {
            yield return WaitForSeconds(1);
            var time = 0.0f;
            var maxSpeed = Random.Next(5, 10);
            var duration = (float)Random.Next(3, 20);

            while (IsActive && v.IsSafeExist())
            {
                if (!v.IsInRangeOf(PlayerPed.Position, 600)) yield break;
                if(PlayerVehicle.Value == v) yield break;
                var currentSpeed = maxSpeed * ((float)Math.Cos(2 * Math.PI * (time / duration)) + 0.5f);
                v.ApplyForce(currentSpeed * v.ForwardVector);
                time += 0.1f;
                yield return null;
            }

        }
    }
}
