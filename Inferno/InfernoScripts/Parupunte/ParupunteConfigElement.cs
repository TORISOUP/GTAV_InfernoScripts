using System.ComponentModel;

namespace Inferno.InfernoScripts
{
    internal class ParupunteConfigElement
    {
        public static ParupunteConfigElement Default = new("", "");

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

        public string StartMessage { get; set; }
        public string SubMessage { get; set; }
        public string FinishMessage { get; set; }

        public ParupunteConfigDto ToDto()
        {
            return new ParupunteConfigDto(StartMessage, SubMessage, FinishMessage);
        }
    }

    internal class ParupunteConfigDto
    {
        public ParupunteConfigDto(string startMessage, string subMessage, string finishMessage)
        {
            StartMessage = startMessage;
            SubMessage = subMessage;
            FinishMessage = finishMessage;
        }

        [DefaultValue("")] public string StartMessage { get; set; }

        [DefaultValue("")] public string SubMessage { get; set; }

        [DefaultValue("")] public string FinishMessage { get; set; }

        public ParupunteConfigElement ToDomain()
        {
            return new ParupunteConfigElement(StartMessage, SubMessage, FinishMessage);
        }
    }
}