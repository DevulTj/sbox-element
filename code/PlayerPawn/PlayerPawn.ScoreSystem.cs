using Sandbox;
using System;

namespace Element
{
	public partial class PlayerStats : NetworkComponent
	{
		[Net] public int Kills { get; set; }
		[Net] public int Deaths { get; set; }

		public void Clear()
		{
			if ( Host.IsServer )
			{
				Kills = 0;
				Deaths = 0;
			}
		}

		public int GetKills() => Kills;
		public int GetDeaths() => Deaths;
		public float GetKillDeathRatio() => Convert.ToSingle( Kills ) / Convert.ToSingle( Deaths );
	}

	partial class PlayerPawn
	{
		[Net] public PlayerStats Stats { get; private set; }
	}
}
