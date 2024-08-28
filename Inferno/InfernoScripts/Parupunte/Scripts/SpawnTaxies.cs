using System.Collections.Generic;
using GTA;

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
            StartCoroutine(SpawnTaxi());
        }

        private IEnumerable<object> SpawnTaxi()
        {
            var player = core.PlayerPed;

            foreach (var s in WaitForSeconds(1))
            {
                var taxi = GTA.World.CreateVehicle(VehicleHash.Taxi, player.Position.AroundRandom2D(20));

                if (taxi.IsSafeExist())
                {
                    taxi.MarkAsNoLongerNeeded();
                    var ped = taxi.CreateRandomPedAsDriver();
                    if (ped.IsSafeExist())
                    {
                        ped.MarkAsNoLongerNeeded();
                    }
                }

                yield return null;
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