using Sandbox;
using System;
using System.Linq;

namespace Element
{
	/// <summary>
	/// This is the heart of the gamemode. It's responsible
	/// for creating the player and stuff.
	/// </summary>
	[Library( "element", Title = "Element" )]
	partial class Game : Sandbox.Game
	{
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
		}

		public override void PostLevelLoaded()
		{
			base.PostLevelLoaded();

			ItemRespawn.Init();
		}

		public override void ClientJoined( Client cl )
		{
			base.ClientJoined( cl );

			var player = new PlayerPawn();
			player.Respawn();

			cl.Pawn = player;
		}
	}
}
