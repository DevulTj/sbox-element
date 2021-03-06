using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Element
{
	public class FPSCamera : Camera
	{
		Vector3 _lastPos;

		public override void Activated()
		{
			var pawn = Local.Pawn as PlayerPawn;
			if ( pawn == null ) return;

			Pos = pawn.EyePos;
			Rot = pawn.EyeRot;

			_lastPos = Pos;
		}

		public override void BuildInput( InputBuilder input )
		{
			var pawn = Local.Pawn;
			if ( pawn == null ) return;

			var weapon = pawn.ActiveChild as Weapon.BaseWeapon;

			if ( weapon == null )
				return;

			var oldPitch = input.ViewAngles.pitch;
			input.ViewAngles.pitch -= weapon.CurrentRecoilAmount * Time.Delta;
			weapon.CurrentRecoilAmount -= (oldPitch - input.ViewAngles.pitch) * weapon.RecoilRecoveryScaleFactor;
			
			base.BuildInput( input );
		}

		public override void Update()
		{
			var pawn = Local.Pawn;
			if ( pawn == null ) return;

			var eyePos = pawn.EyePos;

			Pos = Vector3.Lerp( eyePos.WithZ( _lastPos.z ), eyePos, 20.0f * Time.Delta );
			Rot = pawn.EyeRot;

			FieldOfView = 80;

			Viewer = pawn;
			_lastPos = Pos;
		}
	}
}
