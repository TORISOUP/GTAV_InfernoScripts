using System;
using GTA.Native;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteIsono("いまなんじ")]
    internal class SetDateTime : ParupunteScript
    {
        private int hour;
        private string name;

        public SetDateTime(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }


        public override void OnSetNames()
        {
            Name = name;
        }

        public override void OnSetUp()
        {
            var random = new Random();
            hour = random.Next(0, 23);
            name = hour + "時かな";
        }

        public override void OnStart()
        {
            var dayTime = GTA.World.CurrentDayTime;
            Function.Call(Hash.SET_CLOCK_TIME, hour, dayTime.Minutes, dayTime.Seconds);
            ParupunteEnd();
        }
    }
}