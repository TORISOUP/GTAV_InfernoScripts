namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("オワタ式の可能性")]
    internal class Owatashiki : ParupunteScript
    {
        public Owatashiki(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnSetUp()
        {
        }

        public override void OnStart()
        {
            var player = core.PlayerPed;
            player.Health = 10;
            player.Armor = 0;
            ParupunteEnd();
        }
    }
}