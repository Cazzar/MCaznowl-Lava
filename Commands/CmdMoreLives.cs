/*
	Copyright 2010 MCLawl Team - Written by Valek (Modified for use with MCForge)
 
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

namespace MCForge
{
    public class CmdMoreLives : Command
    {
        public override string name { get { return "morelives"; } }
        public override string shortcut { get { return "ml"; } }
        public override string type { get { return "lava"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdMoreLives() { }
        public override void Use(Player p, string message)
        {
            int amountOfSponges = 1;

            if (IsNumeric(message))
            {
                p.lives = p.lives + Convert.ToInt32(message);
                Player.SendMessage(p, c.lime + "You have " + c.red + p.lives + c.lime + " lives left!");
            }
            else
            {
                p.lives = p.lives + amountOfSponges;
                Player.SendMessage(p, c.lime + "You have " + c.red + p.lives + c.lime + " lives left!");
            }
            string query;
            query = "UPDATE Players SET sponges = '" + Convert.ToString(p.lives) + "' WHERE Name = '" + p.name + "'";
            MySQL.executeQuery(query);

        }
        public override void Help(Player p)
        {
            Player.SendMessage(p, "/morelives [number] - Gives you more lives to place!");
        }

        private bool IsNumeric(string chkNumeric)
        {
            int intOutVal;
            bool isValidNumeric = false;
            isValidNumeric = int.TryParse(chkNumeric, out intOutVal);
            return isValidNumeric;
        }
    }
}