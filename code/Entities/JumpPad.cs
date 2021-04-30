
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementGame
{
	[Library( "element_jumppad" )]
	public partial class JumpPad : ModelEntity
	{
		public virtual float JumpPower => 4096;

		public override void Spawn()
		{
			base.Spawn();

			SetupPhysicsFromCapsule( PhysicsMotionType.Keyframed, new Capsule( Vector3.Zero, Vector3.One * 0.1f, 100f ) );

			Transmit = TransmitType.Default;
			EnableTouch = true;
			CollisionGroup = CollisionGroup.Trigger;
		}

		public override void StartTouch( Entity other )
        {
			base.StartTouch( other );

			if ( IsClient ) return;

			if ( other is Player player )
			{
				if ( player.GetActiveController() is WalkController controller )
                {
					controller.JumpPadEntity = this;
					//controller.ClearGroundEntity();

					//float startZ = Velocity.z;
					//// TODO - change this
					//float groundFactor = 1f;

					//Log.Error( "Jump" );

					//controller.Velocity = controller.Velocity.WithZ( startZ + JumpPower * groundFactor );
					//controller.AddEvent( "jump" );
				}
            }
        }
	}
}