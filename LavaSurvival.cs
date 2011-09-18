/*
	Copyright 2011 MCForge
		
	Dual-licensed under the	Educational Community License, Version 2.0 and
	the GNU General Public License, Version 3 (the "Licenses"); you may
	not use this file except in compliance with the Licenses. You may
	obtain a copy of the Licenses at
	
	http://www.opensource.org/licenses/ecl2.php
	http://www.gnu.org/licenses/gpl-3.0.html
	
	Unless required by applicable law or agreed to in writing,
	software distributed under the Licenses are distributed on an "AS IS"
	BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
	or implied. See the Licenses for the specific language governing
	permissions and limitations under the Licenses.
*/
using System;
using System.IO;
using System.Collections.Generic;
using System.Timers;

namespace MCForge
{
    public class LavaSurvival
    {
        // Private variables
        private string propsPath = "properties/lavasurvival/";
        private List<string> maps, voted;
        private Dictionary<string, int> votes;
        private Random rand = new Random();
        private Timer announceTimer, voteTimer, transferTimer;
        private DateTime startTime;

        // Public variables
        public bool active = false, roundActive = false, roundroundActive = false, flooded = false, voteActive = false, sendingPlayers = false;
        public Level map;
        public MapSettings mapSettings;
        public MapData mapData;

        // Settings
        public bool startOnStartup, sendAfkMain;
        public byte voteCount;
        public double voteTime;
        public LevelPermission setupRank;

        // Constructors
        public LavaSurvival()
        {
            maps = new List<string>();
            voted = new List<string>();
            votes = new Dictionary<string, int>();
            announceTimer = new Timer(60000);
            announceTimer.AutoReset = true;
            announceTimer.Elapsed += new ElapsedEventHandler(delegate
            {
                AnnounceTimeLeft(true, false);
            });

            startOnStartup = false;
            sendAfkMain = true;
            voteCount = 2;
            voteTime = 2;
            setupRank = LevelPermission.Operator;
            LoadSettings();
        }

        // Private methods
        private decimal NumberClamp(decimal value, decimal low, decimal high)
        {
            return Math.Max(Math.Min(value, high), low);
        }

        private void LevelCommand(string name, string msg = "")
        {
            Command cmd = Command.all.Find(name.Trim());
            if (cmd != null && map != null)
                try { cmd.Use(null, map.name + " " + msg.Trim()); }
                catch (Exception e) { Server.ErrorLog(e); }
        }

        // Public methods
        public byte Start(string mapName = "")
        {
            if (active) return 1; // Already started
            if (maps.Count < 3) return 2; // Not enough maps
            if (!String.IsNullOrEmpty(mapName) && !HasMap(mapName)) return 3; // Map doesn't exist

            active = true;
            try { LoadMap(String.IsNullOrEmpty(mapName) ? maps[rand.Next(maps.Count)] : mapName); }
            catch (Exception e) { Server.ErrorLog(e); }
            return 0;
        }
        public byte Stop()
        {
            if (!active) return 1; // Not started

            active = false;
            roundActive = false;
            voteActive = false;
            if (announceTimer.Enabled) announceTimer.Stop();
            try { mapData.Dispose(); }
            catch { }
            try { voteTimer.Dispose(); }
            catch { }
            try { transferTimer.Dispose(); }
            catch { }
            map.Unload(true, false);
            return 0;
        }

        public void StartRound()
        {
            if (roundActive) return;

            try
            {
                mapData.roundTimer.Elapsed += new ElapsedEventHandler(delegate { EndRound(); });
                mapData.floodTimer.Elapsed += new ElapsedEventHandler(delegate { DoFlood(); });
                mapData.roundTimer.Start();
                mapData.floodTimer.Start();
                announceTimer.Start();
                startTime = DateTime.Now;
                roundActive = true;
                try
                {
                    foreach (Player p in Player.players)
                    {
                        Server.lava.AnnounceRoundInfo(p);
                        Server.lava.AnnounceTimeLeft(!Server.lava.flooded, true, p);
                    }
                }
                catch { }
            }
            catch (Exception e) { Server.ErrorLog(e); }
        }

