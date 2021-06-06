using Sandbox;
using System;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace Element
{
	public partial class PlayerPawn : Player
	{
		TimeSince _timeSinceDropped;

		public bool SupressPickupNotices { get; private set; }

		[Net, Local] public bool NoMove { get; set; }

		public virtual bool IsSliding {
			get {
				if ( GetActiveController() is WalkController controller )
					return controller.Slide.IsActive;
				else
					return false;
			}
		}

		partial void Construct();

		public PlayerPawn()
		{
			Inventory = new Inventory( this );

			Construct();
		}

		public override void Respawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );

			Controller = new Element.WalkController();
			Animator = new StandardPlayerAnimator();
			Camera = new FPSCamera();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			Dress();
			ClearAmmo();

			SupressPickupNotices = true;

			Inventory.Add( new Weapon.Pistol(), true );
			Inventory.Add( new Weapon.Shotgun() );
			Inventory.Add( new Weapon.SMG() );

			GiveAmmo( AmmoType.Pistol, 100 );
			GiveAmmo( AmmoType.Buckshot, 8 );

			SupressPickupNotices = false;
			Health = 100;

			base.Respawn();
		}

		public override void OnKilled()
		{
			base.OnKilled();

			Inventory.DropActive();
			Inventory.DeleteContents();

			BecomeRagdollOnClient( LastDamage.Force, GetHitboxBone( LastDamage.HitboxIndex ) );

			Controller = null;
			Camera = new SpectateRagdollCamera();

			EnableAllCollisions = false;
			EnableDrawing = false;
		}


		public override void Simulate( Client cl )
		{
			//if ( cl.NetworkIdent == 1 )
			//	return;

			base.Simulate( cl );

			//
			// Input requested a weapon switch
			//
			if ( Input.ActiveChild != null )
			{
				ActiveChild = Input.ActiveChild;
			}

			if ( LifeState != LifeState.Alive )
				return;

			TickPlayerUse();

			if ( Input.Pressed( InputButton.View ) )
			{
				if ( Camera is ThirdPersonCamera )
				{
					Camera = new FirstPersonCamera();
				}
				else
				{
					Camera = new ThirdPersonCamera();
				}
			}

			if ( Input.Pressed( InputButton.Drop ) )
			{
				var dropped = Inventory.DropActive();
				if ( dropped != null )
				{
					if ( dropped.PhysicsGroup != null )
					{
						dropped.PhysicsGroup.Velocity = Velocity + (EyeRot.Forward + EyeRot.Up) * 300;
					}

					_timeSinceDropped = 0;
					SwitchToBestWeapon();
				}
			}

			SimulateActiveChild( cl, ActiveChild );

			//
			// If the current weapon is out of ammo and we last fired it over half a second ago
			// lets try to switch to a better wepaon
			//
			if ( ActiveChild is Weapon.BaseWeapon weapon && !weapon.IsUsable() && weapon.TimeSincePrimaryAttack > 0.5f &&
			     weapon.TimeSinceSecondaryAttack > 0.5f )
			{
				SwitchToBestWeapon();
			}
		}

		public void SwitchToBestWeapon()
		{
			var best = Children.Select( x => x as Weapon.BaseWeapon )
				.Where( x => x.IsValid() && x.IsUsable() )
				.OrderByDescending( x => x.BucketWeight )
				.FirstOrDefault();

			if ( best == null ) return;

			ActiveChild = best;
		}

		public override void StartTouch( Entity other )
		{
			if ( _timeSinceDropped < 1 ) return;

			base.StartTouch( other );
		}

		Rotation lastCameraRot = Rotation.Identity;

		public override void PostCameraSetup( ref CameraSetup setup )
		{
			base.PostCameraSetup( ref setup );

			if ( lastCameraRot == Rotation.Identity )
				lastCameraRot = setup.Rotation;

			var angleDiff = Rotation.Difference( lastCameraRot, setup.Rotation );
			var angleDiffDegrees = angleDiff.Angle();
			var allowance = 20.0f;

			if ( angleDiffDegrees > allowance )
			{
				// We could have a function that clamps a rotation to within x degrees of another rotation?
				lastCameraRot = Rotation.Lerp( lastCameraRot, setup.Rotation, 1.0f - (allowance / angleDiffDegrees) );
			}
			else
			{
				//lastCameraRot = Rotation.Lerp( lastCameraRot, Camera.Rotation, Time.Delta * 0.2f * angleDiffDegrees );
			}

			// uncomment for lazy cam
			//camera.Rotation = lastCameraRot;

			if ( setup.Viewer != null )
			{
				AddCameraEffects( ref setup );
			}
		}

		float walkBob = 0;
		float lean = 0;
		float fov = 0;

		private void AddCameraEffects( ref CameraSetup setup )
		{
			var speed = Velocity.Length.LerpInverse( 0, 320 );
			var forwardspeed = Velocity.Normal.Dot( setup.Rotation.Forward );

			var left = setup.Rotation.Left;
			var up = setup.Rotation.Up;

			if ( GroundEntity != null )
			{
				walkBob += Time.Delta * 10.0f * speed;
			}

			setup.Position += up * MathF.Sin( walkBob ) * speed * 2;
			setup.Position += left * MathF.Sin( walkBob * 0.6f ) * speed * 1;

			// Camera lean
			lean = lean.LerpTo( Velocity.Dot( setup.Rotation.Right ) * 0.01f, Time.Delta * 15.0f );

			var controller = Controller as WalkController;
			var wallRun = controller.WallRun;
			var wallRunLean = wallRun.GetWallJumpDirection();

			if ( wallRun.IsWallRunning )
			{
				Log.Info( "wall run lean: " + wallRunLean.ToString()  );
				Log.Info( "dot: " + wallRunLean.Dot( setup.Rotation.Right ) * 0.01f  );
				lean = lean.LerpTo( ( wallRunLean.Dot( setup.Rotation.Right ) * 0.01f ) * 20f, Time.Delta * 10.0f );
			}

			var appliedLean = lean;
			appliedLean += MathF.Sin( walkBob ) * speed * 0.2f;
			setup.Rotation *= Rotation.From( 0, 0, appliedLean );

			speed = (speed - 0.7f).Clamp( 0, 1 ) * 3.0f;

			fov = fov.LerpTo( speed * 5 * MathF.Abs( forwardspeed ), Time.Delta * 2.0f );

			setup.FieldOfView += fov;
		}

		DamageInfo LastDamage;

		public override void TakeDamage( DamageInfo info )
		{
			LastDamage = info;

			// hack - hitbox 0 is head
			// we should be able to get this from somewhere
			if ( info.HitboxIndex == 0 )
			{
				info.Damage *= 2.0f;
			}

			base.TakeDamage( info );

			if ( info.Attacker is PlayerPawn attacker && attacker != this )
			{
				// Note - sending this only to the attacker!
				attacker.DidDamage( To.Single( attacker ), info.Position, info.Damage,
					((float)Health).LerpInverse( 100, 0 ) );

				TookDamage( To.Single( this ), info.Weapon.IsValid() ? info.Weapon.Position : info.Attacker.Position );
			}
		}

		[ClientRpc]
		public void DidDamage( Vector3 pos, float amount, float healthinv )
		{
			Sound.FromScreen( "dm.ui_attacker" )
				.SetPitch( 1 + healthinv * 1 );

			HitIndicator.Current?.OnHit( pos, amount );
		}

		[ClientRpc]
		public void TookDamage( Vector3 pos )
		{
			//DebugOverlay.Sphere( pos, 5.0f, Color.Red, false, 50.0f );

			DamageIndicator.Current?.OnHit( pos );
		}
		
		[ClientRpc]
		public void ViewPunch( float Angle, float Time )
		{
			Host.AssertClient();

			if ( this != Local.Pawn ) return;

			new ViewPunch.Vertical( Angle, Time );
		}
	}
}
