
using Sandbox;
using System;

namespace Element
{
	[Library]
	public class Slide : NetworkClass
	{
		public BasePlayerController Controller;

		public bool IsActive; // replicate
		public bool Wish;

		public float BoostTime = 1f;

		TimeSince Activated = 0;

		// You can only slide once every X
		public virtual float Cooldown => 2f;
		public virtual float MinimumSpeed => 64f;
		public virtual float WishDirectionFactor => 4f;

		public virtual float SlideIntensity => 1 - ( Activated / BoostTime );

		public Slide( BasePlayerController controller )
		{
			Controller = controller;
		}

		public void Reset()
		{
			Activated = Cooldown;
		}

		public virtual void PreTick()
		{
			bool isDown = Controller.Input.Down( InputButton.Duck );

			var oldWish = Wish;
			Wish = isDown;

			if ( Controller.Velocity.Length <= MinimumSpeed )
				StopTry();

			// No sliding while you're already in the sky
			if ( Controller.GroundEntity == null )
				StopTry();

			if ( oldWish == Wish )
				return;

			if ( isDown != IsActive )
			{
				if ( isDown ) Try();
				else StopTry();
			}

			if ( IsActive )
				Controller.SetTag( "sliding" );
		}

		void Try()
		{
			if ( Activated < Cooldown )
				return;

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
			var speedMult = Vector3.Dot( Controller.Velocity.Normal, Vector3.Cross( Controller.Rot.Up, hitNormal ) );

			wishdir *= WishDirectionFactor;

			if ( BoostTime > Activated )
				speedMult -= 1 - ( Activated / BoostTime );

			Controller.Velocity += wishdir + ( Controller.Velocity.Normal * MathF.Abs( speedMult ) * 20 );
		}
	}
}
