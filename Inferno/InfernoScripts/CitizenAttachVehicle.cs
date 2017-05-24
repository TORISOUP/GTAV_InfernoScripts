using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.Utilities;
using UniRx;

namespace Inferno.InfernoScripts
{
    internal class CitizenAttachVehicleConfig : InfernoConfig
    {
        public string EnableKeyCode { get; set; } = "K";
        public string DisableKeyCode { get; set; } = "J";

        public override bool Validate()
        {
            return !(string.IsNullOrEmpty(EnableKeyCode) || string.IsNullOrEmpty(DisableKeyCode));
        }
    }

    /// <summary>
    /// 周辺市民を近くの車に引っ付ける
    /// </summary>
    class CitizenAttachVehicle : InfernoScript
    {
        #region conf
        protected override string ConfigFileName { get; } = "CitizenAttachVehicle.conf";
        private CitizenAttachVehicleConfig config;
        private string EnableKeyCode => config?.EnableKeyCode ?? "K";
        private string DisableKeyCode => config?.DisableKeyCode ?? "J";
        #endregion

        /// <summary>
        /// 処理中の市民IDリスト
        /// 毎回判定するのでHashSet
        /// </summary>
        private HashSet<int> processingPedIdSet = new HashSet<int>();
        private List<Ped> processingPedList = new List<Ped>();


        protected override void Setup()
        {
            config = LoadConfig<CitizenAttachVehicleConfig>();

            this.OnKeyDownAsObservable
                .Where(x => PlayerPed.IsInVehicle() && x.KeyCode.ToString() == EnableKeyCode)
                .Subscribe(_ =>
                {
                    DrawText("GrabVehicle");
                    StartAttachAction();
                });

            this.OnKeyDownAsObservable
                .Where(x => x.KeyCode.ToString() == DisableKeyCode)
                .Subscribe(_ =>
                {
                    DrawText("ReleaseVehicle");
                    ReleaseAll();
                });

            //車から降りたら終了
            this.OnThinnedTickAsObservable
                .Select(_ => PlayerPed.IsInVehicle())
                .DistinctUntilChanged()
                .Where(x => !x)
                .Subscribe(_ => ReleaseAll());

            this.OnAbortAsync
                .Subscribe(_ => ReleaseAll());
        }

        void ReleaseAll()
        {
            StopAllCoroutine();
            processingPedIdSet.Clear();
            foreach (var p in processingPedList.Where(x => x.IsSafeExist()))
            {
                SetPedProof(p, false);
                ReleaseVehicle(p);
            }
            processingPedList.Clear();
        }

        /// <summary>
        /// 周辺市民を車にくっつける
        /// </summary>
        void StartAttachAction()
        {
            var playerVeh = PlayerVehicle.Value;
            //プレイヤ周辺市民
            var peds = CachedPeds.Where(x => x.IsSafeExist()
                                             && x.IsAlive
                                             && !x.IsInVehicle()
                                             && x.IsInRangeOf(playerVeh.Position, 7)
                                             && !x.IsCutsceneOnlyPed());


            foreach (var p in peds
                .Where(x => !processingPedIdSet.Contains(x.Handle))
                .Where(p => playerVeh.IsSafeExist()))
            {
                processingPedIdSet.Add(p.Handle);
                StartCoroutine(PedAttachCoroutine(p, playerVeh));
            }
        }

        /// <summary>
        /// 対象が有効な状態であるか
        /// </summary>
        bool IsEnableStatus(Ped ped, Vehicle veh)
        {
            return ped.IsSafeExist() && ped.IsAlive && veh.IsSafeExist() && veh.IsAlive;
        }

        private void SetPedProof(Ped ped, bool hasCollisionProof)
        {
            ped.SetProofs(
                ped.IsBulletProof,
                ped.IsFireProof,
                ped.IsExplosionProof,
                hasCollisionProof,
                ped.IsMeleeProof,
                false, false, false);
        }

        /// <summary>
        /// くっつけるコルーチン
        /// </summary>
        IEnumerable<object> PedAttachCoroutine(Ped ped, Vehicle veh)
        {
            var handleId = ped.Handle;
            ped.SetToRagdoll(1, 2);
            SetPedProof(ped, true);

            //ターゲットが車に触るまでforceを加える
            foreach (var s in WaitForSeconds(3))
            {
                if (!IsEnableStatus(ped, veh))
                {
                    processingPedIdSet.Remove(ped.Handle);
                    yield break;
                }

                //触ったら終わり
                if (ped.IsTouching(veh)) break;

                //車に向かって引っ張る
                var dir = (veh.Position + Vector3.WorldUp * 2.0f - ped.Position).Normalized;
                ped.ApplyForce(dir * 3.0f, Vector3.Zero, ForceType.MaxForceRot2);
                yield return null;
            }

            //オブジェクトが消失した、または車に触っていなかったら終了
            if (!IsEnableStatus(ped, veh) || !ped.IsTouching(veh))
            {
                if (ped.IsSafeExist()) { SetPedProof(ped, false); }
                processingPedIdSet.Remove(ped.Handle);
                yield break;
            }

            var rHandCoord = ped.GetBoneCoord(Bone.SKEL_R_Hand);
            var offsetPosition = Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_GIVEN_WORLD_COORDS,
                veh,
                rHandCoord.X,
                rHandCoord.Y,
                rHandCoord.Z);

            //掴み始めたら追加
            processingPedList.Add(ped);

            //掴みループ
            while (IsEnableStatus(ped, veh))
            {
                GripVehicle(ped, veh, offsetPosition);
                yield return null;
            }

            if (ped.IsSafeExist())
            {
                ReleaseVehicle(ped);
            }
            processingPedIdSet.Remove(handleId);
        }


        /// <summary>
        /// 車両を掴む
        /// </summary>
        private void GripVehicle(Ped ped, Vehicle vehicle, Vector3 ofsetPosition)
        {
            ped.SetToRagdoll(0, 1);
            SetPedProof(ped, true);
            var forceToBreak = 10000.0f;
            var rotation = new Vector3(0.0f, 0.0f, 0.0f);
            var isCollision = true;
            Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY_PHYSICALLY,
                ped,
                vehicle,
                ped.GetBoneIndex(Bone.SKEL_R_Hand),
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

        /// <summary>
        /// 車両から手を離す
        /// </summary>
        private void ReleaseVehicle(Ped ped)
        {
            Function.Call(Hash.DETACH_ENTITY, ped, false, false);
            ped.SetToRagdoll();
        }

    }
}
