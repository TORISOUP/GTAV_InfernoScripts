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

        public ParupunteConfigDto ToDto()
        {
            return new ParupunteConfigDto(StartMessage, SubMessage, FinishMessage);
        }
    }

    internal class ParupunteConfigDto
    {

        [DefaultValue("")]
        public string StartMessage { get; set; }

        [DefaultValue("")]
        public string SubMessage { get; set; }

        [DefaultValue("")]
        public string FinishMessage { get; set; }

        public ParupunteConfigDto(string startMessage, string subMessage, string finishMessage)
        {
            StartMessage = startMessage;
            SubMessage = subMessage;
            FinishMessage = finishMessage;
        }

        public ParupunteConfigElement ToDomain()
        {
            return new ParupunteConfigElement(StartMessage, SubMessage, FinishMessage);
        }
    }
}
