using Sandbox;
using Sandbox.UI;
using System.Threading.Tasks;

namespace Element
{
	public partial class FFARound : BaseRound
	{
		public override int Length => 180;
		public override string Name => "Free for all";

		public override void Begin()
		{
			base.Begin();

			if ( Host.IsClient )
				ChatBox.AddInformation( "The game has begun." );
		}

		public override void End()
		{
			base.End();

			if ( Host.IsClient )
				ChatBox.AddInformation( "Game over." );
		}
	}
}
