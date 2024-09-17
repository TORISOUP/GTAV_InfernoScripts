namespace Inferno.InfernoScripts.Event.Isono
{
    public struct IsonoMessage : IEventMessage
    {
        public string Command { get; private set; }

        public IsonoMessage(string command)
        {
            Command = command;
        }
    }
}