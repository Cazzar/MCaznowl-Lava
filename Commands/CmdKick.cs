/*
	Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCForge)
	
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

using System.Text.RegularExpressions;
using System.IO;

namespace MCForge
{
    public class CmdKick : Command
    {
        public override string name { get { return "kick"; } }
        public override string shortcut { get { return "k"; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.AdvBuilder; } }
        public CmdKick() { }

        public override void Use(Player p, string message)
        {
            if (message == "") { Help(p); return; }
            Player who = Player.Find(message.Split(' ')[0]);
            if (who == null) { Player.SendMessage(p, "Could not find player specified."); return; }
            if (message.Split(' ').Length > 1)
                message = message.Substring(message.IndexOf(' ') + 1);
            else
                if (p == null) message = "You were kicked by an IRC controller!"; else message = "You were kicked by " + p.name + "!";

            if (p != null)
                if (who == p)
                {
                    Player.SendMessage(p, "You cannot kick yourself!");
                    return;
                }
                else if (who.group.Permission >= p.group.Permission && p != null)
                {
                    Player.GlobalChat(p, p.color + p.name + Server.DefaultColor + " tried to kick " + who.color + who.name + " but failed.", false);
                    return;
                }

            string reason = "";
            Regex regex = new Regex(@"^([1-9]|[1-9][0-9]|[1-9][0-9][0-9])$");

            if (message.Substring(message.IndexOf(' ') + 1).Trim().StartsWith("@"))
            {
                if (!File.Exists("text/rules.txt"))
                {
                    File.WriteAllText("text/rules.txt", "No rules entered yet!");
                }
                int rulenumber = 0;
                try
                {
                    rulenumber = int.Parse(message.Substring(message.IndexOf(' ') + 1).Trim().Replace("@", ""));
                }
                catch { rulenumber = rulenumber; }
                StreamReader r = File.OpenText("text/rules.txt");
                while (!r.EndOfStream)
                {
                    string currentline = r.ReadLine();
                    try
                    {
                        if (int.Parse(currentline.Substring(0, 1)) == rulenumber)
                        {
                            reason = currentline;
                            reason = reason.Replace(Convert.ToString(rulenumber) + ". ", "");
                            who.Kick(reason);
                            return;
                        }
                    }
                    catch { }
                }

                if (regex.IsMatch(message))
                {
                    p.SendMessage("Invalid Rule Specified.");
                    return;
                }

                r.Close();
                r.Dispose();
            }
            else
            {
                who.Kick(message);
            }

            //who.Kick(message);
        }
        public override void Help(Player p)
        {
            Player.SendMessage(p, "/kick <player> [message] - Kicks a player.");
        }
    }
}