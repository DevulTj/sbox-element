
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace ElementGame
{
	public class VitalsInfo : Panel
	{
		public Label HealthImage;
		public Label HealthAmount;

		public VitalsInfo()
		{
			StyleSheet.Load( "/UI/VitalsInfo.scss" );

			// HealthImage = Add.Label( "", "healthImage" );
			HealthAmount = Add.Label( "100", "healthAmount" );
		}

		public override void Tick()
		{
			var player = Sandbox.Player.Local;
			if ( player == null ) return;

			SetClass( "active", true );

			HealthAmount.Text = $"{ player.Health }%";
		}
	}
}