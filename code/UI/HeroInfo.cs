
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace ElementGame
{
	public class HeroInfo : Panel
	{
		public Label HeroImage;
		public Label HeroName;

		public HeroInfo()
		{
			StyleSheet.Load( "/UI/HeroInfo.scss" );

			HeroImage = Add.Label( "", "heroIcon" );
			HeroName = Add.Label( "Volt", "heroName" );
		}

		public override void Tick()
		{
			var player = Sandbox.Player.Local;
			if ( player == null ) return;

			SetClass( "active", true );

			HeroName.Text = "Volt";
		}
	}
}