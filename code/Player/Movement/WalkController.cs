using Sandbox;
using System;
using System.Collections.Generic;

namespace ElementGame
{
	public struct Impulse
	{
		public Vector3 Direction;
		public bool LiftPlayer;

		public Impulse( Vector3 impulse, bool shouldLiftPlayer ) : this()
		{
			Direction = impulse;
			LiftPlayer = shouldLiftPlayer;
		}
	}

	public struct AdditionalJump
	{
		public float DirectionalPower;
		public bool ResetVelocity;
		public float ModifiedJumpPower;

		public AdditionalJump( float directionalPower, bool resetVelocity, float modifiedJumpPower ) : this()
		{
			DirectionalPower = directionalPower;
			ResetVelocity = resetVelocity;
			ModifiedJumpPower = modifiedJumpPower;
		}
	}

	[Library]
	public class WalkController : BasePlayerController
	{
		public float SprintSpeed { get; set; } = 320.0f;
		public float WalkSpeed { get; set; } = 150.0f;
		public float DefaultSpeed { get; set; } = 190.0f;
		public float Acceleration { get; set; } = 10.0f;
		public float AirAcceleration { get; set; } = 50.0f;
		public float FallSoundZ { get; set; } = -30.0f;
		public float GroundFriction { get; set; } = 4.0f;
		public float StopSpeed { get; set; } = 100.0f;
		public float Size { get; set; } = 20.0f;
		public float DistEpsilon { get; set; } = 0.03125f;
		public float GroundNormalZ { get; set; } = 0.707f;
		public float Bounce { get; set; } = 0.0f;
		public float MoveFriction { get; set; } = 1.0f;
		public float StepSize { get; set; } = 18.0f;
		public float MaxNonJumpVelocity { get; set; } = 140.0f;
		public float BodyGirth { get; set; } = 32.0f;
		public float BodyHeight { get; set; } = 72.0f;
		public float EyeHeight { get; set; } = 64.0f;
		public float Gravity { get; set; } = 800.0f;
		public float AirControl { get; set; } = 30.0f;
		public bool Swimming { get; set; } = false;
		public bool AutoJump { get; set; } = true;

		public float JumpPower { get; set; } = 256f;

		public Duck Duck;
		public Unstuck Unstuck;

		public int AllowedJumps = 0;
		List<AdditionalJump> AllowedJumpsInfo = new();

		public WalkController()
		{
			Duck = new Duck( this );
			Unstuck = new Unstuck( this );
		}

		internal void ExtraJump( bool resetVelocity, float directionalPower = 0f, float modifiedJumpPower = 0f )
		{
			if ( modifiedJumpPower == 0f )
				modifiedJumpPower = JumpPower;

			AllowedJumpsInfo.Add( new( directionalPower, resetVelocity, modifiedJumpPower ) );
			AllowedJumps++;
		}

		/// <summary>
		/// This is temporary, get the hull size for the player's collision
		/// </summary>
		public override BBox GetHull()
		{
			var girth = BodyGirth * 0.5f;
			var mins = new Vector3( -girth, -girth, 0 );
			var maxs = new Vector3( +girth, +girth, BodyHeight );

			return new BBox( mins, maxs );
		}

		List<Impulse> ImpulseList = new();

		public void QueueImpulse( Vector3 impulse, bool shouldLiftPlayer = false )
			=> ImpulseList.Add( new( impulse, shouldLiftPlayer ) );

		public void QueueImpulseAdditive( Vector3 impulse, bool shouldLiftPlayer = false )
			=> ImpulseList.Add( new( Velocity + impulse, shouldLiftPlayer ) );

		// Duck body height 32
		// Eye Height 64
		// Duck Eye Height 28

		protected Vector3 mins;
		protected Vector3 maxs;

		public virtual void SetBBox( Vector3 mins, Vector3 maxs )
		{
			if ( this.mins == mins && this.maxs == maxs )
				return;

			this.mins = mins;
			this.maxs = maxs;
		}

