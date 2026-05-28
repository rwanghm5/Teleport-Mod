using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace TeleportMod
{
    public class ModEntry : Mod
    {
            // Key used to store the waypoint in this save file's data.
            private const string WaypointDataKey = "waypoint";

            // The currently saved waypoint, or null if none. Read by TeleportMenu.
            public (string Map, int X, int Y)? Waypoint { get; private set; }
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        }
        // Load the saved waypoint (if any) when a save file is opened.
        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            var data = this.Helper.Data.ReadSaveData<WaypointData>(WaypointDataKey);
            if (data != null)
                Waypoint = (data.Map, data.X, data.Y);
            else
                Waypoint = null;
        }

        // Called by TeleportMenu when the player saves their current position.
        public void SaveWaypoint(string map, int x, int y)
        {
            Waypoint = (map, x, y);
            this.Helper.Data.WriteSaveData(WaypointDataKey, new WaypointData
            {
                Map = map,
                X   = x,
                Y   = y
            });
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
    // Serializable holder for the waypoint. SMAPI's data API needs a class with
    // public properties (it can't serialize a C# tuple directly).
    public class WaypointData
    {
        public string Map { get; set; } = "";
        public int X { get; set; }
        public int Y { get; set; }
    }
}
