using System;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.InfernoScripts.InfernoCore.UI;
using Inferno.Properties;
using Inferno.Utilities;
using LemonUI;
using LemonUI.Menus;

namespace Inferno.InfernoScripts.Player
{
    internal class Warp : InfernoScript
    {
        protected override void Setup()
        {
            CreateInputKeywordAsObservable("WarpToWaypoint","moveto")
                .Subscribe(_ =>
                {
                    WarpToAsync().Forget();
                });
        }

        private async ValueTask WarpToAsync()
        {
            var blip = GTA.World.WaypointBlip;
            if (blip == null)
            {
                return;
            }
            
            Function.Call(Hash.REQUEST_COLLISION_AT_COORD, blip.Position.X, blip.Position.Y, blip.Position.Z);
            await YieldAsync();
            
            var targetHeight = GTA.World.GetGroundHeight(blip.Position);
            //地面ピッタリだと地面に埋まるので少し上空を指定する
            var targetPos = new Vector3(blip.Position.X, blip.Position.Y, targetHeight + 0.1f);

            var tryPos = GTA.World.GetSafeCoordForPed(targetPos);
            if (tryPos != Vector3.Zero)
            {
                targetPos = tryPos;
            }
            Function.Call(Hash.REQUEST_COLLISION_AT_COORD, targetPos.X, targetPos.Y, targetPos.Z);
            await YieldAsync();


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

            targetEntity.PositionNoOffset = targetPos;
            targetEntity.ApplyForce(new Vector3(0, 0, 1));
        }

        public override bool UseUI => true;
        public override string DisplayName => PlayerLocalize.WarpTitle;
        
        public override string Description => PlayerLocalize.WarpDescription;

        public override bool CanChangeActive => false;
        public override MenuIndex MenuIndex => MenuIndex.Misc;

        public override void OnUiMenuConstruct(ObjectPool pool, NativeMenu menu)
        {
            menu.AddButton(
                PlayerLocalize.WarpAction,
                PlayerLocalize.WarpDescription,
                _ => WarpToAsync().Forget()
            );
        }
    }
}