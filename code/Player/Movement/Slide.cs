
using Sandbox;
using System;

namespace ElementGame
{
	[Library]
	public class Slide : NetworkClass
	{
		public BasePlayerController Controller;

		public bool IsActive; // replicate

		public float BoostTime = 1f;

		TimeSince Activated = 0;

		public Slide( BasePlayerController controller )
		{
			Controller = controller;
		}

		public virtual void PreTick()
		{
			bool wants = Controller.Input.Down( InputButton.Duck ) && Controller.Input.Down( InputButton.Run );

			//// No sliding while you're already in the sky
			if ( Controller.GroundEntity == null )
				return;

			if ( wants != IsActive )
			{
				if ( wants ) Try();
				else StopTry();
			}

			if ( IsActive )
			{
				Controller.SetTag( "sliding" );
			}
		}

		void Try()
		{
			var change = IsActive != true;

			IsActive = true;

			if ( change )
				Activated = 0;
		}

		void StopTry()
		{
			var pm = Controller.TraceBBox( Controller.Pos, Controller.Pos, originalMins, originalMaxs );
			if ( pm.StartedSolid ) return;

			IsActive = false;
		}

		// Uck, saving off the bbox kind of sucks
		// and we should probably be changing the bbox size in PreTick
		Vector3 originalMins;
		Vector3 originalMaxs;

		//
		// Coudl we do this in a generic callback too?
		//
		public float GetWishSpeed()
		{
			if ( !IsActive ) return -1;
			return 64;
		}

		internal void Accelerate( ref Vector3 wishdir, ref float wishspeed, ref float speedLimit, ref float acceleration )
		{
			var hitNormal = Controller.GroundNormal;
			var speedMult = Vector3.Dot( Controller.Rot.Right, Vector3.Cross( Controller.Rot.Up, hitNormal ) );

			if ( BoostTime > Activated )
			{
				var multiplier = ( 1 - ( Activated / BoostTime ) );

				speedMult -= multiplier;
			}

			Controller.Velocity += wishdir * MathF.Abs( speedMult ) * 20;
		}
	}
}
