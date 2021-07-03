using Sandbox;

namespace Element
{
	public static class GameEvent
	{
		public const string PlayerKilled = "playerKilled";

		public class PlayerKilledAttribute : EventAttribute
		{
			public PlayerKilledAttribute() : base(PlayerKilled) { }
		}
	}
}