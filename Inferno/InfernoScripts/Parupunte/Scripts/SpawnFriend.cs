﻿using System;
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
    [ParupunteConfigAttribute("味方召喚", "定時なんで帰ります")]
    [ParupunteIsono("みかたしょうかん")]
    class SpawnFriend :ParupunteScript
    {
        private List<Ped> pedList = new List<Ped>(); 
        public SpawnFriend(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(20 * 1000);
            AddProgressBar(ReduceCounter);
            foreach (var i in Enumerable.Range(0,4))
            {
                pedList.Add(CreateFriend());
            }

            ReduceCounter.OnFinishedAsync.Subscribe(_ =>
            {
                ParupunteEnd();
            });

            this.OnUpdateAsObservable
                .Where(_ => core.PlayerPed.IsDead)
                .FirstOrDefault()
                .Subscribe(_ =>
                {
                    ParupunteEnd();
                });
        }

        protected override void OnFinished()
        {
            foreach (var ped in pedList.Where(ped => ped.IsSafeExist()))
            {
                ped.SetNotChaosPed(false);
                ped.MarkAsNoLongerNeeded();
            }
        }

        private Ped CreateFriend()
        {
            var ped = GTA.World.CreateRandomPed(core.PlayerPed.Position.Around(3));
            if(!ped.IsSafeExist()) return null;
            ped.SetNotChaosPed(true);
            core.PlayerPed.CurrentPedGroup.Add(ped,false);
            ped.MaxHealth = 500;
            ped.Health = ped.MaxHealth;

            var weaponhash = (int)ChaosModeWeapons.GetRandomWeapon();
            ped.SetDropWeaponWhenDead(false); //武器を落とさない
            ped.GiveWeapon(weaponhash, 1000); //指定武器所持
            ped.EquipWeapon(weaponhash); //武器装備

            return ped;
        }

    
    }
}