        public void EndRound()
        {
            if (!roundActive) return;

            roundActive = false;
            roundroundActive = false;
            flooded = false;
            try
            {
                try { mapData.Dispose(); }
                catch { }
                map.setPhysics(5);
                map.ChatLevel("The round has ended!");
                StartVote();
            }
            catch (Exception e) { Server.ErrorLog(e); }
        }

        public void DoFlood()
        {
            if (!active || !roundActive || flooded || map == null) return;
            flooded = true;

            roundroundActive = true;

            try
            {
                announceTimer.Stop();
                map.ChatLevel("&4Look out, the round has started!");
                    ushort x, y, z; int currentBlock = 0;
                    List<Pos> stored = new List<Pos>(); Pos pos;
                    foreach (byte b in map.blocks)
                    {
                        if (b == Block.lava_timer)
                        {
                            map.IntToPos(currentBlock, out x, out y, out z);
                            pos.x = x; pos.y = y; pos.z = z;
                            stored.Add(pos);
                        }
                        currentBlock++;
                    }
                    foreach (Pos Pos in stored)
                    {
                        byte b2;
                        b2 = mapData.block;
                        /*if(mapData.water)
                            b2 = Block.activedeathwater;
                        else if (mapData.fast)
                            b2 = Block.lava_fast;
                        else
                            b2 = Block.Byte("active_lava");*/
                        //map.Blockchange(Pos.x, Pos.y, Pos.z, Block.bookcase);
                        ushort yy = Pos.y;
                        Player.GlobalBlockchange(map, Pos.x, yy, Pos.z, b2);

                        if (yy >= map.depth) yy = (ushort)(map.depth - 1);

                        map.SetTile(Pos.x, yy, Pos.z, b2);               //Updates server level blocks

                        map.AddCheck(map.PosToInt(Pos.x, yy, Pos.z));
                    }
                    //map.Blockchange((ushort)mapSettings.blockFlood.x, (ushort)mapSettings.blockFlood.y, (ushort)mapSettings.blockFlood.z, mapData.block, true);
            }
            catch (Exception e) { Server.ErrorLog(e); }
        }

        public void DoFloodLayer()
        {
            map.ChatLevel("&4Layer " + mapData.currentLayer + " flooding...");
            map.Blockchange((ushort)mapSettings.blockLayer.x, (ushort)(mapSettings.blockLayer.y + ((mapSettings.layerHeight * mapData.currentLayer) - 1)), (ushort)mapSettings.blockLayer.z, mapData.block, true);
            mapData.currentLayer++;
        }

        public void AnnounceTimeLeft(bool flood, bool round, Player p = null)
        {
            if (!active || !roundActive || startTime == null || map == null) return;

            if (flood)
            {
                double floodMinutes = Math.Ceiling((startTime.AddMinutes(mapSettings.floodTime) - DateTime.Now).TotalMinutes);
                if(floodMinutes != 0 && !roundroundActive)
                {
                if (p == null) map.ChatLevel("&3" + floodMinutes + " minute" + (floodMinutes == 1 ? "" : "s") + Server.DefaultColor + " until the flood.");
                else Player.SendMessage(p, "&3" + floodMinutes + " minute" + (floodMinutes == 1 ? "" : "s") + Server.DefaultColor + " until the flood.");
                }
            }
            if (round)
            {
                double roundMinutes = Math.Ceiling((startTime.AddMinutes(mapSettings.roundTime) - DateTime.Now).TotalMinutes);
                if (roundMinutes != 0)
                {
                    if (p == null) map.ChatLevel("&3" + roundMinutes + " minute" + (roundMinutes == 1 ? "" : "s") + Server.DefaultColor + " until the round ends.");
                    else Player.SendMessage(p, "&3" + roundMinutes + " minute" + (roundMinutes == 1 ? "" : "s") + Server.DefaultColor + " until the round ends.");
                }
            }
        }

