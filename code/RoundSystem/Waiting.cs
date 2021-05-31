using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Element
{
	public class WaitingRound : BaseRound
	{
		public override string Name => "Waiting for players";

		public override BaseRound GetNextRound() => new FreeForAll.IntroRound();

		public override void OnClientJoined( Client cl )
		{
			if ( Client.All.Count >= Game.MinPlayers )
				End();
		}
	}
}
