using System;
using System.Collections.Generic;

namespace Inferno.InfernoScripts.Conf
{

    [Serializable]
    public class InfernoCommandConfig
    {
        public Dictionary<string, string> CommandList = new();
    }
}