using Sandbox;
using System;
using System.Linq;
using Element;
using WalkController = Sandbox.WalkController;

partial class DmViewModel : BaseViewModel
{
	[Net, Predicted] public Element.Weapon.BaseWeapon Weapon {get; set; }

	float walkBob = 0;
	Vector3 velocity;
	Vector3 acceleration;
	float MouseScale => 1.5f;
	float ReturnForce => 400f;
	float Damping => 18f;
	float AccelDamping => 0.05f;
	float PivotForce => 1000f;
	float VelocityScale => 10f;
	float RotationScale => 2.5f;
	float LookUpPitchScale => 10f;
	float LookUpSpeedScale => 10f;
	float NoiseSpeed => 0.8f;
	float NoiseScale => 20f;

	Vector3 WalkCycleOffsets => new Vector3( 50f, 20f, 50f );
	float ForwardBobbing => 4f;
	float SideWalkOffset => 200f;
	public Vector3 AimOffset {get; set;} =  new Vector3( 10f, 16.9f, 2.8f );
	Vector3 Offset => new Vector3( -6f, 5f, -5f );
	Vector3 CrouchOffset => new Vector3( -10f, -50f, -0f );
	Vector3 SmoothedVelocity;
	float VelocityClamp => 3f;

	float noiseZ = 0;
	float noisePos = 0;
	float upDownOffset = 0;
	float avoidance = 0;

	float sprintLerp = 0;
	float aimLerp = 0;

	float smoothedDelta = 0;

	float DeltaTime => smoothedDelta;
	

	public DmViewModel()
	{
		noiseZ = Rand.Float( -10000, 10000 );
	}

	public override void PostCameraSetup( ref CameraSetup camSetup )
	{
		base.PostCameraSetup( ref camSetup );


		AddCameraEffects( ref camSetup );
	}

	private void SmoothDeltaTime () {
		var delta = (Time.Delta - smoothedDelta) * Time.Delta;
		var clamped = MathF.Min(MathF.Abs(delta), 1/60f);

		smoothedDelta += clamped * MathF.Sign(delta);
	}

