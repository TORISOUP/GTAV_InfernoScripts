using System;
using System.Linq;
using System.Reactive.Linq;
using GTA;
using GTA.Math;
using GTA.Native;

namespace Inferno.InfernoScripts.Player
{
    internal class PlayerGripVehicle : InfernoScript
    {
        private bool _isGriped;
        private Vector3 _ofsetPosition;
        private Vehicle _vehicle;

        protected override void Setup()
        {
            //0.3秒間押しっぱなしなら発動
            OnThinnedTickAsObservable
                .Select(_ => !this.GetPlayerVehicle().IsSafeExist()
                             && !_isGriped
                             && this.IsGamePadPressed(GameKey.Reload)
                )
                .Buffer(3, 1)
                .Where(x => x.All(v => v))
                .Subscribe(_ => GripAction());

            OnThinnedTickAsObservable
                .Where(_ => _isGriped)
                .Subscribe(_ => { Grip(PlayerPed, _vehicle, _ofsetPosition); });

            OnThinnedTickAsObservable
                .Where(_ => _isGriped && (!this.IsGamePadPressed(GameKey.Reload) || PlayerPed.IsDead))
                .Subscribe(_ => GripRemove());

            OnAbortAsync.Subscribe(_ => { SetPlayerProof(false); });
        }

        /// <summary>
        /// 車両から手を離す
        /// </summary>
        private void GripRemove()
        {
            SetPlayerProof(false);
            _isGriped = false;
            Function.Call(Hash.DETACH_ENTITY, PlayerPed, false, false);
            PlayerPed.Task.ClearAllImmediately();
            PlayerPed.SetToRagdoll();
        }

        private void SetPlayerProof(bool hasCollisionProof)
        {
            PlayerPed.SetProofs(
                PlayerPed.IsBulletProof,
                PlayerPed.IsFireProof,
                PlayerPed.IsExplosionProof,
                hasCollisionProof,
                PlayerPed.IsMeleeProof,
                false, false, false);
        }

        /// <summary>
        /// 掴む車両の選別
        /// </summary>
        private void GripAction()
        {
            var gripAvailableVeles = CachedVehicles
                .Where(x => x.IsSafeExist() && x.IsInRangeOf(PlayerPed.Position, 10.0f))
                .OrderBy(x => x.Position.DistanceTo(PlayerPed.Position))
                .FirstOrDefault();

            if (gripAvailableVeles == null || !gripAvailableVeles.IsTouching(PlayerPed)) return;
            _isGriped = true;
            var playerRHandCoords = PlayerPed.GetBonePosition(Bone.SkelRightHand);

            var offsetPosition = Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_GIVEN_WORLD_COORDS,
                gripAvailableVeles,
                playerRHandCoords.X,
                playerRHandCoords.Y,
                playerRHandCoords.Z);
            Grip(PlayerPed, gripAvailableVeles, offsetPosition);
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
            SetPlayerProof(true);
            _vehicle = vehicle;
            _ofsetPosition = ofsetPosition;
            var forceToBreak = 99999.0f;
            var rotation = new Vector3(0.0f, 0.0f, 0.0f);
            var isCollision = true;
            Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY_PHYSICALLY,
                player,
                vehicle,
                Bone.SkelRightHand,
                Bone.SkelRoot,
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