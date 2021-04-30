
using Sandbox;
using Sandbox.UI;
using System.Threading.Tasks;

namespace ElementGame
{
	public abstract class FFARound : BaseRound
	{
		public override int Length => 180;
		public override string Name => "Free for all";

		public virtual float EndGameTime => 10f;

		TaskSource RoundEndTask;

		public override void Begin()
		{
			base.Begin();

			// Respawn all players
			Player.All.ForEach( Player => Player.Respawn() );

			ChatBox.AddInformation( "The game has begun." );
		}

		protected async Task HandleRoundEnd()
		{
			await RoundEndTask.DelaySeconds( EndGameTime );

			Begin();
		}

		public override void End()
		{
			base.End();

			ChatBox.AddInformation( $"Game over. Next round in {EndGameTime} seconds." );

			_ = HandleRoundEnd();
		}
	}
}