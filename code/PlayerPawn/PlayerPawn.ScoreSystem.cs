using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Element
{
	public partial class PlayerStats : NetworkComponent
	{
		public static List<PlayerPawn> GetPlayersSorted() => Client.All.Select( x => x.Pawn as PlayerPawn ).OrderByDescending( x => x.Stats?.Kills ).ToList();
		// Easy accessor to get the winner
		public static PlayerPawn GetWinner() => GetPlayersSorted().First();

		[Net] protected int Kills { get; set; }
		[Net] protected int Deaths { get; set; }

		public void Clear()
		{
			if ( Host.IsServer )
			{
				Kills = 0;
				Deaths = 0;
			}
		}

		public void Kill() => Kills++;
		public void Die() => Deaths++;
		public int GetKills() => Kills;
		public int GetDeaths() => Deaths;

		public float GetKillDeathRatio() => Convert.ToSingle( Kills ) / Convert.ToSingle( Deaths );
	}

	partial class PlayerPawn
	{
		[Net] public PlayerStats Stats { get; private set; }
	}
}
