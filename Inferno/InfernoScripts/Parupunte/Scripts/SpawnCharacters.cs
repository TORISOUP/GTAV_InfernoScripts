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


    internal class SpawnCharacters : ParupunteScript
    {
        private Model pedModel;
        private string name;
        private Random random;

        public SpawnCharacters(ParupunteCore core) : base(core)
        {
            pedModel = new Model(PedHash.LamarDavis);
            name = "ニガ～♪";
            random = new Random();
        }

        public override string Name
        {
            get { return name; }
        }

        public override void OnStart()
        {
            StartCoroutine(SpawnCharacter());
        }

        private IEnumerable<object> SpawnCharacter()
        {
            var player = core.PlayerPed;
            foreach (var s in WaitForSeconds(2))
            {
                var ped = GTA.World.CreatePed(pedModel, player.Position.AroundRandom2D(10));
                ped.MarkAsNoLongerNeeded();
                GiveWeaponTpPed(ped);
                yield return s;
            }
            ParupunteEnd();
        }

        /// <summary>
        /// 市民に武器をもたせる
        /// </summary>
        /// <param name="ped">市民</param>
        /// <returns>装備した武器</returns>
        private void GiveWeaponTpPed(Ped ped)
        {
            try
            {
                if (!ped.IsSafeExist()) return;

                //車に乗っているなら車用の武器を渡す
                var weapon = Enum.GetValues(typeof (WeaponHash))
                    .Cast<WeaponHash>()
                    .OrderBy(c => random.Next())
                    .FirstOrDefault();

                var weaponhash = (int) weapon;

                ped.SetDropWeaponWhenDead(false); //武器を落とさない
                ped.GiveWeapon(weaponhash, 1000); //指定武器所持
                ped.EquipWeapon(weaponhash); //武器装備
            }
            catch (Exception e)
            {

            }
        }
    }
}
