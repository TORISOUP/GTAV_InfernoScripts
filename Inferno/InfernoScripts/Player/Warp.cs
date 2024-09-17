using System;
using GTA;
using GTA.Math;

namespace Inferno.InfernoScripts.Player
{
    internal class Warp : InfernoScript
    {
        protected override void Setup()
        {
            CreateInputKeywordAsObservable("moveto")
                .Subscribe(_ =>
                {
                    var blip = GTA.World.WaypointBlip;
                    if (blip == null)
                    {
                        return;
                    }

                    var targetHeight = GTA.World.GetGroundHeight(blip.Position);
                    //地面ピッタリだと地面に埋まるので少し上空を指定する
                    var targetPos = new Vector3(blip.Position.X, blip.Position.Y, targetHeight + 0.1f);

                    var tryPos = GTA.World.GetSafeCoordForPed(targetPos);
                    if (tryPos != Vector3.Zero)
                    {
                        targetPos = tryPos;
                    }

                    var targetEntity = default(Entity);

                    if (PlayerPed.IsInVehicle())
                    {
                        var vec = PlayerPed.CurrentVehicle;
                        if (!vec.IsSafeExist())
                        {
                            return;
                        }

                        targetEntity = vec;
                    }
                    else
                    {
                        targetEntity = PlayerPed;
                    }

                    targetEntity.Position = targetPos;
                    targetEntity.ApplyForce(new Vector3(0, 0, 1));
                });
        }
    }
}