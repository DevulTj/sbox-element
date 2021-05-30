using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Element
{
	public class RoundInfo : Panel
	{
		public Label FirstText;
		public Label SecondText;

		public RoundInfo()
		{
			StyleSheet.Load( "/UI/HUD/RoundInfo.scss" );

			FirstText = Add.Label( "Waiting for players", "firstText" );
			SecondText = Add.Label( "", "secondText" );
		}

		public override void Tick()
		{
			var player = Local.Pawn;
			if ( player == null ) return;

			var round = (Game.Current as Game)?.Round;

			if ( round != null )
			{
				FirstText.Text = round.Name;

				SecondText.Text = round.TimeLeftFormatted;
			}

			SetClass( "active", round != null );
		}
	}
}
