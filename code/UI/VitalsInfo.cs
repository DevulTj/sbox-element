
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

			var weapon = player.ActiveChild as ElementWeapon;
			var isValid = weapon != null;

			SetClass( "active", isValid );

			HealthAmount.Text = $"{ player.Health }%";
		}
	}
}