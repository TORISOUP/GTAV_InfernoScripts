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
            Random random = new Random();

            weather = Enum.GetValues(typeof(Weather))
                .Cast<Weather>()
                .OrderBy(x => random.Next())
                .FirstOrDefault();

            var weatherName = GetWeatherName(weather);
            name = "天候変更" + weatherName;
        }

        public override string Name
        {
            get { return name; }
        }
        
        public override void OnStart()
        {
            GTA.World.Weather = weather;
            ParupunteEnd();
        }

        private string GetWeatherName(Weather weather)
        {
            string weatherName = null;
            switch (weather)
            {
                case Weather.ExtraSunny:
                    weatherName = "快晴";
                    break;
                case Weather.Clear:
                    weatherName = "晴れ";
                    break;
                case Weather.Clouds:
                    weatherName = "くもり";
                    break;
                case Weather.Smog:
                    weatherName = "スモッグ";
                    break;
                case Weather.Foggy:
                    weatherName = "霧";
                    break;
                case Weather.Overcast:
                    weatherName = "くもり2";
                    break;
                case Weather.Raining:
                    weatherName = "雨";
                    break;
                case Weather.ThunderStorm:
                    weatherName = "嵐";
                    break;
                case Weather.Clearing:
                    weatherName = "天気雨";
                    break;
                case Weather.Neutral:
                    weatherName = "奇妙";
                    break;
                case Weather.Snowing:
                    weatherName = "雪";
                    break;
                case Weather.Blizzard:
                    weatherName = "吹雪";
                    break;
                case Weather.Snowlight:
                    weatherName = "雪明り";
                    break;
                case Weather.Christmas:
                    weatherName = "クリスマス";
                    break;
                default:
                    return string.Empty;
            }
            return weatherName.Insert(0, "：");
        }
    }
}