		/// <summary>
		/// Update the size of the bbox. We should really trigger some shit if this changes.
		/// </summary>
		public virtual void UpdateBBox()
		{
			var girth = BodyGirth * 0.5f;

			var mins = new Vector3( -girth, -girth, 0 ) * Player.WorldScale;
			var maxs = new Vector3( +girth, +girth, BodyHeight ) * Player.WorldScale;

			Duck.UpdateBBox( ref mins, ref maxs );

			SetBBox( mins, maxs );
		}

		protected float SurfaceFriction;

		public override void Tick()
		{
			ViewOffset = Vector3.Up * ( EyeHeight * Player.WorldScale );
			UpdateBBox();

			ViewOffset += TraceOffset;

			RestoreGroundPos();

			//Velocity += BaseVelocity * ( 1 + Time.Delta * 0.5f );
			//BaseVelocity = Vector3.Zero;

			//  Rot = Rotation.LookAt( Input.Rot.Forward.WithZ( 0 ), Vector3.Up );

			if ( Unstuck.TestAndFix() )
				return;

			// Check Stuck
			// Unstuck - or return if stuck

			// Set Ground Entity to null if  falling faster then 250

			// store water level to compare later

			// if not on ground, store fall velocity

			// player->UpdateStepSound( player->m_pSurfaceData, mv->GetAbsOrigin(), mv->m_vecVelocity )


			// RunLadderMode

			CheckLadder();
			Swimming = Player.WaterLevel.Fraction > 0.6f;

			//
			// Start Gravity
			//
			if ( !Swimming && !IsTouchingLadder )
			{
				Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
				Velocity += new Vector3( 0, 0, BaseVelocity.z ) * Time.Delta;

				BaseVelocity = BaseVelocity.WithZ( 0 );
			}


			/*
				if (player->m_flWaterJumpTime)
				{
					WaterJump();
					TryPlayerMove();
					// See if we are still in water?
					CheckWater();
					return;
				}
			*/

			// if ( underwater ) do underwater movement

			if ( AutoJump ? Input.Down( InputButton.Jump ) : Input.Pressed( InputButton.Jump ) )
			{
				CheckJumpButton();
			}

			if ( ImpulseList.Count > 0 )
			{
				Velocity = Vector3.Zero;

				bool hasLifted = false;
				ImpulseList.ForEach( x =>
				{
					if ( x.LiftPlayer && !hasLifted )
					{
						hasLifted = true;

						ClearGroundEntity();
						AddEvent( "jump" );
					}

					Velocity += x.Direction;
				} );

				// Clear the list after we've done processing
				ImpulseList.Clear();
			}

			// Fricion is handled before we add in any base velocity. That way, if we are on a conveyor, 
			//  we don't slow when standing still, relative to the conveyor.
			bool bStartOnGround = GroundEntity != null;
			//bool bDropSound = false;
			if ( bStartOnGround )
			{
				//if ( Velocity.z < FallSoundZ ) bDropSound = true;

				Velocity = Velocity.WithZ( 0 );
				//player->m_Local.m_flFallVelocity = 0.0f;

				if ( GroundEntity != null )
				{
					ApplyFriction( GroundFriction * SurfaceFriction );
				}
			}

			//
			// Work out wish velocity.. just take input, rotate it to view, clamp to -1, 1
			//
			WishVelocity = new Vector3( Input.Forward, Input.Left, 0 );
			var inSpeed = WishVelocity.Length.Clamp( 0, 1 );
			WishVelocity *= Input.Rot;

			if ( !Swimming && !IsTouchingLadder )
			{
				WishVelocity = WishVelocity.WithZ( 0 );
			}

			WishVelocity = WishVelocity.Normal * inSpeed;
			WishVelocity *= GetWishSpeed();

			Duck.PreTick();

			bool bStayOnGround = false;
			if ( Swimming )
			{
				ApplyFriction( 1 );
				WaterMove();
			}
			else if ( IsTouchingLadder )
			{
				LadderMove();
			}
			else if ( GroundEntity != null )
			{
				bStayOnGround = true;
				WalkMove();
			}
			else
			{
				AirMove();
			}

			CategorizePosition( bStayOnGround );

			// FinishGravity
			if ( !Swimming && !IsTouchingLadder )
			{
				Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
			}


			if ( GroundEntity != null )
			{
				Velocity = Velocity.WithZ( 0 );
			}

			// CheckFalling(); // fall damage etc

			// Land Sound
			// Swim Sounds

			SaveGroundPos();

			if ( Debug )
			{
				DebugOverlay.Box( Pos + TraceOffset, mins, maxs, Color.Red );
				DebugOverlay.Box( Pos, mins, maxs, Color.Blue );

				var lineOffset = 0;
				if ( Host.IsServer ) lineOffset = 10;

				DebugOverlay.ScreenText( lineOffset + 0, $"             Pos: {Pos}" );
				DebugOverlay.ScreenText( lineOffset + 1, $"             Vel: {Velocity}" );
				DebugOverlay.ScreenText( lineOffset + 2, $"    BaseVelocity: {BaseVelocity}" );
				DebugOverlay.ScreenText( lineOffset + 3, $"    GroundEntity: {GroundEntity} [{GroundEntity?.Velocity}]" );
				DebugOverlay.ScreenText( lineOffset + 4, $" SurfaceFriction: {SurfaceFriction}" );
				DebugOverlay.ScreenText( lineOffset + 5, $"    WishVelocity: {WishVelocity}" );
			}

		}

