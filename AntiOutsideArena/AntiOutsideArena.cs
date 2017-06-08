using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rocket.Core.Plugins;
using Rocket.API;
using Logger = Rocket.Core.Logging.Logger;
using SDG.Unturned;
using UnityEngine;
using Steamworks;

namespace AntiOutsideArena
{
    public class AntiOutsideArena : RocketPlugin<Configuration>
    {
        private Dictionary<ArenaPlayer, DateTime> OutsidePlayers = new Dictionary<ArenaPlayer, DateTime>();

        void FixedUpdate()
        {
            if (LevelManager.arenaPlayers == null)
                return;

            List<ArenaPlayer> remove = new List<ArenaPlayer>();

            for(int i = 0; i < LevelManager.arenaPlayers.Count; i++)
            {
                if (!LevelManager.isPlayerInArena(LevelManager.arenaPlayers[i].steamPlayer.player) || LevelManager.arenaPlayers[i].hasDied)
                {
                    remove.Add(LevelManager.arenaPlayers[i]);
                    continue;
                }

                if (IsPlayerOutsideArena(LevelManager.arenaPlayers[i]))
                {
                    if (!OutsidePlayers.ContainsKey(LevelManager.arenaPlayers[i]))
                        OutsidePlayers.Add(LevelManager.arenaPlayers[i], DateTime.Now);

                    if((DateTime.Now - OutsidePlayers[LevelManager.arenaPlayers[i]]).TotalSeconds >= this.Configuration.Instance.SecondsUntilKill)
                    {
                        LevelManager.arenaPlayers[i].steamPlayer.player.life.askDamage(10, Vector3.up * 10f, EDeathCause.ARENA, ELimb.SPINE, CSteamID.Nil, out EPlayerKill ePlayerKill);
                        remove.Add(LevelManager.arenaPlayers[i]);
                    }
                }
                else
                {
                    if (OutsidePlayers.ContainsKey(LevelManager.arenaPlayers[i]))
                        remove.Add(LevelManager.arenaPlayers[i]);
                }
            }

            while(remove.Count > 0)
            {
                OutsidePlayers.Remove(remove[0]);
                remove.RemoveAt(0);
            }
        }

        public static bool IsPlayerOutsideArena(ArenaPlayer player)
        {
            float num = Mathf.Pow(player.steamPlayer.player.transform.position.x - LevelManager.arenaCenter.x, 2f) + Mathf.Pow(player.steamPlayer.player.transform.position.z - LevelManager.arenaCenter.z, 2f);

            return (num > LevelManager.arenaRadius);
        }
    }
}
