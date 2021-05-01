using Sandbox;

namespace ElementGame
{
	public partial class ElementPlayer : BasePlayer
	{
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

			var killCam = new KillCamera();

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

			new ViewPunch.Vertical( 10f, 0.6f );
			new ViewPunch.FOVImpact( -15f, 2f );
		}
	}
}
