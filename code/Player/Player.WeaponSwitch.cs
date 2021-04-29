using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ElementGame
{

	partial class ElementPlayer
	{
		protected Dictionary<InputButton, int> KeyMap = new()
		{
			{ InputButton.Slot0, 0 },
			{ InputButton.Slot1, 1 },
			{ InputButton.Slot2, 2 },
			{ InputButton.Slot3, 3 }
		};

		protected int GetSlotIndex( InputButton bind )
        {
			if ( KeyMap.TryGetValue( bind, out var val ) )
            {
				return val;
            }

			return 0;
		}

		protected void ChangeWeapon( InputButton bind )
        {
			int index = GetSlotIndex( bind );
			if ( index < 1 )
				return;

			Inventory.SetActiveSlot( index - 1, true );
        }

		protected void WeaponSwitchTick()
        {
			if ( Input.Pressed( InputButton.Slot0 ) )
				ChangeWeapon( InputButton.Slot0 );
			else if ( Input.Pressed( InputButton.Slot1 ) )
				ChangeWeapon( InputButton.Slot1 );
			else if ( Input.Pressed( InputButton.Slot2 ) )
				ChangeWeapon( InputButton.Slot2 );
			else if ( Input.Pressed( InputButton.Slot3 ) )
				ChangeWeapon( InputButton.Slot3 );
		}
	}
}