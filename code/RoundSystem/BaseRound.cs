
using Sandbox;
using System;

namespace Element
{
	public abstract partial class BaseRound : NetworkComponent
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

		public void DoLog( string text )
		{
			var State = Host.IsServer ? "SERVER" : "CLIENT";

			Log.Info( $"[{State}] " + text );
		}

		// Functionality
		public void Begin()
		{
			if ( Host.IsServer && Length > 0 )
			{
				EndTime = Time.Now + Length;
			}

			OnBegin();
		}

		protected virtual void OnBegin() { }

		public void End()
		{
			EndTime = 0f;

			OnEnd();

			( Game.Current as Game )?.SetRound( GetNextRound() );
		}

		protected virtual void OnEnd() { }

		public void Tick()
		{
			OnTick();
		}

		protected virtual void OnTick() { }

		public virtual void OnSecondPassed()
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

		public virtual void OnClientLeft( Client cl, NetworkDisconnectionReason reason )
		{

		}

		public virtual void OnClientJoined( Client cl )
		{
			
		}

		public virtual BaseRound GetNextRound()
		{
			return null;
		}
	}
}
