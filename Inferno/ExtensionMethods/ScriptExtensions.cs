
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

        public static Ped GetPlayer(this Script script)
        {
            return Game.Player.Character;
        }

        public static bool IsSafeExist(this Entity entity)
        {
            return entity != null && Entity.Exists(entity);
        }
    }
}
