using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using Inferno.ChaosMode;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteIsono("けものふれんず")]
    internal class BeastFriends : ParupunteScript
    {
        private readonly HashSet<int> _setEntities = new();

        private readonly bool isBeast;
        private PedHash[] _animalsHash;

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
                        PedHash.Westy
                    };

                #endregion
            }

            CreateAsync(ActiveCancellationToken).Forget();
        }

        private async ValueTask CreateAsync(CancellationToken ct)
        {
            try
            {
                var player = core.PlayerPed;
                for (int i = 0; i < 100; i++)
                {
                    try
                    {
                        var ped = GTA.World.CreatePed(GetModel(), player.Position.AroundRandom2D(50)
                                                                  + player.Velocity + (Vector3.UnitY *
                                                                      (float)Random.NextDouble() * 15f));
                        if (ped.IsSafeExist())
                        {
                            if (!ped.IsHuman)
                            {
                                ped.SetNotChaosPed(true);
                            }

                            ped.MarkAsNoLongerNeeded();
                            ped.Task.FightAgainst(player, 30000);
                            ped.IsInvincible = true;

                        }
                    }
                    finally
                    {
                        await YieldAsync(ct);
                    }
                }
            }
            finally
            {
                ParupunteEnd();
            }
        }


        private Model GetModel()
        {
            var model = _animalsHash[Random.Next(0, _animalsHash.Length)];
            return new Model(model);
        }
    }
}