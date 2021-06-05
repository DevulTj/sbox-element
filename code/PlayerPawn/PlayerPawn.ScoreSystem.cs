using Sandbox;
using System;

namespace Element
{
	public partial class PlayerStats : NetworkComponent
	{
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
