
using Sandbox;
using Sandbox.UI;
using System.Threading.Tasks;

namespace ElementGame
{
	public abstract partial class BasePickup : ModelEntity
	{
		public virtual float RefreshTime => 2f;
		public virtual string SoundToPlay => "";
		
		[Net]
		public Particles Effect { get; set; }

		bool IsRefreshing = false;
		TaskSource RefreshTask;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/props/pickuppad.vmdl" );
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

		public virtual void OnPickupActivated( ElementPlayer player )
		{
			IsRefreshing = true;

			Effect.Destroy( true );

			_ = HandleRefresh();
		}


		public virtual void Refresh()
		{
			IsRefreshing = false;

			Effect = Particles.Create( "particles/green_circle_teleporter.vpcf", this, "Base", true );

			Log.Error( "Refreshed." );
		}

		protected async Task HandleRefresh()
		{
			Log.Info( "refreshing..." );

			await RefreshTask.DelaySeconds( RefreshTime );

			Refresh();
		}

		public virtual bool CanUse( ElementPlayer player )
		{
			return !IsRefreshing;
		}

		public override void StartTouch( Entity other )
		{
			base.StartTouch( other );

			if ( IsClient )
			{
				return;
			}

			if ( other is ElementPlayer player )
			{
				if ( !CanUse( player ) )
				{
					return;
				}

				if ( SoundToPlay != "" )
					PlaySound( SoundToPlay );

				OnPickupActivated( player );
			}
		}
	}
}