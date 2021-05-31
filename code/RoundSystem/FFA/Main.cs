using Sandbox;
using Sandbox.UI;
using System.Threading.Tasks;

namespace Element.FreeForAll
{
	public partial class MainRound : BaseRound
	{
		[ServerVar( "element_ffa_length", Help = "Round length for Free For All in Element" )]
		public static int RoundLength { get; set; } = 60;

		public override int Length => RoundLength;
		public override string Name => "Free for all";
		public override string StartMessage => "The game has begun.";
		public override BaseRound GetNextRound() => new StatsRound();
	}
}
