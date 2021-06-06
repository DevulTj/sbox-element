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

		public float GetKillDeathRatio()
		{
			var result = Convert.ToSingle( Kills ) / Convert.ToSingle( Deaths );

			// Handle NaN
			if ( float.IsNaN( result ) )
			{
				return 1f;
			}
			// Handle cannot divide by zero
			else if ( Deaths == 0 )
			{
				return Kills;
			}
			// Round to 1 decimal point
			else
			{
				return MathF.Round( result, 1 );
			}
		}
	}

	partial class PlayerPawn
	{
		[Net] public PlayerStats Stats { get; private set; }

		partial void Construct()
		{
			Stats = new PlayerStats();
		}
	}
}
