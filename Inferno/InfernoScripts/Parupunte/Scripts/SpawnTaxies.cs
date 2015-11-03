using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Native;
using Inferno.ChaosMode;
using Inferno.ChaosMode.WeaponProvider;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    class SpawnTaxies : ParupunteScript
    {
        private string name, honorific;

        public SpawnTaxies(ParupunteCore core) : base(core)
        {
            honorific = core.PlayerPed.Gender == Gender.Male ? "くん！" : "ちゃん！";
            name = "仲間が増えるよ！やったね" + GetPlayerCharacterName() + honorific;
        }

        public override string Name
        {
            get { return name; }
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
                var taxi = GTA.World.CreateVehicle(GTA.Native.VehicleHash.Taxi, player.Position.AroundRandom2D(10));

                if (taxi.IsSafeExist())
                {
                    taxi.MarkAsNoLongerNeeded();
                    var ped = taxi.CreateRandomPedAsDriver();
                    if (ped.IsSafeExist()) { ped.MarkAsNoLongerNeeded(); }
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
                    return Game.GetGXTEntry("BLIP_TREV");
                case PedHash.Michael:
                    return Game.GetGXTEntry("BLIP_MICHAEL");
                case PedHash.Franklin:
                    return Game.GetGXTEntry("BLIP_FRANKLIN");
                default:
                    return hash.ToString();
            }
        }
    }
}
