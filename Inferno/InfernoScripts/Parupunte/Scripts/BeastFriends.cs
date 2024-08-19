using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using System.Linq;
using System.Reactive.Linq;
using System;
using System.Reactive;
using System.Reactive.Subjects;

using GTA.Math;
using GTA.Native;
using Inferno.ChaosMode.WeaponProvider;


namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteIsono("けものふれんず")]
    class BeastFriends : ParupunteScript
    {
        private PedHash[] _animalsHash;

        private HashSet<int> _setEntities = new HashSet<int>();

        private bool isBeast = false;

        public BeastFriends(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
            isBeast = Random.Next(0, 100) <= 80;
        }

        public override void OnSetNames()
        {
            Name = isBeast ? "けものフレンズ" : "のけものフレンズ";
            EndMessage = () => "おわり";
        }

        public override void OnStart()
        {

            if (!isBeast)
            {
                _animalsHash = new[]
                {
                    PedHash.Babyd,
                    PedHash.Baywatch01SMY,
                    PedHash.PrisMuscl01SMY,
                    PedHash.Chimp,
                    PedHash.Devin,
                    PedHash.Runner01AFY,
                    PedHash.SteveHains
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
                        PedHash.Westy
                    };

                #endregion
            }
            ReduceCounter = new ReduceCounter(15 * 1000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());

            StartCoroutine(SpawnCoroutineToPed());
            StartCoroutine(SpawnCoroutineToVehicle());
        }

        IEnumerable<object> SpawnCoroutineToVehicle()
        {
            while (true)
            {
                var centerPos = core.PlayerPed.Position;

                var targetVehs = core.CachedVehicles.Where(x =>
                x.IsSafeExist() && x.IsAlive && x.IsInRangeOf(centerPos, 30) && !_setEntities.Contains(x.Handle) && !x.Driver.IsSafeExist());

                foreach (var veh in targetVehs)
                {
                    for (var i = 0; i < 4; i++)
                    {
                        var m = _animalsHash[Random.Next(_animalsHash.Length)];

                        var seat = new[] { VehicleSeat.Driver, VehicleSeat.Passenger, VehicleSeat.RightRear, VehicleSeat.LeftRear }[i];
                        var animal = veh.CreatePedOnSeat(seat, m);

                        if (!animal.IsSafeExist()) continue;

                        animal.MarkAsNoLongerNeeded();
                        animal.MaxHealth = 10000;
                        animal.Health = animal.MaxHealth;
                        animal.Task.FightAgainst(core.PlayerPed);
                        _setEntities.Add(animal.Handle);
                        _setEntities.Add(veh.Handle);

                    }
                    yield return null;
                }
                yield return WaitForSeconds(0.2f);
            }
        }

        IEnumerable<object> SpawnCoroutineToPed()
        {

            while (IsActive)
            {
                var playerPos = core.PlayerPed.Position;

                var targetPeds = core.CachedPeds.Where(x =>
                x.IsSafeExist() && x.IsHuman && x.IsAlive && x.IsInRangeOf(playerPos, 20) && !_setEntities.Contains(x.Handle));

                foreach (var ped in targetPeds)
                {
                    for (var i = 0; i < 3; i++)
                    {
                        var animal = SpawnPed(ped, i);

                        if (!animal.IsSafeExist()) continue;

                        animal.MarkAsNoLongerNeeded();
                        animal.MaxHealth = 10000;
                        animal.Health = animal.MaxHealth;
                        _setEntities.Add(animal.Handle);
                        animal.Task.FightAgainst(ped);
                        _setEntities.Add(ped.Handle);

                    }
                    yield return null;
                }
                yield return WaitForSeconds(0.25f);
            }
        }

        /// <summary>
        /// フレンズを召喚する
        /// </summary>
        private Ped SpawnPed(Ped targetPed, int index)
        {
            var m = _animalsHash[Random.Next(_animalsHash.Length)];

            if (!targetPed.IsInVehicle())
            {
                var pos = targetPed.Position;
                var spawnPosition = pos.AroundRandom2D(2 + (float)Random.NextDouble() * 15.0f);
                return GTA.World.CreatePed(m, spawnPosition);
            }
            else
            {
                if (index >= 3) return null;
                var seat = new[] { VehicleSeat.Passenger, VehicleSeat.RightRear, VehicleSeat.LeftRear }[index];
                var v = targetPed.CurrentVehicle;
                _setEntities.Add(v.Handle);
                return v.CreatePedOnSeat(seat, m);
            }
        }

    }
}