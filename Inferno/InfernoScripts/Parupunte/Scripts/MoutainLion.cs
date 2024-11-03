using System.Threading;
using System.Threading.Tasks;
using GTA;
using Inferno.Utilities;

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
            SpawnCharacterAsync(ActiveCancellationToken).Forget();
        }

        private async ValueTask SpawnCharacterAsync(CancellationToken ct)
        {
            var targetTime = core.ElapsedTime + 3f;
            while (core.ElapsedTime < targetTime && !ct.IsCancellationRequested)
            {
                Spawn();
                await YieldAsync(ct);
                Spawn();
                await YieldAsync(ct);
                Spawn();
                await YieldAsync(ct);

                await DelaySecondsAsync(0.5f, ct);
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