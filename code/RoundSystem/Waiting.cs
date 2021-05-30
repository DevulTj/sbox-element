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

		public override void Begin()
		{
			Log.Info( "Started Lobby Round" );
		}

		public override void End()
		{
			Log.Info( "Finished Lobby Round" );
		}
	}
}