        public void AnnounceRoundInfo(Player p = null)
        {
            if (p == null)
            {
                if (mapData.water)
                {
                    map.ChatLevel("The map will be flooded with &9water " + Server.DefaultColor + "this round!");
                    map.motd = "[Caznowl] Water Survival | Current Map: " + map.name;
                }
                if (mapData.fast) map.ChatLevel("The lava will be &cfast " + Server.DefaultColor + "this round!");
                if (mapData.destroy) map.ChatLevel("The " + (mapData.water ? "water" : "lava") + " will &cdestroy plants " + (mapData.water ? "" : "and flammable blocks ") + Server.DefaultColor + "this round!");
            }
            else
            {
                if (mapData.water)
                {
                    Player.SendMessage(p, "The map will be flooded with &9water " + Server.DefaultColor + "this round!");
                    map.motd = "[Caznowl] Water Survival | Current Map: " + map.name;
                }
                if (mapData.fast) Player.SendMessage(p, "The lava will be &cfast " + Server.DefaultColor + "this round!");
                if (mapData.destroy) Player.SendMessage(p, "The " + (mapData.water ? "water" : "lava") + " will &cdestroy plants " + (mapData.water ? "" : "and flammable blocks ") + Server.DefaultColor + "this round!");
            }
        }

        public void LoadMap(string name)
        {
            if (String.IsNullOrEmpty(name) || !HasMap(name)) return;

            name = name.ToLower();
            Level oldMap = null;
            if (active && map != null) oldMap = map;
            Command.all.Find("load").Use(null, name);
            map = Level.Find(name);

            if (map != null)
            {
                mapSettings = LoadMapSettings(name);
                mapData = GenerateMapData(mapSettings);

                map.setPhysics(mapData.destroy ? 2 : 1);
                map.motd = "[Caznowl] Lava Survival | Current Map: " + map.name;
                map.overload = 1000000;
                map.unload = false;
                map.loadOnGoto = false;
                Level.SaveSettings(map);
            }
            
            if (active && map != null)
            {
                sendingPlayers = true;
                try
                {
                    Player.players.ForEach(delegate(Player pl)
                    {
                        if (pl.level == oldMap)
                        {
                            if (sendAfkMain && Server.afkset.Contains(pl.name)) Command.all.Find("main").Use(pl, "");
                            else Command.all.Find("goto").Use(pl, map.name);
                        }
                    });
                    Command.all.Find("clearblockchanges").Use(null, oldMap.name.ToLower());
                    oldMap.Unload(true, false);
                }
                catch { }
                sendingPlayers = false;

                StartRound();
            }
        }

