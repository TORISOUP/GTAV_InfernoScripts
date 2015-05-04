using GTA;
using GTA.Native;

namespace Inferno
{
    public static class NativeFunctions
    {
        public static bool IsGamePadPressed(this Script script, GameKey gameKey)
        {
            return Function.Call<bool>(Hash.IS_CONTROL_PRESSED, new InputArgument[2] { 0, (int)gameKey });
        }
    }
}
