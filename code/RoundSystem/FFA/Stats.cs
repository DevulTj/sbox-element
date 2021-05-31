using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Element.FreeForAll
{
	// Called at the end of a game round, to display stats and other cool information :)
	public class StatsRound : BaseRound
	{
		public override int Length => 10;
		public override string Name => "Round over";
		public override BaseRound GetNextRound() => Client.All.Count < Game.MinPlayers ? new WaitingRound() : new IntroRound();
	}
}
