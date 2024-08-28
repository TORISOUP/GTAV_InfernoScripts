using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;
using GTA.Math;

namespace Inferno
{
    internal class CitizenRagdoll : InfernoScript
    {
        protected override void Setup()
        {
            OnKeyDownAsObservable
                .Where(x => x.KeyCode == Keys.F8)
                .Subscribe(_ =>
                {
                    DrawText("Ragdoll");
                    StartCoroutine(RagdollCoroutine());
                });
        }

        private IEnumerable<object> RagdollCoroutine()
        {
            var peds = CachedPeds.Where(
                    x => x.IsSafeExist()
                         && x.IsRequiredForMission()
                         && x.CanRagdoll
                         && x.IsInRangeOf(PlayerPed.Position, 15))
                .ToArray();

            foreach (var ped in peds)
            {
                if (!ped.IsSafeExist())
                {
                    continue;
                }

                ped.SetToRagdoll(100);
                ped.ApplyForce(new Vector3(0, 0, 2));
                yield return null;
            }
        }
    }
}