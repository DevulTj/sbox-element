
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace Element.UI
{
	public partial class ScoreboardBase<T> : Panel where T : ScoreboardEntry, new()
	{
		public Panel Canvas { get; protected set; }
		public TextEntry Input { get; protected set; }

		Dictionary<int, T> Entries = new();

		public Panel Header { get; protected set; }

		protected bool IsOpen = false;

		public ScoreboardBase()
		{
			AddClass( "scoreboard" );


			AddHeader();

			Canvas = Add.Panel( "canvas" );

			//PlayerScore.OnPlayerAdded += AddPlayer;
			//PlayerScore.OnPlayerUpdated += UpdatePlayer;
			//PlayerScore.OnPlayerRemoved += RemovePlayer;

			foreach ( var client in Client.All )
			{
				AddClient( client );
			}
		}

		protected virtual void OnStateChanged( bool bNowOpen )
		{
			if ( !bNowOpen )
				return;

			foreach ( var client in Client.All )
			{
				Update( client );
			}

			Canvas.SortChildren<ScoreboardEntry>( x => -x.Kills.Text.ToInt() );
		}

		public override void Tick()
		{
			base.Tick();

			var lastState = IsOpen;
			IsOpen = Local.Client?.Input.Down( InputButton.Score ) ?? false;

			// On change
			if ( lastState != IsOpen )
			{
				OnStateChanged( IsOpen );
			}

			SetClass( "open", IsOpen );
		}

		protected virtual void AddHeader()
		{
			Header = Add.Panel( "header" );
			Header.Add.Label( "Name", "name" );
			Header.Add.Label( "Kills", "kills" );
			Header.Add.Label( "Deaths", "deaths" );
			Header.Add.Label( "Ratio", "ratio" );
		}

		public virtual void AddClient( Client client )
		{
			var p = Canvas.AddChild<T>();
			p.UpdateFrom( client );

			Entries[ client.NetworkIdent ] = p;
		}

		public virtual void Update( Client client )
		{
			if ( Entries.TryGetValue( client.NetworkIdent, out var panel ) )
			{
				panel.UpdateFrom( client );
			}
			else // If we didn't exist, add!
			{
				AddClient( client );
			}
		}
	}

	public class Scoreboard : ScoreboardBase<ScoreboardEntry>
	{

		public Scoreboard()
		{
			StyleSheet.Load( "/UI/Element/Scoreboard/Scoreboard.scss" );
		}

		protected override void AddHeader()
		{
			Header = Add.Panel( "header" );
			Header.Add.Label( "Name", "name" );
			Header.Add.Label( "Kills", "kills" );
			Header.Add.Label( "Deaths", "deaths" );
			Header.Add.Label( "Ratio", "ratio" );
		}
	}
}
