using GTA;
using System.Linq;
using System.Reactive.Linq;
using System;
using System.Reactive;
using System.Reactive.Subjects;

using GTA.Native;
using System.Collections.Generic;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("近すぎちゃって♪ どうしようもない♪")]
    [ParupunteIsono("くーがー")]
    internal class MoutainLion : ParupunteScript
    {
        public MoutainLion(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnStart()
        {
            StartCoroutine(SpawnCharacter());
        }

        private IEnumerable<object> SpawnCharacter()
        {
            foreach (var s in WaitForSeconds(2))
            {
                Spawn();
                Spawn();
                yield return s;
            }
            ParupunteEnd();
        }

        private void Spawn()
        {
            var player = core.PlayerPed;
            var lion = GTA.World.CreatePed(new Model(PedHash.MountainLion), player.Position.Around(4));
            if (lion.IsSafeExist())
            {
                lion.MarkAsNoLongerNeeded();
                lion.MaxHealth = 10000;
                lion.Health = lion.MaxHealth;
                lion.Task.FightAgainst(core.PlayerPed);
            }
        }
    }
}
