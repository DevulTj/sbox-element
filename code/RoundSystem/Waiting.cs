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

		protected override void OnBegin()
		{
			base.OnBegin();

			DoLog( "Started Lobby Round" );
		}

		protected override void OnEnd()
		{
			base.OnEnd();

			DoLog( "Finished Lobby Round" );
		}

		public override void SecondPassed()
		{
			var game = Game.Current as Game;
			if ( game == null ) return;

			if ( Client.All.Count >= Game.MinPlayers )
			{
				End();
			}
		}

		public override BaseRound GetNextRound()
		{
			return new FreeForAll.IntroRound();
		}
	}
}
