using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.ChaosMode;
using Inferno.Utilities;


namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    class SetDateTime : ParupunteScript
    {
        private int hour;
        private string name;

        public SetDateTime(ParupunteCore core) : base(core)
        {

        }

        public override string Name
        {
            get { return name; }
        }

        public override void OnSetUp()
        {
            Random random = new Random();
            hour = random.Next(0, 23);
            name = hour.ToString() + "時かな";
        }

        public override void OnStart()
        {
            var dayTime = GTA.World.CurrentDayTime;
            Function.Call(Hash.SET_CLOCK_TIME, hour, dayTime.Minutes, dayTime.Seconds);
            ParupunteEnd();
        }

    }
}
