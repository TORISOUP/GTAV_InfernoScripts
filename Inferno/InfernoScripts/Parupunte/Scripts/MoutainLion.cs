using GTA;
using GTA.Native;
using System.Collections.Generic;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    internal class MoutainLion : ParupunteScript
    {
        public MoutainLion(ParupunteCore core) : base(core)
        {
        }

        public override string Name { get; } = "近すぎちゃって♪ どうしようもない♪";

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
