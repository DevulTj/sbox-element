using Sandbox.UI;
using Sandbox.UI.Construct;

namespace ElementGame
{
	public class GameStateInfo : Panel
	{
		public Label FirstText;
		public Label SecondText;

		public GameStateInfo()
		{
			StyleSheet.Load( "/UI/GameStateInfo.scss" );

			// HealthImage = Add.Label( "", "healthImage" );
			FirstText = Add.Label( "Waiting for players", "firstText" );
			SecondText = Add.Label( "1/2", "secondText" );
		}

		public override void Tick()
		{
			var player = Sandbox.Player.Local;
			if ( player == null ) return;

			var weapon = player.ActiveChild as ElementWeapon;
			var isValid = weapon != null;

			SetClass( "active", isValid );
		}
	}
}