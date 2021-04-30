using Sandbox;
using Sandbox.UI;
using System;

namespace ElementGame.ViewPunch
{
	public class Vertical : CameraModifier
	{
		float Angle;
		float TimeLength;

		TimeSince lifeTime = 0;

		float Speed = 10f;

		public Vertical( float angle, float time )
		{
			Angle = angle;
			TimeLength = time;
		}

		public override bool Update( Camera cam )
		{
			var delta = ( (float)lifeTime ).LerpInverse( 0, TimeLength, true );
			delta = (float)Math.Sin( (float)Math.PI * delta ) + 1f;

			var invdelta = 1 - delta;

			cam.Rot *= Rotation.FromAxis( Vector3.Right, Angle * invdelta );

			return lifeTime < TimeLength;
		}
	}
}
