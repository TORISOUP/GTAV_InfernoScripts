using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using Inferno.ChaosMode;
using Inferno.ChaosMode.WeaponProvider;
using UniRx;
namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteIsono("ぶんしん")]
    class Avatar : ParupunteScript
    {
        private List<Ped> peds = new List<Ped>();

        public Avatar(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override ParupunteConfigElement DefaultElement { get; } = new ParupunteConfigElement("ブンシンノジツ", "おわり");

        public override void OnStart()
        {
            StartCoroutine(SpawnCoroutine());

            ReduceCounter = new ReduceCounter(20000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());

            this.OnFinishedAsObservable
                .Subscribe(_ =>
                {
                    foreach (var p in peds)
                    {
                        if (p.IsSafeExist()) p.MarkAsNoLongerNeeded();
                    }
                });
        }

        IEnumerable<object> SpawnCoroutine()
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
            if (!ped.IsSafeExist()) return;

            if (isFriend)
            {
                ped.SetNotChaosPed(true);
                core.PlayerPed.CurrentPedGroup.Add(ped, false);
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
