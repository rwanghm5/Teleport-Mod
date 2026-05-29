using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;

namespace TeleportMod
{

    //SDV menu that lets the player warp to any of the preset locations.

    public class TeleportMenu : IClickableMenu
    {
        // Warp Destinations
        private static readonly List<(string Label, string Map, int X, int Y)> Locations = new()
        {
            ("Farm",           "Farm",        64,  15),
            ("Pelican Town",   "Town",        35,  35),
            ("Beach",          "Beach",       20,   4),
            ("Mountain",       "Mountain",    31,  20),
            ("Forest",         "Forest",      60,  20),
            ("Bus Stop",       "BusStop",     12,  10),
            ("Mine Entrance",  "Mountain",    50,   8),
            ("Desert",         "Desert",      35,  43),
            ("Witch's Swamp",  "WitchSwamp",  20,  26),
            ("Ginger Island",  "IslandSouth", 21,  35),
        };

        //Layout constants 
        private const int BtnW      = 320;   // button width
        private const int BtnH      = 52;    // button height
        private const int BtnGap    = 6;     // vertical gap between buttons
        private const int TitleH    = 56;    // space reserved for the title
        private const int EdgePad   = 24;    // inner horizontal padding

        // Special action button names (used to tell them apart from location buttons)
        private const string SaveWaypointName     = "Save Waypoint";
        private const string TeleportWaypointName = "Teleport to Waypoint";
        
        private readonly IMonitor _monitor;
        private readonly ModEntry _mod;
        private readonly List<ClickableComponent> _buttons = new();

        
        private static int ExtraRowCount(ModEntry mod)
            => mod.Waypoint.HasValue ? 2 : 1;

        // Total rows = preset locations + extra action rows.
        private static int RowCount(ModEntry mod)
            => Locations.Count + ExtraRowCount(mod);
        
        //Constructor 
        public TeleportMenu(IMonitor monitor, ModEntry mod)
            : base(
                x:      (Game1.uiViewport.Width  - (BtnW + EdgePad * 2)) / 2,
                y:      (Game1.uiViewport.Height - (TitleH + RowCount(mod) * (BtnH + BtnGap) + EdgePad)) / 2,
                width:   BtnW + EdgePad * 2,
                height:  TitleH + RowCount(mod) * (BtnH + BtnGap) + EdgePad
            )
        {
            _monitor = monitor;
            _mod     = mod;

            int row = 0;

            // Preset location buttons
            for (int i = 0; i < Locations.Count; i++, row++)
            {
                _buttons.Add(new ClickableComponent(
                    bounds: new Rectangle(
                        xPositionOnScreen + EdgePad,
                        yPositionOnScreen + TitleH + row * (BtnH + BtnGap),
                        BtnW,
                        BtnH
                    ),
                    name: Locations[i].Label
                ));
            }

            // "Teleport to Waypoint" button (only if a waypoint is saved)
            if (_mod.Waypoint.HasValue)
            {
                _buttons.Add(new ClickableComponent(
                    bounds: new Rectangle(
                        xPositionOnScreen + EdgePad,
                        yPositionOnScreen + TitleH + row * (BtnH + BtnGap),
                        BtnW,
                        BtnH
                    ),
                    name: TeleportWaypointName
                ));
                row++;
            }

            // "Save Waypoint" button (always present, last row)
            _buttons.Add(new ClickableComponent(
                bounds: new Rectangle(
                    xPositionOnScreen + EdgePad,
                    yPositionOnScreen + TitleH + row * (BtnH + BtnGap),
                    BtnW,
                    BtnH
                ),
                name: SaveWaypointName
            ));
        }

        // Drawing
        public override void draw(SpriteBatch b)
        {
            // 1. Dim everything behind the menu
            b.Draw(Game1.fadeToBlackRect,
                   new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
                   Color.Black * 0.4f);

            // 2. Outer dialog box (uses the standard SDV menu texture)
            drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, width, height, Color.White);

            // 3. Title
            const string title = "Teleport";
            Vector2 titleSize = Game1.dialogueFont.MeasureString(title);
            Utility.drawTextWithShadow(b, title, Game1.dialogueFont,
                new Vector2(
                    xPositionOnScreen + width / 2f  - titleSize.X / 2f,
                    yPositionOnScreen + 14f
                ),
                Game1.textColor);

