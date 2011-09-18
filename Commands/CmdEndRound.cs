using System;
using System.IO;

namespace MCForge
{
    public class CmdEndRound : Command
    {
        public override string name { get { return "endround"; } }
        public override string shortcut { get { return "er"; } }
        public override string type { get { return "lava"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdEndRound() { }
        public override void Use(Player p, string message)
        {
            if (Server.lava.roundroundActive)
                Server.lava.EndRound();
            else
                Player.SendMessage(p, "The round must be active to end the round!");
        }
        public override void Help(Player p)
        {
            Player.SendMessage(p, "/endround - ends the round");
        }
    }
}