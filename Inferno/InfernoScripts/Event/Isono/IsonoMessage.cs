using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inferno.InfernoScripts.Event.Isono
{
    struct IsonoMessage : IEventMessage
    {
        public string Command { get; private set; }

        public IsonoMessage(string command)
        {
            Command = command;
        }
    }
}
