
using Sandbox;
using System;

namespace ElementGame
{
	public abstract partial class BaseRound : NetworkClass
	{
		// Configuration
		public virtual int Length => 0;
		public virtual string Name => "Round";

		// Networked variables
		[Net]
		public float EndTime { get; set; }

		// Properties
		public string TimeLeftFormatted
		{
			get { return TimeSpan.FromSeconds( TimeRemaining ).ToString( @"mm\:ss" ); }
		}

		public float TimeRemaining
		{
			get { return EndTime - Time.Now; }
		}

		// Functionality

		public virtual void Begin()
		{
			if ( Host.IsServer && Length > 0 )
			{
				EndTime = Time.Now + Length;
			}
		}

		public virtual void End()
		{
			EndTime = 0f;
		}

		public virtual void Tick()
		{

		}

		public virtual void SecondPassed()
		{
			if ( Host.IsServer )
			{
				if ( EndTime > 0 && Time.Now >= EndTime )
				{
					EndTime = 0f;

					End();
				}
			}
		}
	}
}