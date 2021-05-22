using Sandbox;

namespace Element
{
	[Library( "element_jumppad" )]
	public partial class JumpPad : ModelEntity
	{
		public virtual float JumpPowerUp => 700f;
		public virtual float JumpPowerUpDucked => 550f;
		public virtual float JumpPowerForward => 400f;
		public virtual float JumpPowerForwardDucked => 768f;

		public Particles Effect { get; set; }

		public JumpPad()
		{

		}

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/props/jumppadlow.vmdl" );
			SetupPhysicsFromCapsule( PhysicsMotionType.Keyframed, new Capsule( Vector3.Zero, Vector3.One * 0.1f, 16f ) );

			Transmit = TransmitType.Default;
			EnableTouch = true;
			CollisionGroup = CollisionGroup.Trigger;
			PhysicsEnabled = false;
		}

		public override void OnNewModel( Model model )
		{
			if ( Effect != null )
				return;

			Effect = Particles.Create( "particles/green_circle_teleporter.vpcf", this, "Base", true );
		}

		public override void StartTouch( Entity other )
		{
			base.StartTouch( other );

			if ( IsClient )
			{
				return;
			}

			if ( other is PlayerPawn player )
			{
				if ( player.GetActiveController() is WalkController controller )
				{
					var directionNormalized = controller.Velocity.Normal;
					var duck = controller.Duck.IsActive;
					var jumpPowerForward = duck ? JumpPowerForwardDucked : JumpPowerForward;
					var jumpPowerUp = duck ? JumpPowerUpDucked : JumpPowerUp;

					// Queue an impulse for when the movement controller can process it
					controller.QueueImpulse( directionNormalized * jumpPowerForward + controller.Rot.Up * jumpPowerUp, true );
					// Allow the player to jump again
					controller.ExtraJump( true, 300f, 368f );

					PlaySound( "jumppad" );

					player.ViewPunch( 10f, 0.6f );
				}
			}
		}
	}
}
