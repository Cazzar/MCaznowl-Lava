using System;
using System.Text;
using System.Security.Cryptography;

namespace MCForge
{
    public class CmdAnydurl : Command
    {
        public override string name { get { return "anydurl"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Nobody; } }

        public override void Use(Player p, string message)
        {
            Byte[] ob;
            string code, name;

            name = message.Split(' ')[0];

            ob = ASCIIEncoding.Default.GetBytes(Server.salt + name);

            code = BitConverter.ToString(new MD5CryptoServiceProvider().ComputeHash(ob)).Replace("-", "").ToLower();
            Player.SendMessage(p, "mc://127.0.0.1:" + Convert.ToString(Server.port) + "/" + name + "/" + code);
        }

        public override void Help(Player p)
        {
            Player.SendMessage(p, "/anydurl - Does naughty stuff.");
        }
    }
}