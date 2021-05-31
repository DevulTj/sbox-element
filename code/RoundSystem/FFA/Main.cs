using Sandbox;
using Sandbox.UI;
using System.Threading.Tasks;

namespace Element.FreeForAll
{
	public partial class MainRound : BaseRound
	{
		public override int Length => 20;
		public override string Name => "Free for all";
		public override string StartMessage => "The game has begun.";
		public override BaseRound GetNextRound() => new StatsRound();
	}
}
