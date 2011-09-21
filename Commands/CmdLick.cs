using System;

namespace MCForge
{
    public class CmdLick : Command
    {
        public override string name { get { return "lick"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
        public override void Use(Player p, string message)
        {
            if (message == "") { Help(p); return; }
            Player who = Player.Find(message);
            if (who == null)
            {
                Player.SendMessage(p, "Player is not online!");
            }
            else
            {
                Player.GlobalMessage(p.color + p.name + Server.DefaultColor + " licked " + who.color + who.name);
            }
        }
        public override void Help(Player p)
        {
            Player.SendMessage(p, "/lick <player> - Lick <player>");
        }
    }
}