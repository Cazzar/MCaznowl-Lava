/*
	Copyright 2011 MCForge
		
	Dual-licensed under the	Educational Community License, Version 2.0 and
	the GNU General Public License, Version 3 (the "Licenses"); you may
	not use this file except in compliance with the Licenses. You may
	obtain a copy of the Licenses at
	
	http://www.osedu.org/licenses/ECL-2.0
	http://www.gnu.org/licenses/gpl-3.0.html
	
	Unless required by applicable law or agreed to in writing,
	software distributed under the Licenses are distributed on an "AS IS"
	BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
	or implied. See the Licenses for the specific language governing
	permissions and limitations under the Licenses.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MCForge
{
    public class CmdStore : Command
    {
        public override string name { get { return "store"; } }
        public override string shortcut { get { return "buy"; } }
        public override string type { get { return "zombie"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.AdvBuilder; } }
        public CmdStore() { }
        int message12 = 0;

        public override void Use(Player p, string message)
        {
            if (message == "")
            {
                Help(p);
                return;
            }
            else
            {

                string message1 = "";
                int pos = message.IndexOf(' ');
                if (message.Split(' ').Length == 1) { }
                else
                {
                    if (message.Split(' ').Length > 1) message1 = message.Substring(pos + 1);
                    message = message.Split(' ')[0];
                }

                int id = int.Parse(message);

                switch (id)
                {
                    case 1:
                        if (p.money >= 250)
                        {
                            if (!message1.Equals(""))
                            {
                                if (message1.Length < 17)
                                {
                                    Command.all.Find("title").Use(p, p.name + " " + message1);
                                    p.money = p.money - 250;
                                }
                                else
                                {
                                    p.SendMessage("Title must be less than 17 characters!");
                                }
                            }
                            else
                            {
                                p.SendMessage("You have to specify a title (e.g /buy 1 hello)");
                            }
                        }
                        else
                        {
                            p.SendMessage("You do not have enough " + Server.moneys);
                        }
                        break;
                    case 2:
                        if (p.money >= 250)
                        {
                            if (!message1.Equals(""))
                            {
                                string color = c.Parse(message1);
                                if (color == "") { Player.SendMessage(p, "There is no color \"" + message1 + "\"."); break; }
                                if (p.prefix == "") { Player.SendMessage(p, "You must have a title."); break; }
                                Command.all.Find("tcolor").Use(p, p.name + " " + message1);
                                p.money = p.money - 250;
                            }
                            else
                            {
                                p.SendMessage("You have to specify a color (e.g /buy 2 gold)");
                            }
                        }
                        else
                        {
                            p.SendMessage("You do not have enough " + Server.moneys);
                        }
                        break;
                    case 3:
                        message12 = 1;
                        try
                        {
                            message12 = int.Parse(message1);
                        }
                        catch { message12 = 1; }
                        if (p.money >= (1 * message12))
                        {
                            if (!message1.Equals(""))
                            {
                                    Command.all.Find("morelives").Use(p, Convert.ToString(message12));
                                    p.money = p.money - 1;
                            }
                            else
                            {
                                Command.all.Find("morelives").Use(p, Convert.ToString(message12));
                                p.money = p.money - (1 * message12);
                            }
                        }
                        else
                        {
                            p.SendMessage("You do not have enough " + Server.moneys);
                        }
                        break;
                    case 4:
                        message12 = 1;
                        try
                        {
                            message12 = int.Parse(message1);
                        }
                        catch { message12 = 1; }
                        if (p.money >= (10 * message12))
                        {
                            if (!message1.Equals(""))
                            {
                                    Command.all.Find("moresponges").Use(p, Convert.ToString(message12));
                                    p.money = p.money - 1;
                            }
                            else
                            {
                                Command.all.Find("moresponges").Use(p, Convert.ToString(message12));
                                p.money = p.money - (10 * message12);
                            }
                        }
                        else
                        {
                            p.SendMessage("You do not have enough " + Server.moneys);
                        }
                        break;
                    case 5:
                        if (p.money >= 55)
                        {
                            Command.all.Find("moresponges").Use(p, "8");
                            p.money = p.money - 55;
                        }
                        else
                        {
                            p.SendMessage("You do not have enough " + Server.moneys);
                        }
                        break;
                    case 6:
                            if(p.group.name.ToLower() == "guest" && p.money >= 50)
                            { p.money = p.money - 50; Command.all.Find("promote").Use(null, p.name); }
                            else if (p.group.name.ToLower() == "builder" && p.money >= 150)
                            { p.money = p.money - 150; Command.all.Find("promote").Use(null, p.name); }
                            else if (p.group.name.ToLower() == "advbuilder" && p.money >= 300)
                            { p.money = p.money - 300; Command.all.Find("promote").Use(null, p.name); }
                            else if (p.group.name.ToLower() == "guest" && p.money <= 50)
                                p.SendMessage("You do not have enough " + Server.moneys);
                            else if (p.group.name.ToLower() == "builder" && p.money <= 150)
                                p.SendMessage("You do not have enough " + Server.moneys);
                            else if (p.group.name.ToLower() == "advbuilder" && p.money <= 300)
                                p.SendMessage("You do not have enough " + Server.moneys);
                            else
                                p.SendMessage("You already have the max rank!");
                        break;
                    case 7:
                        if (p.money >= 100)
                        {
                            if (!message1.Equals(""))
                            {
                                if (message1.Length < 30)
                                {
                                    Command.all.Find("loginmessage").Use(p, p.name + " " + message1);
                                    p.money = p.money - 100;
                                }
                                else
                                {
                                    p.SendMessage("Login Message must be less than 30 characters!");
                                }
                            }
                            else
                            {
                                p.SendMessage("You have to specify a login message (e.g /buy 6 hello)");
                            }
                        }
                        else
                        {
                            p.SendMessage("You do not have enough " + Server.moneys);
                        }
                        break;
                    case 8:
                        if (p.money >= 100)
                        {
                            if (!message1.Equals(""))
                            {
                                if (message1.Length < 30)
                                {
                                    Command.all.Find("logoutmessage").Use(p, p.name + " " + message1);
                                    p.money = p.money - 100;
                                }
                                else
                                {
                                    p.SendMessage("Logout message must be less than 30 characters!");
                                }
                            }
                            else
                            {
                                p.SendMessage("You have to specify a login message (e.g /buy 8 hello)");
                            }
                        }
                        else
                        {
                            p.SendMessage("You do not have enough " + Server.moneys);
                        }
                        break;
                    default:
                        Help(p);
                        break;
                }

            }
        }
        public override void Help(Player p)
        {
            p.SendMessage(c.lime + "To purchase an item, type /buy [item number] [amount] (except for item 5)");
            p.SendMessage(c.blue + "1. " + Server.DefaultColor + "Title - 250 " + Server.moneys);
            p.SendMessage(c.blue + "2. " + Server.DefaultColor + "Title Color - 150 " + Server.moneys);
            p.SendMessage(c.blue + "3. " + Server.DefaultColor + "Lives - 1 " + Server.moneys + " per life");
            p.SendMessage(c.blue + "4. " + Server.DefaultColor + "Sponges - 10 " + Server.moneys + " per sponge");
            p.SendMessage(c.blue + "5. " + Server.DefaultColor + "Sponge Pack - 55 " + Server.moneys + " for 8 sponges");
                if (p.group.name.ToLower() == "guest" && p.money >= 50)
                    p.SendMessage(c.blue + "6. " + Server.DefaultColor + "Rank Up - 50 (builder)" + Server.moneys);
                else if (p.group.name.ToLower() == "builder" && p.money >= 150)
                    p.SendMessage(c.blue + "6. " + Server.DefaultColor + "Rank Up - 150 (advbuilder)" + Server.moneys);
                else if (p.group.name.ToLower() == "advbuilder" && p.money >= 300)
                    p.SendMessage(c.blue + "6. " + Server.DefaultColor + "Rank Up - 300 (masterbuilder" + Server.moneys);
                else if (p.group.name.ToLower() == "guest" && p.money <= 50)
                    p.SendMessage("You do not have enough " + Server.moneys + " (50)");
                else if (p.group.name.ToLower() == "builder" && p.money <= 150)
                    p.SendMessage("You do not have enough " + Server.moneys + " (150)");
                else if (p.group.name.ToLower() == "advbuilder" && p.money <= 300)
                    p.SendMessage("You do not have enough " + Server.moneys + " (300)");
                else
                    p.SendMessage("Out of stock!");

            p.SendMessage(c.blue + "7. " + Server.DefaultColor + "Login Message - 200 " + Server.moneys);
            p.SendMessage(c.blue + "8. " + Server.DefaultColor + "Logout Message - 200 " + Server.moneys);
            return;
        }
    }
}
