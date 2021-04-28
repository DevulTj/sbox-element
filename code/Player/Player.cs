using Sandbox;
using System;
using System.Linq;

namespace ElementGame
{
	partial class ElementPlayer : BasePlayer
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
			// Inventory.Add( new Shotgun(), true );

			base.Respawn();
		}

		/// <summary>
		/// Called every tick, clientside and serverside.
		/// </summary>
		protected override void Tick()
		{
			base.Tick();
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
	}
}
