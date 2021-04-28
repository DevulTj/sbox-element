
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace ElementGame
{
	public class WeaponInfo : Panel
	{
		public Panel TextContainer;
		public Label Weapon;
		public Label Inventory;

		public Panel Icon;

		public WeaponInfo()
		{
			StyleSheet.Load( "/UI/WeaponInfo.scss" );

			TextContainer = Add.Panel( "textContainer" );
			Weapon = TextContainer.Add.Label( "100", "weapon" );
			Inventory = TextContainer.Add.Label( "100", "inventory" );
			Icon = Add.Panel( "icon" );
		}

		public override void Tick()
		{
			var player = Sandbox.Player.Local;
			if ( player == null ) return;

			var weapon = player.ActiveChild as ElementWeapon;
			var isValid = weapon != null;

			SetClass( "active", isValid );
			SetClass( "low-ammo", weapon != null && weapon.AmmoClip < 3 );

			Weapon.Text = $"{weapon?.AmmoClip ?? 0}";

			if ( weapon != null && !weapon.UnlimitedAmmo )
			{
				var inv = weapon.AvailableAmmo();
				Inventory.Text = $"/ {inv}";
				Inventory.SetClass( "active", inv >= 0 );
			}
			else
			{
				Inventory.Text = $"/ ∞";
				Inventory.SetClass( "active", true );
			}
		}
	}
}