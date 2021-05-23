using Sandbox;

namespace Element.Weapon
{
	[Library( "crossbow_bolt" )]
	partial class CrossbowBolt : ModelEntity, IPhysicsUpdate
	{
		bool Stuck;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "weapons/rust_crossbow/rust_crossbow_bolt.vmdl" );
		}


		public virtual void OnPostPhysicsStep( float dt )
		{
			//DebugOverlay.Box( 0.1f, Position, -0.1f, 1.1f, Host.Color );

			if ( !IsServer )
				return;

			if ( Stuck )
				return;

			float Speed = 100.0f;
			var velocity = Rotation.Forward * Speed;

			var start = Position;
			var end = start + velocity;

			var tr = Trace.Ray( start, end )
				.UseHitboxes()
				//.HitLayer( CollisionLayer.Water, !InWater )
				.Ignore( Owner )
				.Ignore( this )
				.Size( 1.0f )
				.Run();

			// DebugOverlay.Line( start, end, 10.0f );
			// DebugOverlay.Box( 10.0f, Position, -1, 1, Color.Red );
			// DebugOverlay.Box( 10.0f, tr.EndPos, -1, 1, Color.Red );

			if ( tr.Hit )
			{
				// TODO: CLINK NOISE (unless flesh)

				// TODO: SPARKY PARTICLES (unless flesh)

				Stuck = true;
				Position = tr.EndPos + Rotation.Forward * -1;

				if ( tr.Entity.IsValid() )
				{
					var damageInfo = DamageInfo.FromBullet( tr.EndPos, tr.Direction * 200, 100.0f )
						.UsingTraceResult( tr )
						.WithAttacker( Owner )
						.WithWeapon( this );

					tr.Entity.TakeDamage( damageInfo );
				}

				// TODO: Parent to bone so this will stick in the meaty heads
				SetParent( tr.Entity, tr.Bone );
				Owner = null;

				//
				// Surface impact effect
				//
				tr.Normal = Rotation.Forward * -1;
				tr.Surface.DoBulletImpact( tr );
				velocity = default;

				//
				// BUG: without this the bolt stops short of the wall on the client.
				//		need to make interp finish itself off even though it's not getting new positions?
				//
				ResetInterpolation();

				// DebugOverlay.Box( 10.0f, Position, -1, 1, Color.Red );
				// DebugOverlay.Box( 10.0f, tr.EndPos, -1, 1, Color.Yellow );

				// delete self in 30 seconds
				_ = DeleteAsync( 30.0f );
			}
			else
			{
				Position = end;
			}


		}
	}
}
