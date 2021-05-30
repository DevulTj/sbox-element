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

		protected override void OnBegin()
		{
			base.OnBegin();

			DoLog( "Started Stats View" );
		}

		protected override void OnEnd()
		{
			base.OnEnd();

			DoLog( "Finished Stats View" );
		}

		public override BaseRound GetNextRound()
		{
			return new IntroRound();
		}
	}
}
