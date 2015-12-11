using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.ChaosMode;
using Inferno.Utilities;


namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    class ChangeWeather : ParupunteScript
    {
        private string name;
        private Weather weather;

        public ChangeWeather(ParupunteCore core) : base(core)
        {
        }

        public override string Name
        {
            get { return name; }
        }

        public override void OnSetUp()
        {
            Random random = new Random();

            weather = Enum.GetValues(typeof(Weather))
                .Cast<Weather>()
                .OrderBy(x => random.Next())
                .FirstOrDefault();

            var weatherName = GetWeatherName(weather);
            name = "天候変化" + "：" + weatherName;
        }

        public override void OnStart()
        {
            GTA.World.Weather = weather;
            ParupunteEnd();
        }

        private string GetWeatherName(Weather weather)
        {
            switch (weather)
            {
                case Weather.ExtraSunny:
                    return "快晴";
                case Weather.Clear:
                    return "晴れ";
                case Weather.Clouds:
                    return "くもり";
                case Weather.Smog:
                    return "スモッグ";
                case Weather.Foggy:
                    return "霧";
                case Weather.Overcast:
                    return "くもり2";
                  case Weather.Raining:
                    return "雨";
                case Weather.ThunderStorm:
                    return "嵐";
                case Weather.Clearing:
                    return "天気雨";
                case Weather.Neutral:
                    return "奇妙";
                case Weather.Snowing:
                    return "雪";
                case Weather.Blizzard:
                    return "吹雪";
                case Weather.Snowlight:
                    return "雪明り";
                case Weather.Christmas:
                    return "クリスマス";
                default:
                    return "わからん";
            }
        }
    }
}
