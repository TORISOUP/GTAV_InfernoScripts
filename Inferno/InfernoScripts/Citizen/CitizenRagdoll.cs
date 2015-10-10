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
                .Where(_ => Function.Call<bool>(Hash.IS_CUTSCENE_PLAYING) && IsActive)
                .Subscribe(_ =>
                {
                    foreach (var ped in CachedPeds.Where(x=>x.IsSafeExist()))
                    {
                        ped.CanRagdoll = true;
                        ped.SetToRagdoll(1000);
                        ped.ApplyForce(new Vector3(0, 0, 1));
                    }
                });

            OnKeyDownAsObservable
                .Where(x => IsActive && x.KeyCode == Keys.F8)
                .Subscribe(_ => IsActive = !IsActive);
        }
    }
}
