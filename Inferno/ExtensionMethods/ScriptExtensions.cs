
using GTA;

namespace Inferno
{
    public static class ScriptExtensions
    {
        public static Vehicle GetPlayerVehicle(this Script script)
        {
            var player = Game.Player.Character;
            return player.IsInVehicle() ? player.CurrentVehicle : null;
        }

    }
}
