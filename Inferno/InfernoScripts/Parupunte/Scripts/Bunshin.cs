using System;
using System.Collections.Generic;
using System.Linq;
using GTA;
using Inferno.ChaosMode;
using Inferno.ChaosMode.WeaponProvider;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("ブンシンノジツ", "おわり")]
    [ParupunteIsono("ぶんしん")]
    internal class Bunshin : ParupunteScript
    {
        private readonly List<Ped> peds = new();

        public Bunshin(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnStart()
        {
            StartCoroutine(SpawnCoroutine());

            ReduceCounter = new ReduceCounter(20000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());

            OnFinishedAsObservable
                .Subscribe(_ =>
                {
                    foreach (var p in peds)
                        if (p.IsSafeExist())
                        {
                            p.MarkAsNoLongerNeeded();
                        }
                });
        }

        private IEnumerable<object> SpawnCoroutine()
        {
            foreach (var i in Enumerable.Range(0, 12))
            {
                CreatePed(i < 6);
                yield return null;
            }
        }

        private void CreatePed(bool isFriend)
        {
            var ped = GTA.World.CreatePed(core.PlayerPed.Model, core.PlayerPed.Position.Around(Random.Next(5, 10)));
            if (!ped.IsSafeExist())
            {
                return;
            }

            if (isFriend)
            {
                ped.SetNotChaosPed(true);
                core.PlayerPed.PedGroup.Add(ped, false);
                AutoReleaseOnGameEnd(ped);
                peds.Add(ped);
            }
            else
            {
                ped.MarkAsNoLongerNeeded();
            }

            ped.MaxHealth = 500;
            ped.Health = ped.MaxHealth;

            var weaponhash = (int)ChaosModeWeapons.GetRandomWeapon();
            ped.SetDropWeaponWhenDead(false); //武器を落とさない
            ped.GiveWeapon(weaponhash, 1000); //指定武器所持
            ped.EquipWeapon(weaponhash); //武器装備
        }
    }
}