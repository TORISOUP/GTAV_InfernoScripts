using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;

namespace Inferno.InfernoScripts.Player
{
    class Warp : InfernoScript
    {
        protected override void Setup()
        {
            CreateInputKeywordAsObservable("moveto")

                .Subscribe(_ =>
                {
                    var blip = GTA.World.GetActiveBlips().FirstOrDefault(x => x.Exists());
                    if (blip == null) return;
                    var targetHeight = GTA.World.GetGroundHeight(blip.Position);
                    //地面ピッタリだと地面に埋まるので少し上空を指定する
                    var targetPos = new Vector3(blip.Position.X, blip.Position.Y, targetHeight + 3);

                    var targetEntity = default(Entity);

                    if (PlayerPed.IsInVehicle())
                    {
                        var vec = PlayerPed.CurrentVehicle;
                        if(!vec.IsSafeExist()) return;
                        targetEntity = vec;
                    }
                    else
                    {
                        targetEntity = PlayerPed;
                    }

                    targetEntity.Position = targetPos;
                    targetEntity.ApplyForce(new Vector3(0, 0, 10));
                });

        }
    }
}
