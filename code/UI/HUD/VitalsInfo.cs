using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using BaseWeapon = Element.Weapon.BaseWeapon;

namespace Element.UI
{
	public class VitalsInfo : Panel
	{
		public Label HealthImage;
		public Label HealthAmount;

		public VitalsInfo()
		{
			StyleSheet.Load( "/UI/HUD/VitalsInfo.scss" );

			// HealthImage = Add.Label( "", "healthImage" );
			HealthAmount = Add.Label( "100", "healthAmount" );
		}

		public override void Tick()
		{
			var player = Local.Pawn;
			if ( player == null ) return;

			var isValid = player.ActiveChild is BaseWeapon weapon;

			SetClass( "active", isValid );

			HealthAmount.Text = $"{ player.Health }%";
		}
	}
}
