
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Element.UI
{
	public partial class ScoreboardEntry : Panel
	{
		public Client Client;

		public Label PlayerName;
		public Label Kills;
		public Label Deaths;
		public Label Ratio;

		public ScoreboardEntry()
		{
			AddClass( "entry" );

			PlayerName = Add.Label( "PlayerName", "name" );
			Kills = Add.Label( "", "kills" );
			Deaths = Add.Label( "", "deaths" );
			Ratio = Add.Label( "", "ratio" );
		}

		public virtual void UpdateFrom( Client client )
		{
			Client = client;

			PlayerName.Text = client.Name;

			var pawn = client.Pawn as PlayerPawn;
			var stats = pawn?.Stats;

			if ( stats != null )
			{
				Kills.Text = stats.GetKills().ToString();
				Deaths.Text = stats.GetDeaths().ToString();
				Ratio.Text = stats.GetKillDeathRatio().ToString();
			}

			SetClass( "me", Local.Client == client );
		}
	}
}
