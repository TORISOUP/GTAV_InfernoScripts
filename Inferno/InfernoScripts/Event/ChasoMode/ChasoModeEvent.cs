using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA.Native;

namespace Inferno.InfernoScripts.Event.ChasoMode
{
    public abstract class ChasoModeEvent : IEventMessage
    {
        public readonly static ChangeToDefaultEvent SetToDefault = new ChangeToDefaultEvent();
    }

    public class ChangeWeaponEvent : ChasoModeEvent
    {
        public Weapon Weapon { get; private set; }

        public ChangeWeaponEvent(Weapon weapon)
        {
            Weapon = weapon;
        }
    }

    public class ChangeToDefaultEvent : ChasoModeEvent
    {
        
    }
}
