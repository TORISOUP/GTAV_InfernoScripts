// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Reactive.Linq;
// using System.Threading;
// using System.Threading.Tasks;
// using GTA;
// using GTA.Math;
// using GTA.Native;
// using Inferno.Utilities;
//
// namespace Inferno.InfernoScripts
// {
//     // 封印
//     internal class CitizenAttachVehicleConfig : InfernoConfig
//     {
//         public string EnableKeyCode { get; set; } = "K";
//         public string DisableKeyCode { get; set; } = "J";
//
//         public override bool Validate()
//         {
//             return !(string.IsNullOrEmpty(EnableKeyCode) || string.IsNullOrEmpty(DisableKeyCode));
//         }
//     }
//
//     /// <summary>
//     /// 周辺市民を近くの車に引っ付ける
//     /// </summary>
//     internal class CitizenAttachVehicle : InfernoScript
//     {
//         /// <summary>
//         /// 処理中の市民IDリスト
//         /// 毎回判定するのでHashSet
//         /// </summary>
//         private readonly HashSet<int> processingPedIdSet = new();
//
//         private readonly List<Ped> processingPedList = new();
//         private CancellationTokenSource _cts;
//
//         protected override void Setup()
//         {
//             config = LoadConfig<CitizenAttachVehicleConfig>();
//
//             OnKeyDownAsObservable
//                 .Where(x => PlayerPed.IsInVehicle() && x.KeyCode.ToString() == EnableKeyCode)
//                 .Subscribe(_ =>
//                 {
//                     _cts?.Cancel();
//                     _cts?.Dispose();
//                     _cts = new CancellationTokenSource();
//
//                     DrawText("GrabVehicle");
//                     StartAttachAction();
//                 });
//
//             OnKeyDownAsObservable
//                 .Where(x => x.KeyCode.ToString() == DisableKeyCode)
//                 .Subscribe(_ =>
//                 {
//                     DrawText("ReleaseVehicle");
//                     ReleaseAll();
//                 });
//
//             //車から降りたら終了
//             OnThinnedTickAsObservable
//                 .Select(_ => PlayerPed.IsInVehicle())
//                 .DistinctUntilChanged()
//                 .Where(x => !x)
//                 .Subscribe(_ => ReleaseAll());
//
//             OnAbortAsync
//                 .Subscribe(_ => ReleaseAll());
//         }
//
//         private void ReleaseAll()
//         {
//             _cts?.Cancel();
//             _cts?.Dispose();
//             _cts = null;
//             
//             processingPedIdSet.Clear();
//             foreach (var p in processingPedList.Where(x => x.IsSafeExist()))
//             {
//                 SetPedProof(p, false);
//                 ReleaseVehicle(p);
//             }
//
//             processingPedList.Clear();
//         }
//
//         /// <summary>
//         /// 周辺市民を車にくっつける
//         /// </summary>
//         private void StartAttachAction()
//         {
//             var playerVeh = PlayerVehicle.Value;
//             //プレイヤ周辺市民
//             var peds = CachedPeds.Where(x => x.IsSafeExist()
//                                              && x.IsAlive
//                                              && !x.IsInVehicle()
//                                              && x.IsInRangeOf(playerVeh.Position, 7)
//                                              && !x.IsCutsceneOnlyPed());
//
//
//             foreach (var p in peds
//                          .Where(x => !processingPedIdSet.Contains(x.Handle))
//                          .Where(p => playerVeh.IsSafeExist()))
//             {
//                 processingPedIdSet.Add(p.Handle);
//                 PedAttachAsync(p, playerVeh, _cts.Token).Forget();
//             }
//         }
//
//         /// <summary>
//         /// 対象が有効な状態であるか
//         /// </summary>
//         private bool IsEnableStatus(Ped ped, Vehicle veh)
//         {
//             return ped.IsSafeExist() && ped.IsAlive && veh.IsSafeExist() && veh.IsAlive;
//         }
//
//         private void SetPedProof(Ped ped, bool hasCollisionProof)
//         {
//             ped.SetProofs(
//                 ped.IsBulletProof,
//                 ped.IsFireProof,
//                 ped.IsExplosionProof,
//                 hasCollisionProof,
//                 ped.IsMeleeProof,
//                 false, false, false);
//         }
//
//         /// <summary>
//         /// くっつけるコルーチン
//         /// </summary>
//         private async ValueTask PedAttachAsync(Ped ped, Vehicle veh, CancellationToken ct)
//         {
//             var handleId = ped.Handle;
//             ped.SetToRagdoll(1, 2);
//             SetPedProof(ped, true);
//
//             //ターゲットが車に触るまでforceを加える
//             var targetTime = ElapsedTime + 3;
//             while (ElapsedTime < targetTime)
//             {
//                 if (!IsEnableStatus(ped, veh))
//                 {
//                     processingPedIdSet.Remove(ped.Handle);
//                     return;
//                 }
//
//                 //触ったら終わり
//                 if (ped.IsTouching(veh))
//                 {
//                     break;
//                 }
//
//                 //車に向かって引っ張る
//                 var dir = (veh.Position + Vector3.WorldUp * 2.0f - ped.Position).Normalized;
//                 ped.ApplyForce(dir * 3.0f, Vector3.Zero);
//                 await DelaySecondsAsync(0.1f, ct);
//             }
//
//             //オブジェクトが消失した、または車に触っていなかったら終了
//             if (!IsEnableStatus(ped, veh) || !ped.IsTouching(veh))
//             {
//                 if (ped.IsSafeExist())
//                 {
//                     SetPedProof(ped, false);
//                 }
//
//                 processingPedIdSet.Remove(ped.Handle);
//                 return;
//             }
//
//             var rHandCoord = ped.GetBonePosition(Bone.SkelRightHand);
//             var offsetPosition = Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_GIVEN_WORLD_COORDS,
//                 veh,
//                 rHandCoord.X,
//                 rHandCoord.Y,
//                 rHandCoord.Z);
//
//             //掴み始めたら追加
//             processingPedList.Add(ped);
//
//             //掴みループ
//             while (IsEnableStatus(ped, veh))
//             {
//                 GripVehicle(ped, veh, offsetPosition);
//                 await DelaySecondsAsync(1, ct);
//             }
//
//             if (ped.IsSafeExist())
//             {
//                 ReleaseVehicle(ped);
//             }
//
//             processingPedIdSet.Remove(handleId);
//         }
//
//
//         /// <summary>
//         /// 車両を掴む
//         /// </summary>
//         private void GripVehicle(Ped ped, Vehicle vehicle, Vector3 ofsetPosition)
//         {
//             ped.SetToRagdoll(0, 1);
//             SetPedProof(ped, true);
//             var forceToBreak = 10000.0f;
//             var rotation = new Vector3(0.0f, 0.0f, 0.0f);
//             var isCollision = true;
//             Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY_PHYSICALLY,
//                 ped,
//                 vehicle,
//                 Bone.SkelRightHand,
//                 Bone.SkelRoot,
//                 ofsetPosition.X,
//                 ofsetPosition.Y,
//                 ofsetPosition.Z,
//                 0.0f,
//                 0.0f,
//                 0.0f,
//                 rotation.X,
//                 rotation.Y,
//                 rotation.Z,
//                 forceToBreak,
//                 false, //?
//                 false, //?
//                 isCollision,
//                 false, //?
//                 2); //?
//         }
//
//         /// <summary>
//         /// 車両から手を離す
//         /// </summary>
//         private void ReleaseVehicle(Ped ped)
//         {
//             Function.Call(Hash.DETACH_ENTITY, ped, false, false);
//             ped.SetToRagdoll();
//         }
//
//         #region conf
//
//         protected override string ConfigFileName { get; } = "CitizenAttachVehicle.conf";
//         private CitizenAttachVehicleConfig config;
//         private string EnableKeyCode => config?.EnableKeyCode ?? "K";
//         private string DisableKeyCode => config?.DisableKeyCode ?? "J";
//
//         #endregion
//     }
// }