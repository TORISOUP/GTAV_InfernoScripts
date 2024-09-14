using System;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteIsono("たくしー")]
    internal class SpawnTaxies : ParupunteScript
    {
        private string name;

        public SpawnTaxies(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnSetUp()
        {
            var honorific = core.PlayerPed.Gender == Gender.Male ? "くん！" : "ちゃん！";
            name = "仲間が増えるよ！やったね" + GetPlayerCharacterName() + honorific;
        }

        public override void OnSetNames()
        {
            Name = name;
        }

        public override void OnStart()
        {
            SpawnTaxi(ActiveCancellationToken).Forget();
        }

        private async ValueTask SpawnTaxi(CancellationToken ct)
        {
            var player = core.PlayerPed;

            for (int i = 0; i < 15; i++)
            {
                var pos = core.PlayerPed.Position.Around(5) + (Vector3.WorldUp * (float)Random.NextDouble() * 30.0f);
                var taxi = GTA.World.CreateVehicle(VehicleHash.Taxi, pos,
                    (float)(Random.NextDouble() * 2f * Math.PI));

                if (taxi.IsSafeExist())
                {
                    taxi.MarkAsNoLongerNeeded();
                    taxi.ApplyForce(-Vector3.WorldUp * 20.0f);
                    var ped = taxi.CreateRandomPedAsDriver();
                    if (ped.IsSafeExist())
                    {
                        ped.MarkAsNoLongerNeeded();
                    }
                }

                await Delay100MsAsync(ct);
            }

            ParupunteEnd();
        }

        private string GetPlayerCharacterName()
        {
            var hash = (PedHash)core.PlayerPed.Model.Hash;
            switch (hash)
            {
                case PedHash.Trevor:
                    return NativeFunctions.GetGXTEntry("BLIP_TREV");

                case PedHash.Michael:
                    return NativeFunctions.GetGXTEntry("BLIP_MICHAEL");

                case PedHash.Franklin:
                    return NativeFunctions.GetGXTEntry("BLIP_FRANKLIN");

                default:
                    return hash.ToString();
            }
        }
    }
}