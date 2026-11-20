using System.Collections.Generic;
using UnityEngine;

namespace KingdomNodeInspector.UI
{
    internal static class NodeInspectorIcons
    {
        private static readonly List<Texture2D> Textures = new List<Texture2D>();
        private static readonly List<Sprite> Sprites = new List<Sprite>();

        private static Sprite chevronRight;
        private static Sprite chevronDown;
        private static Sprite trash;
        private static Sprite close;
        private static Sprite refresh;

        public static Sprite ChevronRight => chevronRight ??= BuildSprite(new[]
        {
            "............",
            "...##.......",
            "....##......",
            ".....##.....",
            "......##....",
            ".......##...",
            ".......##...",
            "......##....",
            ".....##.....",
            "....##......",
            "...##.......",
            "............"
        });

        public static Sprite ChevronDown => chevronDown ??= BuildSprite(new[]
        {
            "............",
            "............",
            "............",
            ".##......##.",
            "..##....##..",
            "...##..##...",
            "....####....",
            ".....##.....",
            "............",
            "............",
            "............",
            "............"
        });

        public static Sprite Trash => trash ??= BuildSprite(new[]
        {
            "....####....",
            "....#..#....",
            ".##########.",
            "..########..",
            "..#......#..",
            "..#.#..#.#..",
            "..#.#..#.#..",
            "..#.#..#.#..",
            "..#.#..#.#..",
            "..#......#..",
            "..########..",
            "............"
        });

        public static Sprite Close => close ??= BuildSprite(new[]
        {
            "............",
            ".##......##.",
            "..##....##..",
            "...##..##...",
            "....####....",
            ".....##.....",
            "....####....",
            "...##..##...",
            "..##....##..",
            ".##......##.",
            "............",
            "............"
        });

        public static Sprite Refresh => refresh ??= BuildSprite(new[]
        {
            "....####....",
            "..##....##..",
            ".##......##.",
            ".#........##",
            ".#.......###",
            ".#..........",
            "..........#.",
            "###.......#.",
            "##........#.",
            ".##......##.",
            "..##....##..",
            "....####...."
        });

        public static void Release()
        {
            for (int i = 0; i < Sprites.Count; i++)
            {
                if (Sprites[i])
                    Object.Destroy(Sprites[i]);
            }

            for (int i = 0; i < Textures.Count; i++)
            {
                if (Textures[i])
                    Object.Destroy(Textures[i]);
            }

            Sprites.Clear();
            Textures.Clear();
            chevronRight = null;
            chevronDown = null;
            trash = null;
            close = null;
            refresh = null;
        }

        private static Sprite BuildSprite(string[] rows)
        {
            int height = rows.Length;
            int width = rows[0].Length;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, mipChain: false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            var pixels = new Color32[width * height];
            var on = new Color32(255, 255, 255, 255);
            var off = new Color32(0, 0, 0, 0);

            for (int y = 0; y < height; y++)
            {
                string row = rows[height - 1 - y];
                for (int x = 0; x < width; x++)
                    pixels[y * width + x] = row[x] == '#' ? on : off;
            }

            texture.SetPixels32(pixels);
            texture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
            Textures.Add(texture);

            var sprite = Sprite.Create(
                texture,
                new Rect(0, 0, width, height),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit: 12f);
            Sprites.Add(sprite);
            return sprite;
        }
    }
}
