using Sandbox.UI;

/// <summary>
/// You don't need to put things in a namespace, but it doesn't hurt.
/// </summary>
namespace ElementGame
{
	public partial class ElementHUD : Sandbox.Hud
	{
		public ElementHUD()
		{
			if ( IsClient )
			{
				RootPanel.SetTemplate( "/UI/HUD.html" );
			}
		}
	}

}
