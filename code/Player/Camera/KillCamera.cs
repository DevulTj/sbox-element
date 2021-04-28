using Sandbox;

namespace ElementGame
{
    public partial class KillCamera : BaseCamera
    {
        public Vector3 TargetPos { get; private set; }

        public override void Activated()
        {
            base.Activated();

            Pos = LastPos;
            FieldOfView = 70;
        }

        public override void Update()
        {
            var player = Player.Local as ElementPlayer;
            var Target = player?.LastDamage.Attacker;

            if ( Target != null )
            {
                // Look towards our target
                Rot = Rotation.LookAt( Target.WorldPos, Vector3.Up );
            }

            FieldOfView.LerpTo( 70.0f, Time.Delta * 5.0f );

            Viewer = null;
        }
    }
}