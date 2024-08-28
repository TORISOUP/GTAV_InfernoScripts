using GTA;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteIsono("けいさつ")]
    internal class ChangeWantedLevel : ParupunteScript
    {
        private readonly int wantedLevelThreshold = 1;
        private int wantedLevel;

        public ChangeWantedLevel(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }


        public override void OnSetUp()
        {
            //確実にメインスレッドで取得するためにここで取得
            wantedLevel = Game.Player.WantedLevel;
        }

        public override void OnSetNames()
        {
            Name = wantedLevel >= wantedLevelThreshold ? "無罪放免" : "日頃の行いが悪い";
        }

        public override void OnStart()
        {
            if (wantedLevel >= wantedLevelThreshold)
            {
                Game.Player.WantedLevel = 0;
            }
            else
            {
                IncreasePlayerWantedLevel();
            }

            ParupunteEnd();
        }

        private void IncreasePlayerWantedLevel()
        {
            var playerChar = Game.Player;
            var MaxWantedLevel = Game.MaxWantedLevel;

            if (MaxWantedLevel < playerChar.WantedLevel + 4)
            {
                playerChar.WantedLevel = MaxWantedLevel;
            }
            else
            {
                playerChar.WantedLevel = playerChar.WantedLevel + 4;
            }
        }
    }
}