            // 4. Separator line under title
            b.Draw(Game1.staminaRect,
                   new Rectangle(xPositionOnScreen + EdgePad, yPositionOnScreen + TitleH - 6, BtnW, 2),
                   Color.BurlyWood * 0.6f);

            // 5. Location buttons
            int mx = Game1.getMouseX();
            int my = Game1.getMouseY();

            foreach (var btn in _buttons)
            {
                bool hovered = btn.containsPoint(mx, my);
                bool isAction = btn.name == SaveWaypointName || btn.name == TeleportWaypointName;

                // Action buttons get a slightly different tint so they stand out
                Color baseColor = isAction ? Color.PaleGoldenrod : Color.White;

                // Button background
                drawTextureBox(b,
                    btn.bounds.X, btn.bounds.Y,
                    btn.bounds.Width, btn.bounds.Height,
                    hovered ? Color.Wheat : baseColor);

                // Build label text. For the waypoint teleport button, append the
                // saved coordinates so the player knows where it goes.
                string labelText = btn.name;
                if (btn.name == TeleportWaypointName && _mod.Waypoint.HasValue)
                {
                    var wp = _mod.Waypoint.Value;
                    labelText = $"Waypoint: {wp.Map} ({wp.X},{wp.Y})";
                }

                // Button label (centred)
                Vector2 textSize = Game1.smallFont.MeasureString(labelText);
                Utility.drawTextWithShadow(b, labelText, Game1.smallFont,
                    new Vector2(
                        btn.bounds.X + btn.bounds.Width  / 2f - textSize.X / 2f,
                        btn.bounds.Y + btn.bounds.Height / 2f - textSize.Y / 2f
                    ),
                    hovered ? new Color(86, 22, 12) : Game1.textColor);
            }

            // 6. Mouse cursor
            drawMouse(b);
        }

        // Input handling
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            foreach (var btn in _buttons)
            {
                if (!btn.containsPoint(x, y)) continue;

                // --- Save Waypoint -------------------------------------------------
                if (btn.name == SaveWaypointName)
                {
                    if (Game1.player?.currentLocation == null)
                        return;

                    string map = Game1.player.currentLocation.Name;
                    int tileX  = Game1.player.TilePoint.X;
                    int tileY  = Game1.player.TilePoint.Y;

                    _mod.SaveWaypoint(map, tileX, tileY);
                    _monitor.Log($"[TeleportMod] Saved waypoint at {map} ({tileX},{tileY})", LogLevel.Debug);
                    Game1.playSound("coin");

                    // Rebuild the menu so the "Teleport to Waypoint" button appears.
                    Game1.activeClickableMenu = new TeleportMenu(_monitor, _mod);
                    return;
                }

                // --- Teleport to Waypoint -----------------------------------------
                if (btn.name == TeleportWaypointName)
                {
                    if (!_mod.Waypoint.HasValue)
                        return;

                    var wp = _mod.Waypoint.Value;
                    _monitor.Log($"[TeleportMod] Warping to waypoint ({wp.Map} @ {wp.X},{wp.Y})", LogLevel.Debug);

                    Game1.warpFarmer(wp.Map, wp.X, wp.Y, false);
                    Game1.playSound("wand");
                    exitThisMenu();
                    return;
                }

                // --- Preset location ----------------------------------------------
                foreach (var loc in Locations)
                {
                    if (loc.Label != btn.name) continue;

                    _monitor.Log($"[TeleportMod] Warping to {loc.Label} ({loc.Map} @ {loc.X},{loc.Y})", LogLevel.Debug);
                    Game1.warpFarmer(loc.Map, loc.X, loc.Y, false);
                    Game1.playSound("wand");
                    exitThisMenu();
                    return;
                }

                return;
            }
        }

        //Right-click closes the menu
        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            exitThisMenu();
        }

        //Escape closes the menu
        public override void receiveKeyPress(Keys key)
        {
            if (key == Keys.Escape)
                exitThisMenu();
        }
    }
}
