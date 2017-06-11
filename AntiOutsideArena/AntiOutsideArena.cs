using System;
using System.Reflection;
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
        private static FieldInfo sqrRadius;

        protected override void Load()
        {
            sqrRadius = typeof(LevelManager).GetField("arenaSqrRadius", BindingFlags.NonPublic | BindingFlags.Static);

            Provider.onEnemyDisconnected += new Provider.EnemyDisconnected(OnPlayerExit);
        }

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

                    if((DateTime.Now - OutsidePlayers[LevelManager.arenaPlayers[i]]).TotalSeconds >= Configuration.Instance.SecondsUntilKill)
                    {
                        LevelManager.arenaPlayers[i].steamPlayer.player.life.askDamage(255, Vector3.up * 10f, EDeathCause.ARENA, ELimb.SPINE, CSteamID.Nil, out EPlayerKill ePlayerKill);
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

        private void OnPlayerExit(SteamPlayer player)
        {
            ArenaPlayer ply = OutsidePlayers.Keys.FirstOrDefault(a => a.steamPlayer == player);

            if (ply == null)
                return;
            OutsidePlayers.Remove(ply);
        }

        public static bool IsPlayerOutsideArena(ArenaPlayer player)
        {
            if (player == null)
                return false;
            if (player.steamPlayer == null)
                return false;
            if (player.steamPlayer.player == null)
                return false;

            float num = Mathf.Pow(player.steamPlayer.player.transform.position.x - LevelManager.arenaCenter.x, 2f) + Mathf.Pow(player.steamPlayer.player.transform.position.z - LevelManager.arenaCenter.z, 2f);
            float arenaSqrRadius = (float)sqrRadius.GetValue(null);

            return (num > arenaSqrRadius);
        }
    }
}
