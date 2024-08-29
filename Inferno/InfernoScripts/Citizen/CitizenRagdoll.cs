// using System;
// using System.Linq;
// using System.Reactive.Linq;
// using System.Threading;
// using System.Threading.Tasks;
// using System.Windows.Forms;
// using GTA.Math;
// using Inferno.Utilities;
//
// namespace Inferno
// {
//     internal class CitizenRagdoll : InfernoScript
//     {
//         protected override void Setup()
//         {
//             OnKeyDownAsObservable
//                 // 封印
//                 .Where(x => x.KeyCode == Keys.None)
//                 .Subscribe(_ =>
//                 {
//                     DrawText("CitizenRagdoll");
//                     RagdollAsync(DestroyCancellationToken).Forget();
//                 });
//         }
//
//         private async ValueTask RagdollAsync(CancellationToken ct)
//         {
//             var peds = CachedPeds.Where(
//                     x => x.IsSafeExist()
//                          && x.CanRagdoll
//                          && x.IsInRangeOf(PlayerPed.Position, 15))
//                 .ToArray();
//
//             foreach (var ped in peds)
//             {
//                 if (!ped.IsSafeExist())
//                 {
//                     continue;
//                 }
//
//                 ped.SetToRagdoll(100);
//                 ped.ApplyForce(new Vector3(0, 0, 2));
//                 await YieldAsync(ct);
//             }
//         }
//     }
// }