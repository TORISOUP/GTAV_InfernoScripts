using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.ChaosMode.WeaponProvider;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteDebug(false, true)]
    class BeastFriends : ParupunteScript
    {
        private PedHash[] _animalsHash;

        private HashSet<int> _setPeds = new HashSet<int>();

        public BeastFriends(ParupunteCore core) : base(core)
        {
        }

        public override string Name { get; } = "けものフレンズ";

        public override void OnStart()
        {

            if (Random.Next(0, 100) <= 100)
            {
                _animalsHash = new[]
                {
                    PedHash.Acult01AMM,
                    PedHash.Acult02AMY,
                    PedHash.Baywatch01SMY,
                    PedHash.PrisMuscl01SMY,
                };
            }
            else
            {

                #region animal

                _animalsHash =
                    new[]
                    {
                        PedHash.Boar,
                        PedHash.Cat,
                        PedHash.ChickenHawk,
                        PedHash.Chimp,
                        PedHash.Chop,
                        PedHash.Cormorant,
                        PedHash.Cow,
                        PedHash.Coyote,
                        PedHash.Crow,
                        PedHash.Deer,
                        PedHash.Dolphin,
                        PedHash.Fish,
                        PedHash.HammerShark,
                        PedHash.Hen,
                        PedHash.Humpback,
                        PedHash.Husky,
                        PedHash.KillerWhale,
                        PedHash.MountainLion,
                        PedHash.Pig,
                        PedHash.Pigeon,
                        PedHash.Poodle,
                        PedHash.Pug,
                        PedHash.Rabbit,
                        PedHash.Rat,
                        PedHash.Retriever,
                        PedHash.Rhesus,
                        PedHash.Rottweiler,
                        PedHash.Seagull,
                        PedHash.Shepherd,
                        PedHash.Stingray,
                        PedHash.TigerShark,
                        PedHash.Westy
                    };

                #endregion
            }
            ReduceCounter = new ReduceCounter(30 * 1000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());

            StartCoroutine(SpawnCoroutine());
        }

        IEnumerable<object> SpawnCoroutine()
        {

            while (IsActive)
            {
                var playerPos = core.PlayerPed.Position;
                var targetPeds = core.CachedPeds.Where(x =>
                x.IsSafeExist() && x.IsHuman && x.IsAlive && x.IsInRangeOf(playerPos, 30) && !_setPeds.Contains(x.Handle)
                );

                foreach (var ped in targetPeds)
                {
                    var n = ped.IsInVehicle() ? 1 : 3;
                    for (var i = 0; i < n; i++)
                    {
                        var animal = SpawnPed(ped);

                        if (!animal.IsSafeExist()) continue;

                        animal.MarkAsNoLongerNeeded();
                        animal.MaxHealth = 10000;
                        animal.Health = animal.MaxHealth;

                        animal.Task.FightAgainst(ped);
                        _setPeds.Add(ped.Handle);

                    }
                    yield return null;
                }
                yield return null;
            }
        }

        private Ped SpawnPed(Ped targetPed)
        {
            var m = _animalsHash[Random.Next(_animalsHash.Length)];

            if (!targetPed.IsInVehicle())
            {
                var pos = targetPed.Position;
                var spawnPosition = pos.AroundRandom2D((float)Random.NextDouble() * 10.0f);
                return GTA.World.CreatePed(m, spawnPosition);
            }
            else
            {
                var v = targetPed.CurrentVehicle;
                return v.CreatePedOnSeat(VehicleSeat.Passenger, m);
            }

        }
    }

}