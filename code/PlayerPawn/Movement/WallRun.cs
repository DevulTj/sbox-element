
using Sandbox;
using System;
using System.Linq;
using System.Text;

namespace Element
{
	[Library]
	public class WallRun : NetworkClass
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

		public bool IsActive; // replicate
		public bool Wish;

		public bool IsWallRunning { get; protected set; } = false;

		protected TraceResult[] Hits = new TraceResult[5];

		TimeSince Activated = 0;
		
		// You can only slide once every X
		public virtual float Cooldown => 2f;
		public virtual float MinimumSpeed => 64f;
		public virtual float WishDirectionFactor => 4f;

		public virtual float WallMaxDistance => 16f;
		public virtual float NormalizedAngleThreshold => 0.1f;
		public virtual float WallSpeedMultiplier => 300f;
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

		// Only allow wall run if we are not on the ground
		public bool CanAttach()
		{
			return Controller.GroundEntity == null && Controller.Input.Forward != 0 &&
			       Controller.Velocity.Length >= (WallSpeedMultiplier * 0.9f) && (Controller as WalkController)?.TimeSinceJumped > JumpCooldown;
		}

		public virtual void PreTick()
		{
			if ( !CanAttach() )
			{
				Reset();
				
				return;
			}

			// @TODO: Find out the state of the player to see if we want to wall run or not
			// Then control state based on what we're doing

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
				IsWallRunning = false;
			}
		}

		void PerformWallRun( ref TraceResult Hit )
		{
			float d = Vector3.Dot( Hit.Normal, Vector3.Up );
			if( d >= -NormalizedAngleThreshold && d <= NormalizedAngleThreshold )
			{
				// Vector3 alongWall = Vector3.Cross(hit.normal, Vector3.up);
				float vertical = Controller.Input.Forward;
				Vector3 alongWall = Controller.Rotation.Forward;

				DebugOverlay.Line( Controller.Position, Controller.Position + alongWall.Normal * 10f, Color.Green );
				DebugOverlay.Line( Controller.Position, LastWallNormal * 10, Color.Magenta );
				
				Controller.Velocity = alongWall * vertical * WallSpeedMultiplier;

				IsWallRunning = true;
			}
			else
			{
				IsWallRunning = false;
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
