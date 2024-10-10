using System;
using GTA;
using GTA.Math;
using Inferno.InfernoScripts.InfernoCore.UI;
using Inferno.Properties;
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
                    WarpTo();
                });
        }

        private void WarpTo()
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
        }

        public override bool UseUI => true;
        public override string DisplayName => PlayerLocalize.WarpTitle;
        
        public override string Description => PlayerLocalize.WarpDescription;

        public override bool CanChangeActive => false;
        public override MenuIndex MenuIndex => MenuIndex.Player;

        public override void OnUiMenuConstruct(ObjectPool pool, NativeMenu menu)
        {
            menu.AddButton(
                PlayerLocalize.WarpAction,
                PlayerLocalize.WarpDescription,
                _ => WarpTo()
            );
        }
    }
}