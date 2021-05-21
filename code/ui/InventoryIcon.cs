
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Element.UI
{
	class InventoryIcon : Panel
	{
		public Weapon.BaseWeapon Weapon;
		public Panel Icon;

		public InventoryIcon( Weapon.BaseWeapon weapon )
		{
			Weapon = weapon;
			Icon = Add.Panel( "icon" );
			AddClass( weapon.ClassInfo.Name );
		}

		internal void TickSelection( Weapon.BaseWeapon selectedWeapon )
		{
			SetClass( "active", selectedWeapon == Weapon );
			SetClass( "empty", !Weapon?.IsUsable() ?? true );
		}

		public override void Tick()
		{
			base.Tick();

			if ( !Weapon.IsValid() || Weapon.Owner != Local.Pawn )
				Delete();
		}
	}

}
