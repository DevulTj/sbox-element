
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
		public virtual float JumpPowerUp => 700f;
		public virtual float JumpPowerUpDucked => 550f;
		public virtual float JumpPowerForward => 400f;
		public virtual float JumpPowerForwardDucked => 768f;

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
					Vector3 directionNormalized = controller.Velocity.Normal;

					if ( controller.Duck.IsActive )
                    {
						controller.QueueImpulse( directionNormalized * JumpPowerForwardDucked + controller.Rot.Up * JumpPowerUpDucked, true );
					}
					else
                    {
						controller.QueueImpulse( directionNormalized * JumpPowerForward + controller.Rot.Up * JumpPowerUp, true );
					}
				}
            }
        }
	}
}