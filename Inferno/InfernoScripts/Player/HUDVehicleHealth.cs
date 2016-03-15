using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Native;
using UniRx;

namespace Inferno
{
    /// <summary>
    /// 乗り物の体力表示
    /// </summary>
    class HUDVehicleHealth : InfernoScript
    {

        private UIContainer _mContainer = null;
        private int _screenHeight;
        private int _screenWidth;

        protected override void Setup()
        {
            var screenResolution = NativeFunctions.GetScreenResolution();
            _screenHeight = (int)screenResolution.Y;
            _screenWidth = (int)screenResolution.X;
            _mContainer = new UIContainer(new Point(0, 0), new Size(_screenWidth, _screenHeight));

            OnDrawingTickAsObservable
                .Where(_ => this.GetPlayerVehicle().IsSafeExist() && PlayerPed.IsAlive)
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
            var vheiclePetrolTankHealth = vheicle.PetrolTankHealth;

            var petrolTankHealthColor = Color.FromArgb(200, 200, 0, 128);

            if (vheiclePetrolTankHealth < 0)
            {
                petrolTankHealthColor = Color.FromArgb(200, 255, 200, 0);
                vheiclePetrolTankHealth += 1000.0f;
            }

            DrawHealthBar(vheiclePetrolTankHealth, 1000.0f, new Point(5, 560), petrolTankHealthColor);
            DrawHealthBar(bodyHealth, 1000.0f, new Point(5, 570), Color.FromArgb(200, 0, 128, 200));
            DrawHealthBar(engineHealth, 1000.0f, new Point(5, 580), Color.FromArgb(200, 128, 200, 0));
        }

        /// <summary>
        /// 体力ゲージを描画する
        /// </summary>
        /// <param name="health"></param>
        /// <param name="maxHealth"></param>
        /// <param name="vhicleHealthType"></param>
        private void DrawHealthBar(float health, float maxHealth, Point pos, Color foreGroundColor)
        {
            var width = 180;
            var height = 5;
            var margin = 2;

            var barLength = 0;
            var barPosition = default(Point);
            var barSize = default(Size);
            var backGroundColor = Color.FromArgb(128, 0, 0, 0);

            var t = Function.Call<float>(Hash._GET_SCREEN_ASPECT_RATIO, true);
            width = (int)(width + (width*(1.75f - t)));

            if (health > maxHealth)
            {
                maxHealth = health;
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
