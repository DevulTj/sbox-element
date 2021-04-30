using Sandbox;
using Sandbox.UI;

namespace ElementGame.ScreenShake
{
	public class BasicRecoil : CameraModifier
	{
		float MinAmount;
		float MaxAmount;
		float RecoveryTime;

		TimeSince lifeTime = 0;
		float pos = 0;

		public BasicRecoil( float minAmount = 0.1f, float maxAmount = 0.2f, float recoveryTime = 1.0f )
		{
			MinAmount = minAmount;
			MaxAmount = maxAmount;
			RecoveryTime = recoveryTime;
		}

		public override bool Update( Camera cam )
		{
			//var delta = ( (float)lifeTime ).LerpInverse( 0, RecoveryTime, true );
			//delta = Easing.EaseOut( delta );
			//var invdelta = 1 - delta;

			//pos += Time.Delta * 10 * invdelta * Speed;

			//float x = Noise.Perlin( pos, 0, NoiseZ );
			//float y = Noise.Perlin( pos, 3.0f, NoiseZ );

			//cam.Pos += ( cam.Rot.Right * x + cam.Rot.Up * y ) * invdelta * Size;
			//cam.Rot *= Rotation.FromAxis( Vector3.Up, x * Size * invdelta * RotationAmount );
			//cam.Rot *= Rotation.FromAxis( Vector3.Right, y * Size * invdelta * RotationAmount );

			//return lifeTime < Length;

			return false;
		}
	}
}
