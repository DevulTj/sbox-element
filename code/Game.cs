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

		public Game()
		{
			//
			// Create the HUD entity. This is always broadcast to all clients
			// and will create the UI panels clientside. It's accessible 
			// globally via Hud.Current, so we don't need to store it.
			//
			if ( IsServer )
			{
				new UI.DeathmatchHud();
			}

			_ = StartTickTimer();
		}

		public override void PostLevelLoaded()
		{
			base.PostLevelLoaded();

			ItemRespawn.Init();

			_ = StartSecondTimer();
		}

		public override void ClientJoined( Client cl )
		{
			base.ClientJoined( cl );

			var player = new PlayerPawn();
			player.Respawn();

			cl.Pawn = player;
		}

		public virtual void ChangeRound( BaseRound newRound )
		{
			if ( newRound == null )
				return;

			// End active round
			Round?.End();

			// Assign new one and start it
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
			// CheckMinimumPlayers();
			Round?.SecondPassed();
		}

		private void OnTick()
		{
			Round?.Tick();
		}
	}
}
