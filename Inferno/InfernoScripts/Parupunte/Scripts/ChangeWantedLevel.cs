using GTA;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    internal class ChangeWantedLevel : ParupunteScript
    {
        private string name;
        private int wantedLevelThreshold = 1;
        private int wantedLevel = Game.Player.WantedLevel;

        public ChangeWantedLevel(ParupunteCore core) : base(core)
        {
        }

        public override void OnSetUp()
        {
            name = wantedLevel >= wantedLevelThreshold ? "無罪放免" : "日頃の行いが悪い";
        }

        public override string Name
        {
            get { return name; }
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
