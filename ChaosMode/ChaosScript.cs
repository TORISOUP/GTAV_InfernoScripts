using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Native;

namespace ChaosMode
{
    public class ChaosScript : Script
    {

        public ChaosScript()
        {
            
            Tick += OnTick;
            Interval = 5000;
        }

        private void OnTick(object sender, EventArgs eventArgs)
        {

        }
    }
}
