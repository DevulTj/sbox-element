
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

			if ( Controller.Velocity.Length <= 64f )
			{
				StopTry();

				return;
			}

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
			if ( !IsActive )
				return;

			IsActive = false;
		}

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
