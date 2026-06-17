using ShootMeUp.Properties;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShootMeUp.Model
{
    static class Sprites
    {
        // Characters
        public static Bitmap Player             => Resources.Character_Player;
        public static Bitmap Zombie             => Resources.Character_Zombie;
        public static Bitmap Skeleton           => Resources.Character_Skeleton;
        public static Bitmap Pigman             => Resources.Character_Zombie_Pigman;
        public static Bitmap BabyZombie         => Resources.Character_Zombie;
        public static Bitmap Blaze              => Resources.Character_Blaze;
        public static Bitmap SpiderJockey       => Resources.Character_SpiderJockey;
        public static Bitmap Dragon             => Resources.Character_Dragon;
        public static Bitmap Wither             => Resources.Character_Wither;

        // Projectiles
        public static Bitmap Arrow              => Resources.Projectile_Arrow;
        public static Bitmap Fireball           => Resources.Projectile_Fireball;
        public static Bitmap WitherSkull        => Resources.Projectile_WitherSkull;
        public static Bitmap DragonFireball     => Resources.Projectile_DragonFireball;

        // Obstacles
        public static Bitmap Dirt               => Resources.Obstacle_Dirt;
        public static Bitmap WoodenPlanks       => Resources.Obstacle_WoodPlanks;
        public static Bitmap Cobblestone        => Resources.Obstacle_Cobblestone;
        public static Bitmap Spawner            => Resources.Obstacle_Spawner;
        public static Bitmap Bedrock            => Resources.Obstacle_Bedrock;
        public static Bitmap Barrier            => Resources.Obstacle_Barrier;
        public static Bitmap Bush               => Resources.Obstacle_Bush;

        // Floor
        public static Bitmap Grass              => Resources.Floor_Grass;
        public static Bitmap Stone              => Resources.Floor_Stone;
        public static Bitmap Sand               => Resources.Floor_Sand;

        // Misc
        public static Bitmap PlayerHeart        => Resources.Misc_PlayerHeart;
        public static Bitmap EmptyHeart         => Resources.Misc_EmptyHeart;
        public static Bitmap EnemyHeart         => Resources.Misc_EnemyHeart;
        public static Bitmap BossHeart          => Resources.Misc_BossHeart;


        // LOD (Level Of Detail) Cache
        private static readonly ConcurrentDictionary<Obstacle.Type, Bitmap> _lodSprites = new();


        private const int RotationCount = 64; // 360 / 64 = 5.625 degrees
        private static readonly Bitmap[,] ProjectileRotations = new Bitmap[Enum.GetValues(typeof(Projectile.Type)).Length, RotationCount];

        // Tint cache for floors
        private static readonly ConcurrentDictionary<Color, Bitmap> _tintedFloors = new();

        // Tiled floor cache: (floorType, size) -> bitmap
        private const int MaxFloorTileCache = 32;

        private static readonly LinkedList<(Obstacle.Type type, int size)> _tileLRU = new();

        private static readonly Dictionary<(Obstacle.Type type, int size), Bitmap> _tiledFloors = [];

        private static void AddToLRU((Obstacle.Type type, int size) key)
        {
            _tileLRU.Remove(key);      // remove if already existed
            _tileLRU.AddFirst(key);    // put at front (most recently used)

            if (_tileLRU.Count > MaxFloorTileCache)
            {
                // Evict least-recently-used key
                var last = _tileLRU.Last?.Value;

                if (last == null) return; // should never happen, but just in case

                _tileLRU.RemoveLast();

                if (_tiledFloors.TryGetValue(((Obstacle.Type type, int size))last, out Bitmap? oldBmp))
                {
                    oldBmp?.Dispose();
                    _tiledFloors.Remove(((Obstacle.Type type, int size))last);
                }
            }
        }

        private static int QuantizeSize(int size)
        {
            // Round to nearest 32, 64, 128, 256
            if (size <= 32) return 32;
            if (size <= 64) return 64;
            if (size <= 128) return 128;
            if (size <= 256) return 256;

            // For larger worlds, round to nearest 256 block
            return (size + 255) / 256 * 256;
        }

        public static void InitializeProjectileRotations()
        {
            // Safety: dispose old rotations if any
            for (int t = 0; t < ProjectileRotations.GetLength(0); t++)
            {
                for (int i = 0; i < ProjectileRotations.GetLength(1); i++)
                {
                    ProjectileRotations[t, i]?.Dispose();
                    ProjectileRotations[t, i] = null!;
                }
            }

            foreach (Projectile.Type type in Enum.GetValues(typeof(Projectile.Type)))
            {
                if (type == Projectile.Type.WitherSkull || type == Projectile.Type.DragonFireball)
                    continue; // no rotation needed

                Bitmap baseSprite = GetBaseProjectile(type);
                for (int i = 0; i < RotationCount; i++)
                {
                    float angle = i * (360f / RotationCount);
                    ProjectileRotations[(int)type, i] = RotateSprite(baseSprite, angle);
                }
            }
        }


        public static Bitmap GetCharacterSprite(Character.Type type)
        {
            return type switch
            {
                Character.Type.Player => Player,
                Character.Type.Zombie => Zombie,
                Character.Type.Skeleton => Skeleton,
                Character.Type.Baby_Zombie => BabyZombie,
                Character.Type.Blaze => Blaze,
                Character.Type.Zombie_Pigman => Pigman,
                Character.Type.SpiderJockey => SpiderJockey,
                Character.Type.Wither => Wither,
                Character.Type.Dragon => Dragon,
                _ => Player
            };
        }

        private static Bitmap GetLODObstacle(Obstacle.Type type)
        {
            return _lodSprites.GetOrAdd(type, _ =>
            {
                Color color = type switch
                {
                    Obstacle.Type.Dirt => Color.SaddleBrown,
                    Obstacle.Type.Wood => Color.Peru,
                    Obstacle.Type.CobbleStone => Color.DarkGray,
                    Obstacle.Type.Barrier => Color.Red,
                    Obstacle.Type.Bedrock => Color.Black,
                    Obstacle.Type.Spawner => Color.Purple,
                    Obstacle.Type.Bush => Color.Green,
                    Obstacle.Type.Grass => Color.FromArgb(145, 189, 89),
                    Obstacle.Type.Stone => Color.Gray,
                    Obstacle.Type.Sand => Color.Khaki,
                    _ => Color.White
                };

                Bitmap bmp = new(1, 1);
                bmp.SetPixel(0, 0, color);
                return bmp;
            });
        }

        private static Bitmap GetBaseObstacle(Obstacle.Type type)
        {
            return type switch
            {
                Obstacle.Type.Dirt => Dirt,
                Obstacle.Type.Wood => WoodenPlanks,
                Obstacle.Type.CobbleStone => Cobblestone,
                Obstacle.Type.Barrier => Barrier,
                Obstacle.Type.Bedrock => Bedrock,
                Obstacle.Type.Spawner => Spawner,
                Obstacle.Type.Bush => Bush,
                Obstacle.Type.Grass => GetGrass(Color.FromArgb(145, 189, 89)),
                Obstacle.Type.Stone => Stone,
                Obstacle.Type.Sand => Sand,
                _ => Player
            };
        }

        private static Bitmap GetBaseProjectile(Projectile.Type type)
        {
            return type switch
            {
                Projectile.Type.Arrow_Small or Projectile.Type.Arrow_Big or Projectile.Type.Arrow_Jockey => Arrow,
                Projectile.Type.Fireball_Small or Projectile.Type.Fireball_Big => Fireball,
                Projectile.Type.WitherSkull => WitherSkull,
                Projectile.Type.DragonFireball => DragonFireball,
                _ => Arrow
            };
        }

        private static Bitmap TileSprite(Bitmap baseSprite, int width, int height)
        {
            Bitmap tiled = new(width, height);

            using (Graphics g = Graphics.FromImage(tiled))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.SmoothingMode = SmoothingMode.None;

                for (int x = 0; x < width; x += ShootMeUp.OBSTACLE_SIZE)
                {
                    for (int y = 0; y < height; y += ShootMeUp.OBSTACLE_SIZE)
                    {
                        g.DrawImage(baseSprite, x, y, ShootMeUp.OBSTACLE_SIZE, ShootMeUp.OBSTACLE_SIZE);
                    }
                }
            }

            return tiled;
        }

        public static Bitmap GetObstacleSprite(Obstacle.Type type, int intSize, float Zoom)
        {
            // Low zoom → LOD mode (1×1 pixel)
            if (Zoom < 0.7f)
                return GetLODObstacle(type);

            // Only tile floor materials (else return base)
            if (type != Obstacle.Type.Grass &&
                type != Obstacle.Type.Stone &&
                type != Obstacle.Type.Sand)
            {
                return GetBaseObstacle(type);
            }

            // Normalize size
            int size = QuantizeSize(intSize); // width == height for floors

            var key = (type, size);

            // Check if cached
            if (_tiledFloors.TryGetValue(key, out Bitmap? bmp))
            {
                AddToLRU(key); // update LRU
                return bmp;
            }

            // Build new tile
            Bitmap baseSprite = GetBaseObstacle(type);
            Bitmap tiled = TileSprite(baseSprite, size, size);

            // Store into cache
            _tiledFloors[key] = tiled;
            AddToLRU(key);

            return tiled;
        }

        private static Bitmap RotateSprite(Bitmap original, float angle)
        {
            Bitmap rotated = new(original.Width, original.Height);
            rotated.SetResolution(original.HorizontalResolution, original.VerticalResolution);

            using (Graphics g = Graphics.FromImage(rotated))
            {
                g.SmoothingMode = SmoothingMode.None;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;

                g.TranslateTransform(original.Width / 2f, original.Height / 2f);
                g.RotateTransform(angle);
                g.TranslateTransform(-original.Width / 2f, -original.Height / 2f);

                g.DrawImage(original, 0, 0);
            }

            return rotated;
        }

        public static Bitmap GetProjectileSprite(Projectile.Type type, float angle)
        {
            if (type == Projectile.Type.WitherSkull ||
                type == Projectile.Type.DragonFireball)
            {
                return GetBaseProjectile(type);
            }

            float normalized = (angle % 360 + 360) % 360;
            int index = (int)(normalized / (360f / RotationCount));

            return ProjectileRotations[(int)type, index];
        }

        /// <summary>
        /// Recolor a gray texture
        /// </summary>
        /// <param name="gray">Default gray image</param>
        /// <param name="tint">The tint</param>
        /// <returns>The same image, but recolored</returns>
        private static Bitmap ApplyTint(Bitmap gray, Color tint)
        {
            Bitmap result = new(gray.Width, gray.Height);

            for (int x = 0; x < gray.Width; x++)
            {
                for (int y = 0; y < gray.Height; y++)
                {
                    Color px = gray.GetPixel(x, y);

                    // Multiply the grayscale pixel by the tint (Minecraft-style)
                    int r = (px.R * tint.R) / 255;
                    int g = (px.G * tint.G) / 255;
                    int b = (px.B * tint.B) / 255;

                    result.SetPixel(x, y, Color.FromArgb(px.A, r, g, b));
                }
            }

            return result;
        }

        public static Bitmap GetGrass(Color tint)
        {
            return _tintedFloors.GetOrAdd(tint, _ => ApplyTint(Grass, tint));
        }

        public static void Reset()

        {
            // Dispose pre-rotated projectiles
            for (int t = 0; t < ProjectileRotations.GetLength(0); t++)
            {
                for (int i = 0; i < ProjectileRotations.GetLength(1); i++)
                {
                    ProjectileRotations[t, i]?.Dispose();
                    ProjectileRotations[t, i] = null!;
                }
            }

            // Dispose LOD 1x1 images
            foreach (var kv in _lodSprites)
                kv.Value.Dispose();
            _lodSprites.Clear();

            // Dispose tinted floors
            foreach (var kv in _tintedFloors)
                kv.Value.Dispose();
            _tintedFloors.Clear();

            // Dispose tiled floor cache (LRU + map)
            foreach (var kv in _tiledFloors)
                kv.Value.Dispose();
            _tiledFloors.Clear();
            _tileLRU.Clear();
        }
    }
}
