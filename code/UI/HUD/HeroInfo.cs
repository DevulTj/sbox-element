using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Element
{
	public class HeroInfo : Panel
	{
		public Label HeroImage;
		public Label HeroName;

		public HeroInfo()
		{
			StyleSheet.Load( "/UI/HUD/HeroInfo.scss" );

			HeroImage = Add.Label( "", "heroIcon" );
			HeroName = Add.Label( "Volt", "heroName" );
		}

		public override void Tick()
		{
			var player = Local.Pawn as PlayerPawn;
			if ( player == null ) return;

			var weapon = player.ActiveChild as Weapon.BaseWeapon;
			var isValid = weapon != null;

			SetClass( "active", isValid );

			HeroName.Text = "Volt";
		}
	}
}
