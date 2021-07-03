
using Sandbox;
using System;
using System.Linq;
using System.Text;

namespace Element
{
	[Library]
	public class WallRun : NetworkComponent
	{
		public BasePlayerController Controller;

		public Vector3[] DirectionsOfTravel = new[]
		{
			Vector3.Right,
			Vector3.Right + Vector3.Forward,
			Vector3.Forward,
			Vector3.Left,
			Vector3.Left + Vector3.Forward
		};

		protected Vector3 LastWallPosition;
		protected Vector3 LastWallNormal;
		
		public bool IsWallRunning { get; protected set; } = false;

		protected TraceResult[] Hits = new TraceResult[5];
		protected TimeSince Activated = 0;

		public virtual float TimeUntilSlowDown => 1.5f;
		public virtual float SlowDownSpeed => 100f;
		
		public virtual float TimeUntilStopClimbingUp => 1.5f;
		public virtual float ClimbUpSpeed => 50f;

		// You can only slide once every X
		public virtual float Cooldown => 2f;

		public virtual float WallMaxDistance => 16f;
		public virtual float NormalizedAngleThreshold => 1f;
		public virtual float WallSpeedMultiplier => 400f;
		public virtual float JumpCooldown => 0.3f;
		
		public WallRun( BasePlayerController controller )
		{
			Controller = controller;
		}

		public void Reset()
		{
			IsWallRunning = false;
			
			Activated = Cooldown;
		}

		protected void Activate()
		{
			if ( IsWallRunning )
				return;

			IsWallRunning = true;
			Activated = 0;
		}

		// Only allow wall run if we are not on the ground
		public bool CanAttach()
		{
			return Controller.GroundEntity == null &&
			       Controller.Velocity.Length >= (WallSpeedMultiplier * 0.9f) && 
			       (Controller as WalkController)?.TimeSinceJumped > JumpCooldown;
		}

		public virtual void PreTick()
		{
			if ( !CanAttach() )
			{
				Reset();
				return;
			}

			Hits = new TraceResult[ DirectionsOfTravel.Length ];

			for ( int i = 0; i < DirectionsOfTravel.Length; i++ )
			{
				// Translate local direction to world space
				Vector3 direction = Controller.Rotation * DirectionsOfTravel[ i ];
				Vector3 origin = Controller.Position + Vector3.Up * 5f;
			
				var tr = Trace.Ray( origin, origin + direction * WallMaxDistance )
					.Ignore( Controller.Pawn )
					.Run();
				
				// Cache result
				Hits[ i ] = tr;
				
				if ( Hits[ i ].Entity != null )
				{
					DebugOverlay.Sphere( Hits[i].EndPos, 3, Color.Green );
					DebugOverlay.Line( Controller.Position, origin + direction * WallMaxDistance, Color.Green );
				}
				else
				{
					DebugOverlay.Line( Controller.Position, origin + direction * WallMaxDistance, Color.Red );
				}
			}
			
			Hits = Hits.ToList().Where( h => h.Entity != null ).OrderBy( h => h.Distance ).ToArray();
			
			if ( Hits.Length > 0 )
			{
				PerformWallRun( ref Hits[0] );
			}
			else
			{
				Reset();
			}
		}

		void PerformWallRun( ref TraceResult Hit )
		{
			float d = Vector3.Dot( Hit.Normal, Vector3.Up );
			if( d >= -NormalizedAngleThreshold && d <= NormalizedAngleThreshold )
			{
				// Vector3 alongWall = Vector3.Cross(hit.normal, Vector3.up);
				float vertical = 1;
				Vector3 alongWall = Controller.Rotation.Forward;

				DebugOverlay.Line( Controller.Position, Controller.Position + alongWall.Normal * 10f, Color.Green );
				DebugOverlay.Line( Controller.Position, LastWallNormal * 10, Color.Magenta );
				
				Controller.Velocity = alongWall * vertical * WallSpeedMultiplier;

				if ( Activated > TimeUntilSlowDown )
				{
					Controller.Velocity += Vector3.Down * SlowDownSpeed;
				}

				if ( Activated < TimeUntilStopClimbingUp )
				{
					float percent = Activated / TimeUntilStopClimbingUp;
					
					Controller.Velocity += Vector3.Up * ( ClimbUpSpeed * Easing.EaseOut( percent ) );
				}

				Activate();
			}
			else
			{
				Reset();
			}

			// Ensure this is at the end
			LastWallPosition = Hit.EndPos;
			LastWallNormal = Hit.Normal;
		}

		public Vector3 GetWallJumpDirection()
		{
			if ( IsWallRunning )
				return LastWallNormal * 50 + Vector3.Up;
			
			return Vector3.Zero;
		}
	}
}