        public void StartVote()
        {
            if (maps.Count < 3) return;

            byte i = 0;
            string opt, str = "";
            while (i < Math.Min(voteCount, maps.Count - 1))
            {
                opt = maps[rand.Next(maps.Count)];
                if (!votes.ContainsKey(opt) && opt != map.name.ToLower() && opt != "")
                {
                    votes.Add(opt, 0);
                    str += Server.DefaultColor + ", &5" + opt;
                    i++;
                }
            }

            string playersString = "";
            foreach (Player player in Player.players)
            {

                Random random2 = new Random();
                int randomInt = 1;
                if (player.NoClipcount == 0)
                {
                    if (player.lives > 0)
                    {
                        if (!player.ironmanFailed && player.ironmanActivated)
                            randomInt = random2.Next(10, 15);
                        else if (player.ironmanFailed && !player.ironmanActivated)
                            randomInt = 0;
                        else
                            randomInt = random2.Next(1, 5);
                        Player.SendMessage(player, c.gold + "You gained " + randomInt + " " + Server.moneys);
                        player.money = player.money + randomInt;

                        if (!player.ironmanFailed && player.ironmanActivated)
                            playersString += c.lime + "[IRONMAN] " + player.group.color + player.name + c.white + ", ";
                        else if (player.ironmanFailed && !player.ironmanActivated)
                            playersString += c.lime + "[IRONMAN FAILED] " + player.group.color + player.name + c.white + ", ";
                        else
                            playersString += player.group.color + player.name + c.white + ", ";
                    }
                    else
                    {
                        Player.SendMessage(player, "You didn't survive! Good luck next time!");
                        player.lives++;
                    }
                }
                else
                {
                    Player.SendMessage(player, "Next time don't hide in a block, or hide at spawn!");

                    if (!player.ironmanFailed && player.ironmanActivated)
                        playersString += c.lime + "[IRONMAN] " + player.group.color + player.name + c.white + ", ";
                    else if (player.ironmanFailed && !player.ironmanActivated)
                        playersString += c.lime + "[IRONMAN FAILED] " + player.group.color + player.name + c.white + ", ";
                    else
                        playersString += player.group.color + player.name + c.white + ", ";
                }

                if (player.lives < 20)
                    player.lives++;
                player.ironmanActivated = false;
                player.ironmanFailed = false;
            }
            if (playersString == "")
            {
                map.ChatLevel(c.lime + "No-one survived this round. Better luck next time!");
            }
            else
            {
                map.ChatLevel(c.lime + "Congratulations to our survivors!");
            }
            map.ChatLevel(playersString);
            map.ChatLevel(c.lime + "Vote for the next map! The vote ends in " + voteTime + " minute" + (voteTime == 1 ? "" : "s") +".");
            map.ChatLevel("Choices: " + str.Remove(0, 4));

            voteTimer = new Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
            voteTimer.AutoReset = false;
            voteTimer.Elapsed += new ElapsedEventHandler(delegate
            {
                try
                {
                    EndVote();
                    voteTimer.Dispose();
                }
                catch (Exception e) { Server.ErrorLog(e); }
            });
            voteTimer.Start();
            voteActive = true;
        }

        public void EndVote()
        {
            voteActive = false;
            KeyValuePair<string, int> most = new KeyValuePair<string, int>("", -1);
            foreach (KeyValuePair<string, int> kvp in votes)
            {
                if (kvp.Value > most.Value) most = kvp;
                map.ChatLevelOps("&5" + kvp.Key + "&f: &a" + kvp.Value);
            }
            votes.Clear();
            voted.Clear();

            map.ChatLevel("The vote has ended! &5" + most.Key + Server.DefaultColor + " won with &a" + most.Value + Server.DefaultColor + " vote" + (most.Value == 1 ? "" : "s") + ".");
            map.ChatLevel("You will be transferred in 5 seconds...");
            transferTimer = new Timer(5000);
            transferTimer.AutoReset = false;
            transferTimer.Elapsed += new ElapsedEventHandler(delegate
            {
                try
                {
                    LoadMap(most.Key);
                    transferTimer.Dispose();
                }
                catch (Exception e) { Server.ErrorLog(e); }
            });
            transferTimer.Start();
        }

        public bool AddVote(Player p, string vote)
        {
            if (!voteActive || voted.Contains(p.name) || !votes.ContainsKey(vote)) return false;
            int temp = votes[vote] + 1;
            votes.Remove(vote);
            votes.Add(vote, temp);
            voted.Add(p.name);
            return true;
        }

        public bool HasVote(string vote)
        {
            return voteActive && votes.ContainsKey(vote);
        }

        public bool RoundActive()
        {
            return roundroundActive;
        }

        public bool HasPlayer(Player p)
        {
            return p.level == map;
        }

        public MapData GenerateMapData(MapSettings settings)
        {
            MapData data = new MapData(settings);
            data.killer = true;
            data.destroy = rand.Next(1, 101) <= settings.destroy;
            data.water = rand.Next(1, 101) <= settings.water;
            data.layer = false;
            data.fast = rand.Next(1, 101) <= settings.fast && !data.killer && !data.water;
            data.block = data.water ? (data.killer ? Block.activedeathwater : Block.water) : (data.fast ? Block.lava_fast : (data.killer ? Block.activedeathlava : Block.lava));
            return data;
        }

