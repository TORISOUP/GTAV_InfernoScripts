using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using UniRx;

namespace Inferno
{
    /// <summary>
    /// 乗り物の体力表示
    /// </summary>
    class HUDVehicleHealth : InfernoScript
    {

        private UIContainer _mContainer = null;

        /// <summary>
        /// 描画するゲージの種類
        /// </summary>
        private enum DrawStatus
        {
            VheicleHelth,
            BodyHealth,
            EngineHealth
        };

        protected override void Setup()
        {
            _mContainer = new UIContainer(new Point(0, 0), new Size(500, 20));

            OnDrawingTickAsObservable
                .Where(_ => this.GetPlayerVehicle().IsSafeExist())
                .Subscribe(_ =>
                {
                    _mContainer.Items.Clear();
                    GetVehicleHealth();
                    _mContainer.Draw();
                });
        }

        /// <summary>
        /// 乗り物の体力を取得する
        /// </summary>
        private void GetVehicleHealth()
        {
            var vheicle = this.GetPlayerVehicle();
            if(!vheicle.IsSafeExist()) return;

            var bodyHealth = vheicle.BodyHealth;
            var engineHealth = vheicle.EngineHealth;
            var vheicleHealth = vheicle.Health;
            var vheicleMaxHealth = vheicle.MaxHealth;
            if (vheicleHealth > vheicleMaxHealth)
            {
                vheicleMaxHealth = vheicleHealth;
            }
            DrawHealthBar(vheicleHealth, vheicleMaxHealth, DrawStatus.VheicleHelth);
            DrawHealthBar(bodyHealth, 1000.0f, DrawStatus.BodyHealth);
            DrawHealthBar(engineHealth, 1000.0f, DrawStatus.EngineHealth);
        }

        /// <summary>
        /// 体力ゲージを描画する
        /// </summary>
        /// <param name="health"></param>
        /// <param name="maxHealth"></param>
        /// <param name="drawStatus"></param>
        private void DrawHealthBar(float health, float maxHealth, DrawStatus drawStatus)
        {
            var pos = default(Point);
            var width = 180;
            var height = 5;
            var margin = 2;

            var barLength = 0;
            var barPosition = default(Point);
            var barSize = default(Size);
            var backGroundColor = Color.FromArgb(128, 0, 0, 0);
            var foreGroundColor = default(Color);

            switch (drawStatus)
            {
                case DrawStatus.VheicleHelth:
                    pos = new Point(5, 560);
                    foreGroundColor = Color.FromArgb(200, 255, 128, 0);
                    break;
                case DrawStatus.BodyHealth:
                    pos = new Point(5, 570);
                    foreGroundColor = Color.FromArgb(200, 0, 255, 200);
                    break;
                case DrawStatus.EngineHealth:
                    pos = new Point(5, 580);
                    foreGroundColor = Color.FromArgb(200, 255, 0, 128);
                    break;
            }

            barLength = (int)(width * (health / maxHealth));
            if (barLength < 0) barLength = 0;
            barPosition = new Point(pos.X, pos.Y);
            barSize = new Size(barLength, height);

            _mContainer.Items.Add(new UIRectangle(new Point(pos.X - margin, pos.Y - margin),
                new Size(width + margin * 2, height + margin * 2), backGroundColor));
            _mContainer.Items.Add(new UIRectangle(barPosition, barSize, foreGroundColor));
        }
    }
}
