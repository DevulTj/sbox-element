
using Sandbox;
using Sandbox.UI;
using System;

namespace Element
{
	public abstract partial class BaseRound : NetworkComponent
	{
		public virtual int Length => 0;
		public virtual string Name => "Round";
		// A chatbox message for when this round starts
		public virtual string StartMessage => "";
		// A chatbox message for when this round ends
		public virtual string EndMessage => "";

		[Net] public float EndTime { get; set; }

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

			if ( Host.IsClient && StartMessage.Length > 0 )
				ChatBox.AddInformation( StartMessage );

			OnBegin();
		}

		protected virtual void OnBegin() { }

		public void End( BaseRound newRound = null )
		{
			EndTime = 0f;

			OnEnd();

			if ( Host.IsClient && EndMessage.Length > 0 )
				ChatBox.AddInformation( EndMessage );

			( Game.Current as Game ).SetRound( newRound ?? GetNextRound() );
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