		public virtual float GetWishSpeed()
		{
			var ws = Duck.GetWishSpeed();
			if ( ws >= 0 ) return ws;

			if ( Input.Down( InputButton.Run ) ) return SprintSpeed;
			if ( Input.Down( InputButton.Walk ) ) return WalkSpeed;

			return DefaultSpeed;
		}

		void WalkMove()
		{
			var wishdir = WishVelocity.Normal;
			var wishspeed = WishVelocity.Length;

			WishVelocity = WishVelocity.WithZ( 0 );
			WishVelocity = WishVelocity.Normal * wishspeed;

			Velocity = Velocity.WithZ( 0 );
			Accelerate( wishdir, wishspeed, 0, Acceleration );
			Velocity = Velocity.WithZ( 0 );

			//   Player.SetAnimParam( "forward", Input.Forward );
			//   Player.SetAnimParam( "sideward", Input.Right );
			//   Player.SetAnimParam( "wishspeed", wishspeed );
			//    Player.SetAnimParam( "walkspeed_scale", 2.0f / 190.0f );
			//   Player.SetAnimParam( "runspeed_scale", 2.0f / 320.0f );

			//  DebugOverlay.Text( 0, Pos + Vector3.Up * 100, $"forward: {Input.Forward}\nsideward: {Input.Right}" );

			// Add in any base velocity to the current velocity.
			Velocity += BaseVelocity;

			try
			{
				if ( Velocity.Length < 1.0f )
				{
					Velocity = Vector3.Zero;
					return;
				}

				// first try just moving to the destination	
				var dest = ( Pos + Velocity * Time.Delta ).WithZ( Pos.z );

				var pm = TraceBBox( Pos, dest );

				if ( pm.Fraction == 1 )
				{
					Pos = pm.EndPos;
					StayOnGround();
					return;
				}

				StepMove();
			}
			finally
			{

				// Now pull the base velocity back out.   Base velocity is set if you are on a moving object, like a conveyor (or maybe another monster?)
				Velocity -= BaseVelocity;
			}

			StayOnGround();
		}

