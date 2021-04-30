using Sandbox;
using Sandbox.UI;
using System;

namespace ElementGame.ScreenShake
{
	public class BasicRecoil : CameraModifier
	{
		float Angle;
		float TimeLength;

		TimeSince lifeTime = 0;

		BaseViewModel ViewModelRef;

		float Speed = 10f;

		Rotation LastRot;

		public BasicRecoil( BaseViewModel vm = null, float angle = -3f, float time = 0.6f )
		{
			ViewModelRef = vm;

			Angle = Rand.Float( angle * 0.5f, angle );
			TimeLength = time;
		}

		public override bool Update( Camera cam )
		{
			var delta = ( (float)lifeTime ).LerpInverse( 0, TimeLength, true );
			delta = (float)Math.Sin( (float)Math.PI * delta ) + 1f;

			var invdelta = 1 - delta;

			// Vector3 mouseDelta = ViewModelRef.Owner.Input.MouseDelta;

			if ( ViewModelRef != null )
			{
				ViewModelRef.WorldRot *= Rotation.FromAxis( Vector3.Right, Angle * invdelta );
				cam.Rot *= Rotation.FromAxis( Vector3.Right, ( Angle / 2 ) * invdelta );
			}

			return lifeTime < TimeLength;
		}
	}
}
