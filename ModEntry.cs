using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace TeleportMod
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += OnButtonPressed;
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            // Require a loaded save, player must be able to move 
            if (!Context.IsWorldReady)      return;
            if (!Context.CanPlayerMove)     return;
            // Don't open on top of another menu
            if (Game1.activeClickableMenu != null) return;

            if (e.Button == SButton.L)
            {
                // Suppress so the key doesn't also trigger anything else
                this.Helper.Input.Suppress(e.Button);
                Game1.activeClickableMenu = new TeleportMenu(this.Monitor);
            }
        }
    }
}