		private void StepMove()
		{
			var vecPos = Pos;
			var vecVel = Velocity;

			//
			// First try walking straight to where they want to go.
			//
			TryPlayerMove();

			//
			// mv now contains where they ended up if they tried to walk straight there.
			// Save those results for use later.
			//	
			var vecDownPos = Pos;
			var vecDownVel = Velocity;

			//
			// Reset original values to try some other things.
			//
			Pos = vecPos;
			Velocity = vecVel;

			// Only step up as high as we have headroom to do so.	
			var trace = TraceBBox( Pos, Pos + Vector3.Up * ( StepSize + DistEpsilon ) );
			if ( !trace.StartedSolid )
			{
				Pos = trace.EndPos;
			}
			TryPlayerMove();


			trace = TraceBBox( Pos, Pos + Vector3.Down * ( StepSize + DistEpsilon * 2 ) );
			if ( trace.Normal.z < GroundNormalZ )
			{
				if ( Debug )
				{
					DebugOverlay.Text( vecDownPos, "step down", 2.0f );
				}

				Pos = vecDownPos;
				Velocity = vecDownVel.WithZ( 0 );
				// out step height
				return;
			}

			if ( !trace.StartedSolid )
			{
				Pos = trace.EndPos;
			}

			var vecUpPos = Pos;

			float flDownDist = ( vecDownPos.x - vecPos.x ) * ( vecDownPos.x - vecPos.x ) + ( vecDownPos.y - vecPos.y ) * ( vecDownPos.y - vecPos.y );
			float flUpDist = ( vecUpPos.x - vecPos.x ) * ( vecUpPos.x - vecPos.x ) + ( vecUpPos.y - vecPos.y ) * ( vecUpPos.y - vecPos.y );
			if ( flDownDist > flUpDist )
			{
				Pos = vecDownPos;
				Velocity = vecDownVel;
			}
			else
			{
				// copy z value from slide move
				Velocity = Velocity.WithZ( vecDownVel.z );
			}

			// out step height

		}

		/// <summary>
		/// Add our wish direction and speed onto our velocity
		/// </summary>
		public virtual void Accelerate( Vector3 wishdir, float wishspeed, float speedLimit, float acceleration )
		{
			// This gets overridden because some games (CSPort) want to allow dead (observer) players
			// to be able to move around.
			// if ( !CanAccelerate() )
			//     return;

			if ( speedLimit > 0 && wishspeed > speedLimit )
				wishspeed = speedLimit;

			// See if we are changing direction a bit
			var currentspeed = Velocity.Dot( wishdir );

			// Reduce wishspeed by the amount of veer.
			var addspeed = wishspeed - currentspeed;

			// If not going to add any speed, done.
			if ( addspeed <= 0 )
				return;

			// Determine amount of acceleration.
			var accelspeed = acceleration * Time.Delta * wishspeed * SurfaceFriction;

			// Cap at addspeed
			if ( accelspeed > addspeed )
				accelspeed = addspeed;

			Velocity += wishdir * accelspeed;
		}

		/// <summary>
		/// Remove ground friction from velocity
		/// </summary>
		public virtual void ApplyFriction( float frictionAmount = 1.0f )
		{
			// If we are in water jump cycle, don't apply friction
			//if ( player->m_flWaterJumpTime )
			//   return;

			// Not on ground - no friction


			// Calculate speed
			var speed = Velocity.Length;
			if ( speed < 0.1f ) return;

			// Bleed off some speed, but if we have less than the bleed
			//  threshold, bleed the threshold amount.
			float control = ( speed < StopSpeed ) ? StopSpeed : speed;

			// Add the amount to the drop amount.
			var drop = control * Time.Delta * frictionAmount;

			// scale the velocity
			float newspeed = speed - drop;
			if ( newspeed < 0 ) newspeed = 0;

			if ( newspeed != speed )
			{
				newspeed /= speed;
				Velocity *= newspeed;
			}

			// mv->m_outWishVel -= (1.f-newspeed) * mv->m_vecVelocity;
		}

