using Sandbox;
using Sandbox.UI;
using System.Threading.Tasks;

namespace Element.FreeForAll
{
	public partial class MainRound : BaseRound
	{
		public override int Length => 20;
		public override string Name => "Free for all";

		protected override void OnBegin()
		{
			base.OnBegin();

			if ( Host.IsClient )
				ChatBox.AddInformation( "The game has begun." );

			DoLog( "Main Round Start" );
		}

		protected override void OnEnd()
		{
			base.OnEnd();

			if ( Host.IsClient )
				ChatBox.AddInformation( "Game over." );

			DoLog( "Main Round End" );
		}

		public override BaseRound GetNextRound()
		{
			return new StatsRound();
		}
	}
}
