using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inferno.InfernoScripts
{
    internal class ParupunteConfigElement
    {
        public string StartMessage { get; set; }
        public string SubMessage { get; set; }
        public string FinishMessage { get; set; }

        public ParupunteConfigElement(string startMessage, string subMessage, string finishMessage)
        {
            StartMessage = startMessage;
            SubMessage = subMessage;
            FinishMessage = finishMessage;
        }

        public ParupunteConfigElement(string startMessage, string finishMessage)
        {
            StartMessage = startMessage;
            SubMessage = startMessage;
            FinishMessage = finishMessage;
        }

        public static ParupunteConfigElement Default = new ParupunteConfigElement("", "");
    }

    internal class ParupunteConfigDto
    {
        public string ParupunteName { get; set; }

        [DefaultValue("")]
        public string StartMessage { get; set; }

        [DefaultValue("")]
        public string SubMessage { get; set; }

        [DefaultValue("")]
        public string FinishMessage { get; set; }

        public ParupunteConfigDto(string parupunteName, string startMessage, string subMessage, string finishMessage)
        {
            ParupunteName = parupunteName;
            StartMessage = startMessage;
            SubMessage = subMessage;
            FinishMessage = finishMessage;
        }
    }
}
