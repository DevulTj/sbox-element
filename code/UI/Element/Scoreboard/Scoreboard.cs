
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Element.UI
{
	public class Scoreboard : Sandbox.UI.Scoreboard<ScoreboardEntry>
	{

		public Scoreboard()
		{
			StyleSheet.Load( "/UI/Element/Scoreboard/Scoreboard.scss" );
		}

		protected override void AddHeader()
		{
			Header = Add.Panel( "header" );
			Header.Add.Label( "name", "name" );
			Header.Add.Label( "kills", "kills" );
			Header.Add.Label( "deaths", "deaths" );
			Header.Add.Label( "ping", "ping" );
		}
	}

	public class ScoreboardEntry : Sandbox.UI.ScoreboardEntry
	{
		public ScoreboardEntry()
		{
		}

		public override void UpdateFrom( PlayerScore.Entry entry )
		{
			base.UpdateFrom( entry );
		}
	}
}
