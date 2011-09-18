using System;

namespace MCForge
{
    public class CmdVote : Command
    {
        public override string name { get { return "vote"; } }
        public override string shortcut { get { return "vo"; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdVote() { }

        public override void Use(Player p, string message)
        {
            if (message == "") { Help(p); return; }
            if (Server.voting == true) { Player.SendMessage(p, "A vote is already in progress!"); }
            if (message == "@1")
            {
                if (Server.lava.roundroundActive == true)
                {
                    Player.SendMessage(p, "The round has already started!");
                }
                else
                {
                    Server.voting = true;
                    Server.NoVotes = 0;
                    Server.YesVotes = 0;
                    Player.GlobalMessage(" " + c.green + "VOTE: " + Server.DefaultColor + "Start the lava flood now? " + "(" + c.green + "Yes " + Server.DefaultColor + "/" + c.red + "No" + Server.DefaultColor + ")");
                    System.Threading.Thread.Sleep(30000);
                    Server.voting = false;
                    Player.GlobalMessage("The vote is in! " + c.green + "Y: " + Server.YesVotes + c.red + " N: " + Server.NoVotes);
                    if (Server.YesVotes > Server.NoVotes)
                    {
                        Player.GlobalMessage(c.maroon + "10 " + Server.DefaultColor + "Seconds until round start!");
                        System.Threading.Thread.Sleep(10000);
                        Server.lava.DoFlood();
                    }
                    Player.players.ForEach(delegate(Player winners)
                    {
                        winners.voted = false;
                    });
                }
                return;
            }
            if (message == "@2")
            {
                if (Server.lava.roundroundActive == false)
                {
                    Player.SendMessage(p, "The round hasn't started yet!");
                }
                else
                {
                    Server.voting = true;
                    Server.NoVotes = 0;
                    Server.YesVotes = 0;
                    Player.GlobalMessage(" " + c.green + "VOTE: " + Server.DefaultColor + "End the round now? " + "(" + c.green + "Yes" + Server.DefaultColor + "/" + c.red + "No" + Server.DefaultColor + ")");
                    System.Threading.Thread.Sleep(30000);
                    Server.voting = false;
                    Player.GlobalMessage("The vote is in! " + c.green + "Y: " + Server.YesVotes + c.red + " N: " + Server.NoVotes);
                    if (Server.YesVotes > Server.NoVotes)
                    {
                        Player.GlobalMessage(c.maroon + "10 " + Server.DefaultColor + "Seconds until round end!");
                        System.Threading.Thread.Sleep(10000);
                        Server.lava.EndRound();
                    }
                    Player.players.ForEach(delegate(Player winners)
                    {
                        winners.voted = false;
                    });
                }
                return;
            }
            else
            {
                string temp = message.Substring(0, 1) == "%" ? "" : Server.DefaultColor;
                Server.voting = true;
                Server.NoVotes = 0;
                Server.YesVotes = 0;
                Player.GlobalMessage(" " + c.green + "VOTE: " + temp + message + "(" + c.green + "Yes " + Server.DefaultColor + "/" + c.red + "No" + Server.DefaultColor + ")");
                System.Threading.Thread.Sleep(15000);
                Server.voting = false;
                Player.GlobalMessage("The vote is in! " + c.green + "Y: " + Server.YesVotes + c.red + " N: " + Server.NoVotes);
                Player.players.ForEach(delegate(Player winners)
                {
                    winners.voted = false;
                });
                return;
            }
        }
        public override void Help(Player p)
        {
            p.SendMessage("/vote [message] - Obviously starts a vote!");
            p.SendMessage("/vote @1 - Starts a start-lava-early vote!");
            p.SendMessage("/vote @2 - Starts a finish-lava-early vote!");
        }
    }
}