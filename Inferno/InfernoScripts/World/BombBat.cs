using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using UniRx;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.World
{
    /// <summary>
    /// 人はバットで殴られると爆発する
    /// </summary>
    class BombBat : InfernoScript
    {

        private readonly string Keyword = "batman";
        private List<Ped> pPed = new List<Ped>();

        protected override void Setup()
        {
            pPed.Add(PlayerPed);

            //キーワードが入力されたらON／OFFを切り替える
            CreateInputKeywordAsObservable(Keyword)
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("BombBat:" + IsActive, 3.0f);
                });

            OnAllOnCommandObservable.Subscribe(_ => IsActive = true);

            //interval間隔で実行
            OnThinnedTickAsObservable
                .Where(_ => IsActive)
                .SelectMany(_ => CachedPeds.ToList().Concat(pPed))
                .Where(p => p.IsSafeExist())
                .Subscribe(p => BonBatAction(p));
        }

        private void BonBatAction(Ped ped)
        {
            if (ped.HasBeenDamagedBy(Weapon.BAT))
            {
                if (ped.IsPlayer) GTA.World.AddExplosion(ped.Position, GTA.ExplosionType.Grenade, 40.0f, 0.5f);
                var randomVector = InfernoUtilities.CreateRandomVector();
                ped.ApplyForce(randomVector * Random.Next(800, 1400));
                Function.Call(Hash.CLEAR_PED_LAST_WEAPON_DAMAGE, ped);
            }
            else if (ped.HasBeenDamagedBy(Weapon.KNIFE))
            {
                GTA.World.AddExplosion(ped.Position, GTA.ExplosionType.Molotov1, 0.1f, 0.0f);
                Function.Call(Hash.CLEAR_PED_LAST_WEAPON_DAMAGE, ped);
            }
        }
    }
}
