using System;
using System.IO;

namespace MCForge
{
    public class CmdStartRound : Command
    {
        public override string name { get { return "startround"; } }
        public override string shortcut { get { return "sr"; } }
        public override string type { get { return "lava"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdStartRound() { }
        public override void Use(Player p, string message)
        {
            if (!Server.lava.roundroundActive)
            {
                Server.lava.DoFlood();
                Server.lava.mapData.floodTimer.Stop();
            }
            else
                Player.SendMessage(p, "The round must be non-active to start the round!");
        }
        public override void Help(Player p)
        {
            Player.SendMessage(p, "/startround - starts the round");
        }
    }
}