        public void LoadSettings()
        {
            if (!File.Exists("properties/lavasurvival.properties"))
            {
                SaveSettings();
                return;
            }

            foreach (string line in File.ReadAllLines("properties/lavasurvival.properties"))
            {
                try
                {
                    if (line[0] != '#')
                    {
                        string value = line.Substring(line.IndexOf(" = ") + 3);
                        switch (line.Substring(0, line.IndexOf(" = ")).ToLower())
                        {
                            case "start-on-startup":
                                startOnStartup = bool.Parse(value);
                                break;
                            case "send-afk-to-main":
                                sendAfkMain = bool.Parse(value);
                                break;
                            case "vote-count":
                                voteCount = (byte)NumberClamp(decimal.Parse(value), 2, 10);
                                break;
                            case "vote-time":
                                voteTime = double.Parse(value);
                                break;
                            case "setup-rank":
                                setupRank = Level.PermissionFromName(value.ToLower());
                                break;
                            case "maps":
                                foreach (string mapname in value.Split(','))
                                    if(!maps.Contains(mapname)) maps.Add(mapname);
                                break;
                        }
                    }
                }
                catch (Exception e) { Server.ErrorLog(e); }
            }
        }
        public void SaveSettings()
        {
            File.Create("properties/lavasurvival.properties").Dispose();
            using (StreamWriter SW = File.CreateText("properties/lavasurvival.properties"))
            {
                SW.WriteLine("#Lava Survival main properties");
                SW.WriteLine("start-on-startup = " + startOnStartup.ToString().ToLower());
                SW.WriteLine("send-afk-to-main = " + sendAfkMain.ToString().ToLower());
                SW.WriteLine("vote-count = " + voteCount.ToString());
                SW.WriteLine("vote-time = " + voteTime.ToString());
                SW.WriteLine("setup-rank = " + Level.PermissionToName(setupRank).ToLower());
                SW.WriteLine("maps = " + maps.Concatenate(","));
            }
        }

        public MapSettings LoadMapSettings(string name)
        {
            MapSettings settings = new MapSettings(name);
            if (!Directory.Exists(propsPath)) Directory.CreateDirectory(propsPath);
            if (!File.Exists(propsPath + name + ".properties"))
            {
                SaveMapSettings(settings);
                return settings;
            }

            foreach (string line in File.ReadAllLines(propsPath + name + ".properties"))
            {
                try
                {
                    if (line[0] != '#')
                    {
                        string value = line.Substring(line.IndexOf(" = ") + 3);
                        switch (line.Substring(0, line.IndexOf(" = ")).ToLower())
                        {
                            case "fast-chance":
                                settings.fast = (byte)NumberClamp(decimal.Parse(value), 0, 100);
                                break;
                            case "killer-chance":
                                settings.killer = (byte)NumberClamp(decimal.Parse(value), 0, 100);
                                break;
                            case "destroy-chance":
                                settings.destroy = (byte)NumberClamp(decimal.Parse(value), 0, 100);
                                break;
                            case "water-chance":
                                settings.water = (byte)NumberClamp(decimal.Parse(value), 0, 100);
                                break;
                            case "layer-chance":
                                settings.layer = (byte)NumberClamp(decimal.Parse(value), 0, 100);
                                break;
                            case "layer-height":
                                settings.layerHeight = int.Parse(value);
                                break;
                            case "layer-count":
                                settings.layerCount = int.Parse(value);
                                break;
                            case "layer-interval":
                                settings.layerInterval = double.Parse(value);
                                break;
                            case "round-time":
                                settings.roundTime = double.Parse(value);
                                break;
                            case "flood-time":
                                settings.floodTime = double.Parse(value);
                                break;
                            case "block-flood":
                                settings.blockFlood = new Pos(ushort.Parse(value.Split(',')[0]), ushort.Parse(value.Split(',')[1]), ushort.Parse(value.Split(',')[2]));
                                break;
                            case "block-layer":
                                settings.blockLayer = new Pos(ushort.Parse(value.Split(',')[0]), ushort.Parse(value.Split(',')[1]), ushort.Parse(value.Split(',')[2]));
                                break;
                        }
                    }
                }
                catch (Exception e) { Server.ErrorLog(e); }
            }
            return settings;
        }
        public void SaveMapSettings(MapSettings settings)
        {
            if (!Directory.Exists(propsPath)) Directory.CreateDirectory(propsPath);

            File.Create(propsPath + settings.name + ".properties").Dispose();
            using (StreamWriter SW = File.CreateText(propsPath + settings.name + ".properties"))
            {
                SW.WriteLine("#Lava Survival properties for " + settings.name);
                SW.WriteLine("fast-chance = " + settings.fast);
                SW.WriteLine("killer-chance = " + settings.killer);
                SW.WriteLine("destroy-chance = " + settings.destroy);
                SW.WriteLine("water-chance = " + settings.water);
                SW.WriteLine("layer-chance = " + settings.layer);
                SW.WriteLine("layer-height = " + settings.layerHeight);
                SW.WriteLine("layer-count = " + settings.layerCount);
                SW.WriteLine("layer-interval = " + settings.layerInterval);
                SW.WriteLine("round-time = " + settings.roundTime);
                SW.WriteLine("flood-time = " + settings.floodTime);
                SW.WriteLine("block-flood = " + settings.blockFlood.x + "," + settings.blockFlood.y + "," + settings.blockFlood.z);
                SW.WriteLine("block-layer = " + settings.blockLayer.x + "," + settings.blockLayer.y + "," + settings.blockLayer.z);
            }
        }

