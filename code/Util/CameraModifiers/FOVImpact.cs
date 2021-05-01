using Sandbox;
using Sandbox.UI;
using System;

namespace ElementGame.ViewPunch
{
	public class FOVImpact : CameraModifier
	{
		float AdditiveFOV;
		float TimeLength;

		float cachedFov = 0f;

		TimeSince lifeTime = 0;

		public FOVImpact( float additiveFov, float timeLength )
		{
			AdditiveFOV = additiveFov;
			TimeLength = timeLength;
		}

		public override bool Update( Camera cam )
		{
			if ( cachedFov == 0f )
				cachedFov = cam.FieldOfView;

			var delta = ( (float)lifeTime ).LerpInverse( 0, TimeLength, true );
			delta = (float)Math.Sin( (float)Math.PI * delta ) + 1f;

			var invdelta = 1 - delta;

			cam.FieldOfView = cachedFov + ( invdelta * AdditiveFOV );

			return lifeTime < TimeLength;
		}
	}
}
