using Sandbox;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Element
{
	/// <summary>
	/// This is the heart of the gamemode. It's responsible
	/// for creating the player and stuff.
	/// </summary>
	[Library( "element", Title = "Element" )]
	partial class Game : Sandbox.Game
	{
		[Net] public BaseRound Round { get; private set; }

		protected BaseRound _lastRound;

		[ServerVar( "element_min_players", Help = "The minimum players required to start." )]
		public static int MinPlayers { get; set; } = 2;

		public Game()
		{
			//
			// Create the HUD entity. This is always broadcast to all clients
			// and will create the UI panels clientside. It's accessible 
			// globally via Hud.Current, so we don't need to store it.
			//
			if ( IsServer )
			{
				new UI.GameHud();
			}

			_ = StartTickTimer();
		}

		public override void PostLevelLoaded()
		{
			base.PostLevelLoaded();

			ItemRespawn.Init();

			_ = StartSecondTimer();

			SetRound( new WaitingRound() );
		}

		public override void ClientJoined( Client cl )
		{
			base.ClientJoined( cl );

			var player = new PlayerPawn();
			player.Respawn();

			cl.Pawn = player;
		}

		public virtual void SetRound( BaseRound newRound = null )
		{
			// Prioritize the round specified by the OLD round.
			var roundToUse = newRound;

			if ( roundToUse == null )
				return;

			// Assign new one and start it.
			Round = newRound;
			Round.Begin();
		}

		// I hate this, but I'm just gonna follow suit with Hidden for now.
		public async Task StartSecondTimer()
		{
			while ( true )
			{
				await Task.DelaySeconds( 1 );
				OnSecond();
			}
		}

		// And you. You. UGH.
		public async Task StartTickTimer()
		{
			while ( true )
			{
				await Task.NextPhysicsFrame();
				OnTick();
			}
		}

		private void OnSecond()
		{
			Round?.SecondPassed();
		}

		private void OnTick()
		{
			Round?.Tick();

			if ( IsClient )
			{
				// This is a hack for networking. I hate it. I hate it. We must fix this in the future.
				if ( _lastRound != Round )
				{
					_lastRound?.End();
					_lastRound = Round;
				}
			}
		}
	}
}