		void CheckJumpButton()
		{
			//if ( !player->CanJump() )
			//    return false;


			/*
			if ( player->m_flWaterJumpTime )
			{
				player->m_flWaterJumpTime -= gpGlobals->frametime();
				if ( player->m_flWaterJumpTime < 0 )
					player->m_flWaterJumpTime = 0;

				return false;
			}*/



			// If we are in the water most of the way...
			if ( Swimming )
			{
				// swimming, not jumping
				ClearGroundEntity();

				Velocity = Velocity.WithZ( 100 );

				// play swimming sound
				//  if ( player->m_flSwimSoundTime <= 0 )
				{
					// Don't play sound again for 1 second
					//   player->m_flSwimSoundTime = 1000;
					//   PlaySwimSound();
				}

				return;
			}

			if ( GroundEntity == null && AllowedJumps < 1 )
				return;

			// This code makes me wanna kms
			var resetVelocity = false;
			var jumpDirectionalPower = 0f;
			var usedJumpPower = JumpPower;
			if ( AllowedJumps > 0 )
			{
				var index = AllowedJumps - 1;
				var jumpInfo = AllowedJumpsInfo[ index ];

				resetVelocity = jumpInfo.ResetVelocity;
				jumpDirectionalPower = jumpInfo.DirectionalPower;

				if ( jumpInfo.ModifiedJumpPower > 0f )
				{
					usedJumpPower = jumpInfo.ModifiedJumpPower;
				}

				AllowedJumpsInfo.RemoveAt( index );
				AllowedJumps--;
			}

			/*
			if ( player->m_Local.m_bDucking && (player->GetFlags() & FL_DUCKING) )
				return false;
			*/

			/*
			// Still updating the eye position.
			if ( player->m_Local.m_nDuckJumpTimeMsecs > 0u )
				return false;
			*/

			ClearGroundEntity();


			Velocity = Velocity.WithZ( 0f );
			Velocity = resetVelocity ? new Vector3( 0, 0, usedJumpPower ) + ( WishVelocity.Normal * jumpDirectionalPower ) : Velocity.WithZ( usedJumpPower );
			Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

			// mv->m_outJumpVel.z += mv->m_vecVelocity[2] - startz;
			// mv->m_outStepHeight += 0.15f;

			// don't jump again until released
			//mv->m_nOldButtons |= IN_JUMP;

			AddEvent( "jump" );

		}

		void AirMove()
		{
			var wishdir = WishVelocity.Normal;
			var wishspeed = WishVelocity.Length;

			Accelerate( wishdir, wishspeed, AirControl, AirAcceleration );

			Velocity += BaseVelocity;

			TryPlayerMove();

			Velocity -= BaseVelocity;
		}

		void WaterMove()
		{
			var wishdir = WishVelocity.Normal;
			var wishspeed = WishVelocity.Length;

			wishspeed *= 0.8f;

			Accelerate( wishdir, wishspeed, 100, Acceleration );

			Velocity += BaseVelocity;

			TryPlayerMove();

			Velocity -= BaseVelocity;
		}

		bool IsTouchingLadder = false;
		Vector3 LadderNormal;

		void CheckLadder()
		{
			if ( IsTouchingLadder && Input.Pressed( InputButton.Jump ) )
			{
				Velocity = LadderNormal * 100.0f;
				IsTouchingLadder = false;

				return;
			}

			const float ladderDistance = 1.0f;
			var start = Pos;
			Vector3 end = start + ( IsTouchingLadder ? ( LadderNormal * -1.0f ) : WishVelocity.Normal ) * ladderDistance;

			var pm = Trace.Ray( start, end )
						.Size( mins, maxs )
						.HitLayer( CollisionLayer.All, false )
						.HitLayer( CollisionLayer.LADDER, true )
						.Ignore( Player )
						.Run();

			IsTouchingLadder = false;

			if ( pm.Hit )
			{
				IsTouchingLadder = true;
				LadderNormal = pm.Normal;
			}
		}

		void LadderMove()
		{
			var velocity = WishVelocity;
			float normalDot = velocity.Dot( LadderNormal );
			var cross = LadderNormal * normalDot;
			Velocity = ( velocity - cross ) + ( -normalDot * LadderNormal.Cross( Vector3.Up.Cross( LadderNormal ).Normal ) );

			TryPlayerMove();
		}

		Vector3[] planes = new Vector3[ 5 ];

