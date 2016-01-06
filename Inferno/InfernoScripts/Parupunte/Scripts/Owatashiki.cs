namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    internal class Owatashiki : ParupunteScript
    {
        public Owatashiki(ParupunteCore core) : base(core)
        {
        }

        public override string Name => "オワタ式の可能性";

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
