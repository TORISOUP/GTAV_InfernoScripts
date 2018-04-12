namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    internal class Owatashiki : ParupunteScript
    {
        public Owatashiki(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override ParupunteConfigElement DefaultElement { get; }
            = new ParupunteConfigElement("オワタ式の可能性", "");

        public override void OnSetUp()
        {
        }

        public override void OnStart()
        {
            var player = core.PlayerPed;
            player.Health = 1;
            player.Armor = 0;
            ParupunteEnd();
        }
    }
}
