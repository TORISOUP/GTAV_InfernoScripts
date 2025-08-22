// using System;
// using System.Linq;
// using GTA;
// using GTA.Native;
//
// namespace Inferno.InfernoScripts.Parupunte.Scripts
// {
//     [ParupunteConfigAttribute("変身GOGOベイビー")]
//     [ParupunteDebug(true)]
//     internal class Transform : ParupunteScript
//     {
//         public Transform(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
//         {
//         }
//
//         public override void OnStart()
//         {
//             ReduceCounter = new ReduceCounter(30 * 1000);
//             AddProgressBar(ReduceCounter);
//             ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());
//             var initialModel = core.PlayerPed.Model;
//
//             var hashed = Enum.GetValues(typeof(PedHash)).Cast<PedHash>().ToArray();
//             var targetHash = hashed[Random.Next(hashed.Length)];
//             var targetModel = new Model(targetHash);
//
//             var old = OutfitHelper.SaveOutfit(core.PlayerPed);
//
//             Game.Player.ChangeModel(targetModel);
//             OnFinishedAsObservable
//                 .Subscribe(_ =>
//                 {
//                     Game.Player.ChangeModel(initialModel);
//                     OutfitHelper.ApplyOutfit(core.PlayerPed, old);
//                 });
//         }
//
//
//         public struct ComponentState
//         {
//             public int Drawable, Texture, Palette;
//         }
//
//         public struct PropState
//         {
//             public int Index, Texture; // Index=-1 で未装備
//         }
//
//         public class OutfitState
//         {
//             public ComponentState[] Components = new ComponentState[12];
//             public PropState[] Props = new PropState[10];
//         }
//
//         public static class OutfitHelper
//         {
//             public static OutfitState SaveOutfit(Ped ped)
//             {
//                 var o = new OutfitState();
//                 // components
//                 for (int c = 0; c <= 11; c++)
//                 {
//                     o.Components[c] = new ComponentState
//                     {
//                         Drawable = Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, ped.Handle, c),
//                         Texture = Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, ped.Handle, c),
//                         Palette = Function.Call<int>(Hash.GET_PED_PALETTE_VARIATION, ped.Handle, c),
//                     };
//                 }
//
//                 // props（0..9のうち主に 0,1,2,6,7 を使う）
//                 for (int p = 0; p <= 9; p++)
//                 {
//                     int idx = Function.Call<int>(Hash.GET_PED_PROP_INDEX, ped.Handle, p);
//                     int tex = (idx >= 0)
//                         ? Function.Call<int>(Hash.GET_PED_PROP_TEXTURE_INDEX, ped.Handle, p)
//                         : 0;
//
//                     o.Props[p] = new PropState { Index = idx, Texture = tex };
//                 }
//
//                 return o;
//             }
//
//             public static float Clamp(float value, float min, float max)
//             {
//                 if (value < min)
//                 {
//                     return min;
//                 }
//
//                 if (value > max)
//                 {
//                     return max;
//                 }
//
//                 return value;
//             }
//
//             public static int Clamp(int value, int min, int max)
//             {
//                 if (value < min)
//                 {
//                     return min;
//                 }
//
//                 if (value > max)
//                 {
//                     return max;
//                 }
//
//                 return value;
//             }
//
//             public static void ApplyOutfit(Ped ped, OutfitState o)
//             {
//                 // 安全側：存在確認してから適用
//                 for (int c = 0; c <= 11; c++)
//                 {
//                     var st = o.Components[c];
//                     // drawable の存在確認
//                     int dCount = Function.Call<int>(Hash.GET_NUMBER_OF_PED_DRAWABLE_VARIATIONS, ped.Handle, c);
//                     if (dCount <= 0) continue;
//                     int drawable = Clamp(st.Drawable, 0, dCount - 1);
//
//                     // texture の存在確認
//                     int tCount = Function.Call<int>(Hash.GET_NUMBER_OF_PED_TEXTURE_VARIATIONS, ped.Handle, c, drawable);
//                     int texture = (tCount > 0) ? Clamp(st.Texture, 0, tCount - 1) : 0;
//
//                     // palette は 0..3 が多い（モデルにより異なる）ので 0–3 に丸め
//                     int palette = Clamp(st.Palette, 0, 3);
//
//                     Function.Call(Hash.SET_PED_COMPONENT_VARIATION, ped.Handle, c, drawable, texture, palette);
//                 }
//
//                 // props
//                 for (int p = 0; p <= 9; p++)
//                 {
//                     var st = o.Props[p];
//                     if (st.Index < 0)
//                     {
//                         Function.Call(Hash.CLEAR_PED_PROP, ped.Handle, p);
//                         continue;
//                     }
//
//                     int dCount = Function.Call<int>(Hash.GET_NUMBER_OF_PED_PROP_DRAWABLE_VARIATIONS, ped.Handle, p);
//                     if (dCount <= 0)
//                     {
//                         Function.Call(Hash.CLEAR_PED_PROP, ped.Handle, p);
//                         continue;
//                     }
//
//                     int drawable = Clamp(st.Index, 0, dCount - 1);
//                     int tCount = Function.Call<int>(Hash.GET_NUMBER_OF_PED_PROP_TEXTURE_VARIATIONS, ped.Handle, p,
//                         drawable);
//                     int texture = (tCount > 0) ? Clamp(st.Texture, 0, tCount - 1) : 0;
//
//                     Function.Call(Hash.SET_PED_PROP_INDEX, ped.Handle, p, drawable, texture, true);
//                 }
//             }
//         }
//     }
// }