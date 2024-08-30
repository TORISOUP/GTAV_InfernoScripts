using GTA;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("今じゃあケツをふく紙にもなりゃしねってのによぉ")]
    internal class AddPlayerMoney : ParupunteScript
    {
        public AddPlayerMoney(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }
        
        public override void OnStart()
        {
            Game.Player.Money += 20000;
            ParupunteEnd();
        }
    }
}