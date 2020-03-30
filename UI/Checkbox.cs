﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericModConfigMenu.UI
{
    public class Checkbox : Element
    {
        public Texture2D Texture { get; set; }
        public Rectangle CheckedTextureRect { get; set; }
        public Rectangle UncheckedTextureRect { get; set; }

        public Action<Element> Callback { get; set; }

        public bool Checked { get; set; } = true;

        public Checkbox()
        {
            Texture = Game1.mouseCursors;
            CheckedTextureRect = OptionsCheckbox.sourceRectChecked;
            UncheckedTextureRect = OptionsCheckbox.sourceRectUnchecked;
        }

        public override void Update()
        {
            var bounds = new Rectangle((int)Position.X, (int)Position.Y, CheckedTextureRect.Width * 4, CheckedTextureRect.Height * 4);
            bool hover = bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()) && !GetRoot().Obscured;

            if (hover && Game1.oldMouseState.LeftButton == ButtonState.Released && Mouse.GetState().LeftButton == ButtonState.Pressed && Callback != null)
            {
                Game1.playSound("drumkit6");
                Checked = !Checked;
                Callback.Invoke(this);
            }
        }

        public override void Draw(SpriteBatch b)
        {
            b.Draw(Texture, Position, Checked ? CheckedTextureRect : UncheckedTextureRect, Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, 0);
            Game1.activeClickableMenu?.drawMouse(b);
        }
    }
}
