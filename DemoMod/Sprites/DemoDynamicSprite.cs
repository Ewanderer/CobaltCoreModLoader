using CobaltCoreModding.Definitions.ExternalItems;
using Microsoft.Xna.Framework.Graphics;

namespace DemoMod.Sprites
{
    public class DemoDynamicSprite : ExternalSprite
    {
        private readonly Func<object> getGraphicsDeviceFunc;

        private readonly Random random = new Random();

        private Texture2D? current_texture = null;

        public DemoDynamicSprite(string globalName, Func<object> getGraphicsDeviceFunc) : base(globalName)
        {
            this.getGraphicsDeviceFunc = getGraphicsDeviceFunc;
        }

        public override bool IsCaching => false;

        public override object? GetTexture()
        {
            Microsoft.Xna.Framework.Color[] old_colors = new Microsoft.Xna.Framework.Color[9 * 9];
            if (current_texture == null)
            {
                current_texture = new Texture2D(getGraphicsDeviceFunc() as GraphicsDevice, 9, 9);
                for (int i = 0; i < 9 * 9; i++)
                {
                    old_colors[i] = new Microsoft.Xna.Framework.Color(random.Next(256), random.Next(256), random.Next(256), 1);
                }
            }
            else
            {
                current_texture.GetData(old_colors);
            }
            Microsoft.Xna.Framework.Color[] new_colors = new Microsoft.Xna.Framework.Color[9 * 9];
            {
                int i = 0;
                foreach (var color in old_colors)
                {
                    var r = (color.R + 1) % 256;
                    var b = (color.B + 1) % 256;
                    var g = (color.G + 1) % 256;
                    new_colors[i++] = new Microsoft.Xna.Framework.Color(r, g, b, 255);
                }
            }
            current_texture.SetData(new_colors);
            return current_texture;
        }
    }
}