        public void AddMap(string name)
        {
            if (!maps.Contains(name.ToLower()))
            {
                maps.Add(name.ToLower());
                SaveSettings();
            }
        }
        public void RemoveMap(string name)
        {
            if (maps.Contains(name.ToLower()))
            {
                maps.Remove(name.ToLower());
                SaveSettings();
            }
        }
        public bool HasMap(string name)
        {
            return maps.Contains(name.ToLower());
        }

        // Getters/Setters
        public string GetVoteString()
        {
            string str = "";
            foreach (KeyValuePair<string, int> kvp in votes)
            {
                str += Server.DefaultColor + ", &5" + kvp.Key;
            }
            return votes.Count > 0 ? str.Remove(0, 4) : str;
        }

        public List<string> GetMaps()
        {
            return maps;
        }

        // Internal classes
        public class MapSettings
        {
            public string name;
            public byte fast, killer, destroy, water, layer;
            public int layerHeight, layerCount;
            public double layerInterval, roundTime, floodTime;
            public Pos blockFlood, blockLayer;

            public MapSettings(string name)
            {
                this.name = name;
                fast = 0;
                killer = 0;
                destroy = 0;
                water = 0;
                layer = 0;
                layerHeight = 3;
                layerCount = 10;
                layerInterval = 2;
                roundTime = 30;
                floodTime = 10;
                blockFlood = new Pos(0, 0, 0);
                blockLayer = new Pos(0, 0, 0);
            }
        }

        public class MapData : IDisposable
        {
            public bool fast, killer, destroy, water, layer;
            public byte block;
            public int currentLayer;
            public Timer roundTimer, floodTimer, layerTimer;

            public MapData(MapSettings settings)
            {
                fast = false;
                killer = false;
                destroy = false;
                water = false;
                layer = false;
                block = Block.lava;
                currentLayer = 1;
                roundTimer = new Timer(TimeSpan.FromMinutes(settings.roundTime).TotalMilliseconds); roundTimer.AutoReset = false;
                floodTimer = new Timer(TimeSpan.FromMinutes(settings.floodTime).TotalMilliseconds); floodTimer.AutoReset = false;
                layerTimer = new Timer(TimeSpan.FromMinutes(settings.layerInterval).TotalMilliseconds); layerTimer.AutoReset = true;
            }

            public void Dispose()
            {
                roundTimer.Dispose();
                floodTimer.Dispose();
                layerTimer.Dispose();
            }
        }

        public struct Pos
        {
            public ushort x, y, z;

            public Pos(ushort x, ushort y, ushort z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }
        }
    }
}
