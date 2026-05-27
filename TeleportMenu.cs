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

        private readonly IMonitor _monitor;
        private readonly List<ClickableComponent> _buttons = new();

        //Constructor 
        public TeleportMenu(IMonitor monitor)
            : base(
                x:      (Game1.uiViewport.Width  - (BtnW + EdgePad * 2)) / 2,
                y:      (Game1.uiViewport.Height - (TitleH + Locations.Count * (BtnH + BtnGap) + EdgePad)) / 2,
                width:   BtnW + EdgePad * 2,
                height:  TitleH + Locations.Count * (BtnH + BtnGap) + EdgePad
            )
        {
            _monitor = monitor;

            for (int i = 0; i < Locations.Count; i++)
            {
                _buttons.Add(new ClickableComponent(
                    bounds: new Rectangle(
                        xPositionOnScreen + EdgePad,
                        yPositionOnScreen + TitleH + i * (BtnH + BtnGap),
                        BtnW,
                        BtnH
                    ),
                    name: Locations[i].Label
                ));
            }
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

                // Button background — highlight on hover
                drawTextureBox(b,
                    btn.bounds.X, btn.bounds.Y,
                    btn.bounds.Width, btn.bounds.Height,
                    hovered ? Color.Wheat : Color.White);

                // Button label (centred)
                Vector2 textSize = Game1.smallFont.MeasureString(btn.name);
                Utility.drawTextWithShadow(b, btn.name, Game1.smallFont,
                    new Vector2(
                        btn.bounds.X + btn.bounds.Width  / 2f - textSize.X / 2f,
                        btn.bounds.Y + btn.bounds.Height / 2f - textSize.Y / 2f
                    ),
                    hovered ? new Color(86, 22, 12) : Game1.textColor);
            }

            // 6. Mouse cursor (always draw last)
            drawMouse(b);
        }

        // Input handling
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                if (!_buttons[i].containsPoint(x, y)) continue;

                var (label, map, tileX, tileY) = Locations[i];
                _monitor.Log($"[TeleportMod] Warping to {label} ({map} @ {tileX},{tileY})", LogLevel.Debug);

                Game1.warpFarmer(map, tileX, tileY, false);
                Game1.playSound("wand");
                exitThisMenu();
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
            // intentionally NOT calling base — prevents stray key handling
        }
    }
}
