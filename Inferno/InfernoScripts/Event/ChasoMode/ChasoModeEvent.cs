namespace Inferno.InfernoScripts.Event.ChasoMode
{
    public abstract class ChasoModeEvent : IEventMessage
    {
        public static readonly ChangeToDefaultEvent SetToDefault = new();
    }

    public class ChangeWeaponEvent : ChasoModeEvent
    {
        public ChangeWeaponEvent(Weapon weapon)
        {
            Weapon = weapon;
        }

        public Weapon Weapon { get; private set; }
    }

    public class ChangeToDefaultEvent : ChasoModeEvent
    {
    }
}