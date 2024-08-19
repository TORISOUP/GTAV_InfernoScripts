using GTA;
using System.Linq;
using System.Reactive.Linq;
using System;
using System.Reactive;
using System.Reactive.Subjects;


namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("今じゃあケツをふく紙にもなりゃしねってのによぉ")]
    internal class AddPlayerMoney : ParupunteScript
    {
        public AddPlayerMoney(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

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
