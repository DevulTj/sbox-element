using Sandbox;

namespace ElementGame
{
    public partial class KillCamera : BaseCamera
    {
        public Vector3 TargetPos { get; private set; }


        public override void Activated()
        {
            base.Activated();

            Pos = LastPos + ( Player.Local.WorldRot.Forward * 200 );
            FieldOfView = 70;
        }

        public override void Update()
        {
            var Target = ( Player.Local as ElementPlayer )?.LastDamage.Attacker;
            var TargetPos = Target != null ? Target.WorldPos : LastPos;

            // Look towards our target
            Rot = Rotation.LookAt( TargetPos );

            DebugOverlay.Axis( TargetPos, Rot, 3.0f );


            FieldOfView.LerpTo( 70.0f, Time.Delta * 5.0f );
            Viewer = null;
        }
    }
}