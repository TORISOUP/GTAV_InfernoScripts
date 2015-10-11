using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA.Math;
using GTA.Native;

namespace Inferno
{
    class CitizenRagdoll : InfernoScript
    {
        
        protected override void Setup()
        {
            OnTickAsObservable
                .Where(_ => IsActive)
                .Subscribe(_ =>
                {
                    foreach (var ped in CachedPeds.Where(x=>x.IsSafeExist() && x.IsRequiredForMission()))
                    {
                        ped.CanRagdoll = true;
                        ped.SetToRagdoll(100);
                        ped.ApplyForce(new Vector3(0, 0, 2));
                    }
                });

            OnKeyDownAsObservable
                .Where(x => x.KeyCode == Keys.F8)
                
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("Ragdoll:" + IsActive, 3.0f);
                });
        }
    }
}