		void TryPlayerMove()
		{
			var timeLeft = Time.Delta;
			var allFraction = 0.0f;

			var original_velocity = Velocity;
			var primal_velocity = Velocity;

			var numplanes = 0;

			for ( int bumpCount = 0; bumpCount < 4; bumpCount++ )
			{
				if ( Velocity.Length == 0.0f )
					break;

				// Assume we can move all the way from the current origin to the
				//  end point.
				var end = Pos + Velocity * timeLeft;

				var pm = TraceBBox( Pos, end );

				allFraction += pm.Fraction;

				// actually covered some distance
				if ( pm.Fraction > DistEpsilon )
				{
					// Note: Skipping terrain double check around hack test

					Pos = pm.EndPos;
					original_velocity = Velocity;
					numplanes = 0;
				}

				// If we covered the entire distance, we are done
				//  and can return.
				if ( pm.Fraction == 1 )
				{
					break;      // moved the entire distance
				}

				// MoveHelper( )->AddToTouched( pm, mv->m_vecVelocity );

				bool probablyFloor = pm.Normal.z > GroundNormalZ;

				// If the plane we hit has a high z component in the normal, then
				//  it's probably a floor
				if ( probablyFloor )
				{

				}

				// If the plane has a zero z component in the normal, then it's a 
				//  step or wall
				if ( pm.Normal.z == 0 )
				{

				}

				timeLeft -= timeLeft * pm.Fraction;

				if ( numplanes >= planes.Length )
				{
					Velocity = Vector3.Zero;
					break;
				}

				planes[ numplanes ] = pm.Normal;
				numplanes++;

				if ( numplanes == 1 && GroundEntity == null )
				{
					var overbounce = 1.001f;
					if ( !probablyFloor ) overbounce = 1.0f + Bounce * ( 1.0f - SurfaceFriction );

					original_velocity = ClipVelocity( original_velocity, planes[ 0 ], overbounce );
					Velocity = original_velocity;
				}
				else
				{
					int i = 0;
					for ( i = 0; i < numplanes; i++ )
					{
						Velocity = ClipVelocity( original_velocity, planes[ i ], 1.001f );

						int j = 0;
						for ( j = 0; j < numplanes; j++ )
						{
							if ( j == i ) continue;

							// Are we now moving against this plane?
							if ( Velocity.Dot( planes[ j ] ) < 0 )
								break; // not ok
						}

						if ( j == numplanes )  // Didn't have to clip, so we're ok
							break;
					}

					// Did we go all the way through plane set
					if ( i == numplanes )
					{
						if ( numplanes != 2 )
						{
							Velocity = Vector3.Zero;
							break;
						}

						var dir = Vector3.Cross( planes[ 0 ], planes[ 1 ] ).Normal;
						var d = dir.Dot( Velocity );
						Velocity = dir * d;
					}

					if ( Vector3.Dot( Velocity, primal_velocity ) <= 0 )
					{
						Velocity = Vector3.Zero;
						break;
					}

				}

			}

			if ( allFraction == 0 )
			{
				Velocity = Vector3.Zero;
			}

			// Slam Volumes
		}

		Vector3 ClipVelocity( Vector3 vel, Vector3 norm, float overbounce )
		{
			var backoff = Vector3.Dot( vel, norm ) * overbounce;

			var change = norm * backoff;
			var o = vel - change;

			var adjust = Vector3.Dot( o, norm );
			if ( adjust < 1.0f )
			{
				adjust = MathF.Min( adjust, -1.0f );
				o -= norm * adjust;
			}

			return o;
		}

		void CategorizePosition( bool bStayOnGround )
		{
			SurfaceFriction = 1.0f;

			// Doing this before we move may introduce a potential latency in water detection, but
			// doing it after can get us stuck on the bottom in water if the amount we move up
			// is less than the 1 pixel 'threshold' we're about to snap to.	Also, we'll call
			// this several times per frame, so we really need to avoid sticking to the bottom of
			// water on each call, and the converse case will correct itself if called twice.
			//CheckWater();

			var point = Pos - Vector3.Up * 2;
			var vBumpOrigin = Pos;

			//
			//  Shooting up really fast.  Definitely not on ground trimed until ladder shit
			//
			bool bMovingUpRapidly = Velocity.z > MaxNonJumpVelocity;
			bool bMovingUp = Velocity.z > 0;

			bool bMoveToEndPos = false;

			if ( GroundEntity != null ) // and not underwater
			{
				bMoveToEndPos = true;
				point.z -= StepSize;
			}
			else if ( bStayOnGround )
			{
				bMoveToEndPos = true;
				point.z -= StepSize;
			}

			if ( bMovingUpRapidly || Swimming ) // or ladder and moving up
			{
				ClearGroundEntity();
				return;
			}

			var pm = TraceBBox( vBumpOrigin, point, 4.0f );

			if ( pm.Entity == null || pm.Normal.z < GroundNormalZ )
			{
				ClearGroundEntity();
				bMoveToEndPos = false;

				if ( Velocity.z > 0 )
					SurfaceFriction = 0.25f;
			}
			else
			{
				UpdateGroundEntity( pm );
			}

			if ( bMoveToEndPos && !pm.StartedSolid && pm.Fraction > 0.0f && pm.Fraction < 1.0f )
			{
				Pos = pm.EndPos;
			}

		}

