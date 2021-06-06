using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Element.FreeForAll
{
	public class IntroRound : BaseRound
	{
		public override int Length => 10;

		public override string StartMessage => "The game will start soon. All players were respawned.";

		public override string Name => "Starting in";

		protected override void OnBegin()
		{
			base.OnBegin();

			DoLog( "Started Intro" );

			if ( Host.IsServer )
			{
				// Clear player stats
				PlayerStats.ClearAll();
			}

			Client.All.Select( x => x.Pawn as PlayerPawn ).ToList().ForEach( x => x.Respawn() );
		}

		protected override void OnEnd()
		{
			base.OnEnd();

			DoLog( "Finished Intro" );
		}

		public override BaseRound GetNextRound()
		{
			return new MainRound();
		}
	}
}
