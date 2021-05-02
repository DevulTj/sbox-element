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
			if ( !IsClient )
				return;

			RootPanel.StyleSheet.Load( "/UI/HUD.scss" );

			RootPanel.AddChild<WeaponInfo>();
			RootPanel.AddChild<HeroInfo>();
			RootPanel.AddChild<VitalsInfo>();
			RootPanel.AddChild<GameStateInfo>();

			RootPanel.AddChild<ChatBox>();
			RootPanel.AddChild<Scoreboard<ScoreboardEntry>>();
			RootPanel.AddChild<KillFeed>();
			RootPanel.AddChild<VoiceList>();
		}
	}

}
