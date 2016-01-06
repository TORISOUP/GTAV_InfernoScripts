using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GTA; using UniRx;
using GTA.Math;
using GTA.Native;
using Inferno.ChaosMode;
using Inferno.Utilities;


namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    class KillCitizens : ParupunteScript
    {
        public KillCitizens(ParupunteCore core) : base(core)
        {
        }

        public override string Name => "周辺市民全員即死";

        private uint coroutineId = 0;

        public override void OnSetUp()
        {
            
        }

        public override void OnStart()
        {
            coroutineId = StartCoroutine(KillCitizensCoroutine());
        }

        protected override void OnFinished()
        {
            StopCoroutine(coroutineId);
            ParupunteEnd();
        }

        IEnumerable<object> KillCitizensCoroutine()
        {
            var radius = 100.0f;
            var player = core.PlayerPed;
            var peds = core.CachedPeds.Where(x => x.IsSafeExist()
                                               && !x.IsSameEntity(core.PlayerPed)
                                               && !x.IsRequiredForMission()
                                               && x.IsInRangeOf(player.Position, radius)).ToList();
            while (peds.Count > 0)//一気に数十個も同時に爆発を起こせないので時間差で行う
            {
                var removePedList = peds.Take(10);
                foreach (var ped in removePedList)
                {
                    ped.Kill();
                    GTA.World.AddExplosion(ped.Position, GTA.ExplosionType.Rocket, 8.0f, 2.5f);
                }

                peds.RemoveAll(removePedList.Contains);

                yield return null;
            }

            ParupunteEnd();
        }
    }
}
