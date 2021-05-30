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
		public override int Length => 5;

		public override string Name => "Starting in";

		protected override void OnBegin()
		{
			base.OnBegin();

			DoLog( "Started Intro" );
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
