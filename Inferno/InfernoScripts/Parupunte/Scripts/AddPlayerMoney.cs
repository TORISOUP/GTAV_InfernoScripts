using GTA;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    internal class AddPlayerMoney : ParupunteScript
    {
        public AddPlayerMoney(ParupunteCore core) : base(core)
        {
        }

        public override string Name => "今じゃあケツをふく紙にもなりゃしねってのによぉ!";

        public override void OnSetUp()
        {
        }

        public override void OnStart()
        {
            Game.Player.Money += 20000;
            ParupunteEnd();
        }
    }
}
