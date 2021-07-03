﻿using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;

namespace Element
{
	// Make a struct for killstreaks
	// They hold the audio data
	public struct KillStreak
	{
		public int Amount { get; set; }
		public string SoundPath { get; set; }
		public string ChatMessage { get; set; }
	}

	public class KillStreakHandler
	{
		public KillStreakHandler() => Event.Register( this );
		~KillStreakHandler() => Event.Unregister( this );

		TimeSince LastKill = 0;
		float ResetKillTime = 5f;

		int CurrentKills = 0;

		IReadOnlyDictionary<int, KillStreak> KillStreaks = new Dictionary<int, KillStreak>()
		{
			{ 2, new KillStreak { SoundPath = "my.sound", ChatMessage = "{0} got a Double kill!" } },
			{ 3, new KillStreak { SoundPath = "my.sound", ChatMessage = "{0} got a Triple kill!" } },
		};
		
		[GameEvent.PlayerKilled]
		public void OnPlayerKilled( Entity attacker, Entity victim )
		{
			if ( Host.IsServer )
			{
				return;
			}

			if ( LastKill > ResetKillTime )
			{
				CurrentKills = 0;
			}

			LastKill = 0;
			CurrentKills++;

			if ( KillStreaks.TryGetValue( CurrentKills, out KillStreak streak ) )
			{
				if ( streak.SoundPath != "" )
				{
					Sound.FromScreen( streak.SoundPath );
				}

				if ( streak.ChatMessage != "" )
				{
					ChatBox.AddInformation( string.Format( streak.ChatMessage, attacker?.GetClientOwner()?.Name ?? "Player" ) );
				}
			}
		}
	}
}
