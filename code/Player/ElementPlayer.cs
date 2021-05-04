using Sandbox;
using System;

namespace ElementGame
{
	public partial class ElementPlayer : BasePlayer
	{
		public virtual bool IsSliding { 
			get {
				if ( GetActiveController() is WalkController controller )
					return controller.Slide.IsActive;
				else
					return false;
			}
		}

		[NetLocal]
		public DamageInfo LastDamage { get; protected set; }

		public ElementPlayer()
		{
			Inventory = new Inventory( this );
		}

		public override void Respawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );

			EnableTouch = true;

			//
			// Use WalkController for movement (you can make your own PlayerController for 100% control)
			//
			Controller = new WalkController();

			//
			// Use StandardPlayerAnimator  (you can make your own PlayerAnimator for 100% control)
			//
			Animator = new StandardPlayerAnimator();

			//
			// Use FirstPersonCamera (you can make your own Camera for 100% control)
			//
			Camera = new FirstPersonCamera();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			Inventory.Add( new Rifle(), true );
			Inventory.Add( new Shotgun() );

			GiveAmmo( AmmoType.Rifle, 60 );
			GiveAmmo( AmmoType.Pistol, 40 );
			GiveAmmo( AmmoType.Shotgun, 8 );

			base.Respawn();
		}

		/// <summary>
		/// Called every tick, clientside and serverside.
		/// </summary>
		protected override void Tick()
		{
			base.Tick();

			if ( IsServer )
			{
				WeaponSwitchTick();

				if ( Input.Pressed( InputButton.Flashlight ) )
				{
					var tr = Trace.Ray( EyePos, EyePos + EyeRot.Forward * 4096 )
						.Ignore( this )
						.Run();

					var entity = new AmmoPickup();
					entity.WorldPos = tr.EndPos;
				}
			}
		}

		public override void OnKilled()
		{
			base.OnKilled();

			//
			Inventory.DropActive();

			//
			// Delete any items we didn't drop
			//
			Inventory.DeleteContents();

			BecomeRagdollOnClient( LastDamage.Force, GetHitboxBone( LastDamage.HitboxIndex ) );

			Controller = null;

			var killCam = new SpectateRagdollCamera();

			Camera = killCam;

			EnableAllCollisions = false;
			EnableDrawing = false;
		}


		public override void TakeDamage( DamageInfo info )
		{
			LastDamage = info;

			base.TakeDamage( info );
		}

		public override void StartTouch( Entity other )
		{
			base.StartTouch( other );
		}

		[ClientRpc]
		public void JustHitJumpPad( Entity jumpPad )
		{
			Host.AssertClient();

			if ( this == Local )
			{
				new ViewPunch.Vertical( 10f, 0.6f );
				new ViewPunch.FOVImpact( -15f, 2f );
			}
		}

		float walkBob = 0;
		float lean = 0;
		public override void PostCameraSetup( Camera camera )
		{
				float speed = Velocity.Length.LerpInverse( 0, 320 );

				if ( GroundEntity != null )
					walkBob += Time.Delta * 25.0f * speed;

				// Camera lean
				lean = lean.LerpTo( Velocity.Dot( IsSliding ? camera.Rot.Right : Vector3.Zero ) * 0.03f, Time.Delta * 15.0f );

				var appliedLean = lean;
				appliedLean += MathF.Sin( walkBob ) * speed * 0.2f;
				camera.Rot *= Rotation.From( 0, 0, appliedLean );
		}
	}
}
