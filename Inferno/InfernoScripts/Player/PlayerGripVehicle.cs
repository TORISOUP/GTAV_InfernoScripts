using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;

namespace Inferno.InfernoScripts.Player
{
    class PlayerGripVehicle : InfernoScript
    {
        private bool _isGriped = false;
        private Vehicle _vehicle;
        private Vector3 _ofsetPosition;

        protected override void Setup()
        {
            OnTickAsObservable
                .Where(_ => this.IsGamePadPressed(GameKey.Aim))
                .Subscribe(_ => GripAction());

            OnTickAsObservable
                .Where(_ => _isGriped)
                .Subscribe(_ =>
                {
                    var player = PlayerPed;
                    Grip(player, _vehicle, _ofsetPosition);
                });

            OnTickAsObservable
                .Where(_ => _isGriped && !this.IsGamePadPressed(GameKey.Aim))
                .Subscribe(_ => GripRemove());
        }

        /// <summary>
        /// 車両から手を離す
        /// </summary>
        private void GripRemove()
        {
            var player = PlayerPed;
            player.IsInvincible = false;
            _isGriped = false;
            Function.Call(Hash.DETACH_ENTITY, player, false, false);
            player.Task.Skydive();
            player.Task.ClearAll();

        }

        /// <summary>
        /// 掴む車両の選別
        /// </summary>
        private void GripAction()
        {
            var player = PlayerPed;
            var gripAvailableVeles = CachedVehicles
                            .Where(x => x.IsSafeExist() && x.IsInRangeOf(player.Position, 10.0f));
            foreach (var veh in gripAvailableVeles)
            {
                var isTouchingEntity = Function.Call<bool>(Hash.IS_ENTITY_TOUCHING_ENTITY, player, veh);
                if (!isTouchingEntity) continue;
                _isGriped = true;
                var playerRHandCoords = player.GetBoneCoord(Bone.SKEL_R_Hand);

                var ofsetPosition = Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_GIVEN_WORLD_COORDS,
                    veh,
                    playerRHandCoords.X,
                    playerRHandCoords.Y,
                    playerRHandCoords.Z);
                Grip(player, veh, ofsetPosition);
            }
        }

        /// <summary>
        /// 車両を掴む処理
        /// </summary>
        /// <param name="player"></param>
        /// <param name="vehicle"></param>
        /// <param name="ofsetPosition"></param>
        private void Grip(Ped player, Vehicle vehicle, Vector3 ofsetPosition)
        {
            player.SetToRagdoll(0, 1);
            player.IsInvincible = true;
            _vehicle = vehicle;
            _ofsetPosition = ofsetPosition;
            var forceToBreak = 99999.0f;
            var rotation = new Vector3(0.0f, 0.0f, 0.0f);
            var isCollision = true;
            Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY_PHYSICALLY,
                player,
                vehicle,
                player.GetBoneIndex(Bone.SKEL_R_Hand),
                vehicle.GetBoneIndex("SKEL_ROOT"),
                ofsetPosition.X,
                ofsetPosition.Y,
                ofsetPosition.Z,
                0.0f,
                0.0f,
                0.0f,
                rotation.X,
                rotation.Y,
                rotation.Z,
                forceToBreak,
                false, //?
                false, //?
                isCollision,
                false, //?
                2); //?
        }
    }
}
