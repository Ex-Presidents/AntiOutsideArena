using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rocket.API;

namespace AntiOutsideArena
{
    public class Configuration : IRocketPluginConfiguration
    {
        public int SecondsUntilKill;

        public void LoadDefaults()
        {
            SecondsUntilKill = 30;
        }
    }
}
