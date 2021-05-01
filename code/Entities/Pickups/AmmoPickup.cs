
using Sandbox;
using Sandbox.UI;
using System.Threading.Tasks;

namespace ElementGame
{
	public partial class AmmoPickup : BasePickup
	{
		public override float RefreshTime => 15f;
		public override string SoundToPlay => "jumppad";

		public override void OnPickupActivated( ElementPlayer player )
		{
			base.OnPickupActivated( player );

			player.GiveAmmo( AmmoType.Rifle, 60 );
		}

		public override void Refresh()
		{
			base.Refresh();
		}
	}
}