	private void AddCameraEffects( ref CameraSetup camSetup )
	{
		SmoothDeltaTime();

		SmoothedVelocity += (Owner.Velocity - SmoothedVelocity) * 5f * DeltaTime;

		var camTransform = new Transform(Owner.EyePos, Owner.EyeRot);
		var speed = Owner.Velocity.Length.LerpInverse( 0, 1000 );
		var bobSpeed = SmoothedVelocity.Length.LerpInverse( -100, 500 );
		var left = camSetup.Rotation.Left;
		var up = camSetup.Rotation.Up;
		var forward = camSetup.Rotation.Forward;
		var owner = Owner as PlayerPawn;
		var walkController = owner.Controller as WalkController;
		var avoidanceTrace = Trace.Ray( camSetup.Position, camSetup.Position + forward * 50f )
						.UseHitboxes()
						.Ignore( Owner )
						.Ignore( this )
						.Run();

		var sprint = false;//walkController?.IsSprinting() ?? false;
		var aim = false;
		
		LerpTowards( ref avoidance, avoidanceTrace.Hit ? (1f - avoidanceTrace.Fraction) : 0, 10f );
		LerpTowards( ref sprintLerp,  sprint ? 1 : 0, 8f );
		LerpTowards( ref aimLerp, aim ? 1 : 0, 16f );
		LerpTowards( ref upDownOffset, speed * -LookUpSpeedScale + camSetup.Rotation.Forward.z * -LookUpPitchScale, LookUpPitchScale );

		FieldOfView = 80f * (1- aimLerp) + 40f * aimLerp;

		bobSpeed *= (1 - sprintLerp * 0.25f);

		if ( Owner.GroundEntity != null )
		{
			walkBob += Time.Delta * 30.0f * bobSpeed;
		}

		if ( Owner.Velocity.Length < 60 )
		{
			var step = MathF.Round( walkBob / 90 );

			walkBob += (step * 90 - walkBob) * 10f * Time.Delta;
		}

		if ( walkController?.Duck?.IsActive == true )
		{
			acceleration += CrouchOffset * DeltaTime * (1-aimLerp);
		}

		walkBob %= 360;

		noisePos += DeltaTime * NoiseSpeed;

		acceleration += Vector3.Left * -Input.MouseDelta.x * DeltaTime * MouseScale * 0.5f   * (1f-aimLerp * 2f);
		acceleration += Vector3.Up * -Input.MouseDelta.y * DeltaTime * MouseScale * (1f-aimLerp * 2f);
		acceleration += -velocity * ReturnForce * DeltaTime;

		// Apply horizontal offsets based on walking direction
		var horizontalForwardBob = WalkCycle( 0.5f, 3f ) * speed * WalkCycleOffsets.x * DeltaTime;

		acceleration += forward.WithZ( 0 ).Normal.Dot( Owner.Velocity.Normal ) * Vector3.Forward * ForwardBobbing * horizontalForwardBob;

		// Apply left bobbing and up/down bobbing
		acceleration += Vector3.Left * WalkCycle( 0.5f, 2f ) * speed * WalkCycleOffsets.y * (1 + sprintLerp) * DeltaTime;
		acceleration += Vector3.Up * WalkCycle( 0.5f, 2f, true ) * speed * WalkCycleOffsets.z * DeltaTime;

		acceleration += left.WithZ( 0 ).Normal.Dot( Owner.Velocity.Normal ) * Vector3.Left * speed * SideWalkOffset * DeltaTime * (1-aimLerp* 0.5f);

		velocity += acceleration * DeltaTime;

		ApplyDamping( ref acceleration, AccelDamping );

		ApplyDamping( ref velocity, Damping * (1 + aimLerp));

		acceleration += new Vector3(
			Noise.Perlin( noisePos, 0f, noiseZ ),
			Noise.Perlin( noisePos, 10f, noiseZ ),
			Noise.Perlin( noisePos, 20f, noiseZ )
		) * NoiseScale * Time.Delta * (1-aimLerp * 0.9f);

		velocity = velocity.Normal * Math.Clamp( velocity.Length, 0, VelocityClamp );

		Rotation desiredRotation = Local.Pawn.EyeRot;
		desiredRotation *= Rotation.FromAxis(Vector3.Up, velocity.y * RotationScale* (1-aimLerp));
		desiredRotation *= Rotation.FromAxis(Vector3.Forward, -velocity.y * RotationScale* (1-aimLerp * 0.0f) - 10f * (1-aimLerp));
		desiredRotation *= Rotation.FromAxis(Vector3.Right, velocity.z * RotationScale* (1-aimLerp));

		Rotation = desiredRotation; 

		var desiredOffset = Vector3.Lerp(Offset, AimOffset, aimLerp);

		Position += forward * (velocity.x * VelocityScale + desiredOffset.x);
		Position += left * (velocity.y * VelocityScale + desiredOffset.y);
		Position += up * (velocity.z * VelocityScale + desiredOffset.z + upDownOffset * (1-aimLerp));

		Position += (desiredRotation.Forward - camSetup.Rotation.Forward) * -PivotForce;

		// Apply sprinting / avoidance offsets
		var offsetLerp = MathF.Max(sprintLerp, avoidance);

		Rotation *= Rotation.FromAxis(Vector3.Up, velocity.y * (sprintLerp * 40f) + offsetLerp * 30f   * (1-aimLerp));

		Position += forward * avoidance;
		Position += left * (velocity.y * sprintLerp * -50f + offsetLerp * -10f   * (1-aimLerp));
		Position += up * (offsetLerp * -0f  + avoidance * -10   * (1-aimLerp));
	}

	private float WalkCycle( float speed, float power, bool abs = false )
	{
		var sin = MathF.Sin( walkBob * speed );
		var sign = Math.Sign( sin );

		if ( abs )
		{
			sign = 1;
		}

		return MathF.Pow( sin, power ) * sign;
	}

	public void ApplyImpulse( Vector3 impulse )
	{
		acceleration += impulse;
	}

	private void LerpTowards( ref float value, float desired, float speed )
	{
		var delta = (desired - value) * speed * DeltaTime;
		var deltaAbs = MathF.Min( MathF.Abs( delta ), MathF.Abs( desired - value ) ) * MathF.Sign( delta );

		if ( MathF.Abs( desired - value ) < 0.001f )
		{
			value = desired;

			return;
		}

		value += deltaAbs;
	}

	private void ApplyDamping( ref Vector3 value, float damping )
	{
		var magnitude = value.Length;

		if ( magnitude != 0 )
		{
			var drop = magnitude * damping * DeltaTime;
			value *= Math.Max( magnitude - drop, 0 ) / magnitude;
		}
	}
}