		/// <summary>
		/// We have a new ground entity
		/// </summary>
		public virtual void UpdateGroundEntity( TraceResult tr )
		{
			GroundNormal = tr.Normal;

			// VALVE HACKHACK: Scale this to fudge the relationship between vphysics friction values and player friction values.
			// A value of 0.8f feels pretty normal for vphysics, whereas 1.0f is normal for players.
			// This scaling trivially makes them equivalent.  REVISIT if this affects low friction surfaces too much.
			SurfaceFriction = tr.Surface.Friction * 1.25f;
			if ( SurfaceFriction > 1 ) SurfaceFriction = 1;

			//if ( tr.Entity == GroundEntity ) return;

			Vector3 oldGroundVelocity = default;
			if ( GroundEntity != null ) oldGroundVelocity = GroundEntity.Velocity;

			bool wasOffGround = GroundEntity == null;

			GroundEntity = tr.Entity;

			if ( GroundEntity != null )
			{
				BaseVelocity = GroundEntity.Velocity;
			}

			/*
				m_vecGroundUp = pm.m_vHitNormal;
				player->m_surfaceProps = pm.m_pSurfaceProperties->GetNameHash();
				player->m_pSurfaceData = pm.m_pSurfaceProperties;
				const CPhysSurfaceProperties *pProp = pm.m_pSurfaceProperties;

				const CGameSurfaceProperties *pGameProps = g_pPhysicsQuery->GetGameSurfaceproperties( pProp );
				player->m_chTextureType = (int8)pGameProps->m_nLegacyGameMaterial;
			*/
		}

		/// <summary>
		/// We're no longer on the ground, remove it
		/// </summary>
		public virtual void ClearGroundEntity()
		{
			if ( GroundEntity == null ) return;

			GroundEntity = null;
			GroundNormal = Vector3.Up;
			SurfaceFriction = 1.0f;
		}

		/// <summary>
		/// Traces the current bbox and returns the result.
		/// liftFeet will move the start position up by this amount, while keeping the top of the bbox at the same 
		/// position. This is good when tracing down because you won't be tracing through the ceiling above.
		/// </summary>
		public override TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f )
		{
			return TraceBBox( start, end, mins, maxs, liftFeet );
		}

		/// <summary>
		/// Try to keep a walking player on the ground when running down slopes etc
		/// </summary>
		public virtual void StayOnGround()
		{
			var start = Pos + Vector3.Up * 2;
			var end = Pos + Vector3.Down * StepSize;

			// See how far up we can go without getting stuck
			var trace = TraceBBox( Pos, start );
			start = trace.EndPos;

			// Now trace down from a known safe position
			trace = TraceBBox( start, end );

			if ( trace.Fraction <= 0 ) return;
			if ( trace.Fraction >= 1 ) return;
			if ( trace.StartedSolid ) return;
			if ( trace.Normal.z > GroundNormalZ ) return;

			// This is incredibly hacky. The real problem is that trace returning that strange value we can't network over.
			// float flDelta = fabs( mv->GetAbsOrigin().z - trace.m_vEndPos.z );
			// if ( flDelta > 0.5f * DIST_EPSILON )

			Pos = trace.EndPos;
		}

		void RestoreGroundPos()
		{
			if ( GroundEntity == null || GroundEntity.IsWorld )
				return;

			//var worldPos = GroundEntity.Transform.ToWorld( GroundTransform );
			//Pos = worldPos.Pos;
		}

		void SaveGroundPos()
		{
			if ( GroundEntity == null || GroundEntity.IsWorld )
				return;

			//GroundTransform = GroundEntity.Transform.ToLocal( new Transform( Pos, Rot ) );
		}

	}
}