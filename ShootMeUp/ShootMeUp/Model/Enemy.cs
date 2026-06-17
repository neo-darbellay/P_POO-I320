using ShootMeUp.Properties;
using System.Numerics;
using static System.Net.Mime.MediaTypeNames;

namespace ShootMeUp.Model
{
    /// <summary>
    /// The enemy class, with more attributes than the regular character
    /// </summary>
    public class Enemy : Character
    {
        /// <summary>
        /// Whether or not the enemy can shoot
        /// </summary>
        private readonly bool _blnShoots;

        /// <summary>
        /// The enemy's projectile type
        /// </summary>
        private readonly Projectile.Type _ProjectileType;

        /// <summary>
        /// The enemy's target
        /// </summary>
        private readonly Character _Target;

        /// <summary>
        /// The cooldown that is used to check if enemies can attack or not (in seconds)
        /// </summary>
        private readonly float DamageCooldown;

        /// <summary>
        /// A timer used to determine if the enemy can attack
        /// </summary>
        public float LastDamageTimer;

        /// <summary>
        /// The enemy's max distance value to the player
        /// </summary>
        public float? MaxDistance;

        /// <summary>
        /// The score that the enemy gives when it dies
        /// </summary>
        public int ScoreValue { get; private set; }

        /// <summary>
        /// The shooting enemy's constructor
        /// </summary>
        /// <param name="x">Its starting X position</param>
        /// <param name="y">Its starting Y position</param>
        /// <param name="length">The length of the character</param>
        /// <param name="type">The enemy's type (zombie, skeleton, ...)</param>
        /// <param name="GameSettings.Current.GameSpeedValue">The game's speed</param>
        /// <param name="Target">The enemy's target</param>
        public Enemy(float x, float y, int length, Type type, int GAMESPEED, Character Target) : base(x, y, length, type, GAMESPEED)
        {
            _Target = Target;

            Position.X = x;
            Position.Y = y;

            // Set up the enemy depending on the current type
            switch (type)
            {
                case Type.Zombie:
                    ScoreValue = 1;
                    Lives = 10;

                    _fltBaseSpeed = 0.4f;
                    break;
                case Type.Skeleton:
                    ScoreValue = 3;
                    Lives = 5;

                    _fltBaseSpeed = 0.5f;

                    MaxDistance = 8 * ShootMeUp.DEFAULT_CHARACTER_SIZE;

                    _blnShoots = true;
                    _ProjectileType = Projectile.Type.Arrow_Small;
                    break;
                case Type.Baby_Zombie:
                    ScoreValue = 4;
                    Lives = 3;

                    _fltBaseSpeed = 1.5f;

                    break;
                case Type.Blaze:
                    ScoreValue = 6;
                    Lives = 10;

                    _fltBaseSpeed = 0.25f;

                    MaxDistance = 12 * ShootMeUp.DEFAULT_CHARACTER_SIZE;

                    _blnShoots = true;
                    _ProjectileType = Projectile.Type.Fireball_Small;
                    break;
                case Type.Zombie_Pigman:
                    ScoreValue = 5;
                    Lives = 20;

                    _fltBaseSpeed = 0.2f;

                    break;
                case Type.SpiderJockey:
                    ScoreValue = 20;
                    Lives = 25;

                    _fltBaseSpeed = 0.75f;

                    CanCollide = false;
                    _blnShoots = true;
                    _ProjectileType = Projectile.Type.Arrow_Jockey;
                    break;
                case Type.WitherSkeleton:
                    ScoreValue = 50;
                    Lives = 35;

                    _fltBaseSpeed = 0.5f;
                    break;
                case Type.Wither:
                    ScoreValue = 100;
                    Lives = 50;

                    _fltBaseSpeed = 0.2f;

                    CanCollide = false;
                    _blnShoots = true;
                    _ProjectileType = Projectile.Type.WitherSkull;
                    break;
                case Type.Dragon:
                    ScoreValue = 250;
                    Lives = 100;

                    _fltBaseSpeed = 0.5f;

                    CanCollide = false;
                    _blnShoots = true;
                    _ProjectileType = Projectile.Type.DragonFireball;

                    break;
                default:
                    _ProjectileType = Projectile.Type.Undefined;
                    break;
            }

            // Change the damage cooldown depending on the projectile type
            DamageCooldown = _ProjectileType switch
            {
                Projectile.Type.Arrow_Small or Projectile.Type.Arrow_Big or Projectile.Type.Arrow_Jockey => 6f / GameSettings.Current.GameSpeedValue,
                Projectile.Type.Fireball_Small or Projectile.Type.Fireball_Big => 12f / GameSettings.Current.GameSpeedValue,
                Projectile.Type.WitherSkull => 4f / GameSettings.Current.GameSpeedValue,
                Projectile.Type.DragonFireball => 10f / GameSettings.Current.GameSpeedValue,
                _ => type switch
                {
                    Type.Baby_Zombie => 3f,
                    Type.Zombie_Pigman => 8f,
                    _ => 5,
                },// No projectile, check the enemy type
            };

            // Add 60 to the cooldown
            DamageCooldown *= 60;

            LastDamageTimer = 0;
        }

        public bool CheckPlayerCollision()
        {
            bool blnColliding = false;


            // Collision checks that simulate movement
            if (ShootMeUp.IsOverlapping(_Target, Position.X + _fltSpeed.X, Position.Y, Size.Width, Size.Height))
            {
                blnColliding = true;
            }

            if (ShootMeUp.IsOverlapping(_Target, Position.X, Position.Y + _fltSpeed.Y, Size.Width, Size.Height))
            {
                blnColliding = true;
            }

            return blnColliding;
        }

        public Obstacle? GetCollidingObstacle()
        {
            foreach (Obstacle obstacle in ShootMeUp.GetObstaclesNear(Position.X, Position.Y, Size.Width, Size.Height, expandChunks: 1))

            {
                // Skip the current obstacle if it has no collisions or is invincible
                if (!obstacle.CanCollide || obstacle.Invincible)
                    continue;


                if (ShootMeUp.IsOverlapping(obstacle, Position.X + _fltSpeed.X, Position.Y, Size.Width, Size.Height))
                {
                    return obstacle;
                }

                if (ShootMeUp.IsOverlapping(obstacle, Position.X, Position.Y + _fltSpeed.Y, Size.Width, Size.Height))
                {
                    return obstacle;
                }
            }

            return null;
        }

        /// <summary>
        /// Move the enemy to the player
        /// </summary>
        public void Move()
        {
            if (Lives <= 0) return;

            LastDamageTimer += ShootMeUp.DeltaTime;

            // Calculate direction to target
            float deltaX = (_Target.Position.X + _Target.Size.Width / 2) - (Position.X + Size.Width / 2);
            float deltaY = (_Target.Position.Y + _Target.Size.Height / 2) - (Position.Y + Size.Height / 2);

            float distanceToPlayer = MathF.Sqrt(deltaX * deltaX + deltaY * deltaY);

            // add a negative if closer to player than distance, add positive if closer to distance

            float fltDirection = 1f;

            // Decide whether or not the enemy should move backwards/forwards if the MaxDistance value is set
            if (MaxDistance.HasValue)
            {
                float distanceToMax = distanceToPlayer - MaxDistance.Value;

                // Smoothly scale direction (-1 to +1)
                fltDirection = Math.Clamp(distanceToMax / MaxDistance.Value, -1f, 1f);
            }

            if (distanceToPlayer != 0 && fltDirection != 0)
            {
                deltaX /= distanceToPlayer;
                deltaY /= distanceToPlayer;
            }

            // Apply game speed and base speed
            float speedX = deltaX * _GAMESPEED * _fltBaseSpeed * ShootMeUp.DeltaTime * fltDirection;
            float speedY = deltaY * _GAMESPEED * _fltBaseSpeed * ShootMeUp.DeltaTime * fltDirection;

            // Move smoothly along X and Y axes
            Position.X = MoveAxis(Position.X, Position.Y, speedX, true);
            Position.Y = MoveAxis(Position.X, Position.Y, speedY, false);

            // Store the current speed for reference
            _fltSpeed.X = speedX;
            _fltSpeed.Y = speedY;

            // Handle attacking the player or obstacle
            HandleAttackOrShoot();
        }

        /// <summary>
        /// Handles damage and shooting logic after movement
        /// </summary>
        private void HandleAttackOrShoot()
        {
            if (!_blnShoots)
            {
                if (LastDamageTimer < DamageCooldown) return;

                bool blnPlayerCollision = CheckPlayerCollision();
                Obstacle? obstacleHit = GetCollidingObstacle();

                if (blnPlayerCollision || (obstacleHit != null && !obstacleHit.Invincible))
                {
                    if (blnPlayerCollision)
                        Damage((CFrame)_Target);
                    else if (obstacleHit != null && !obstacleHit.Invincible)
                        Damage((CFrame)obstacleHit);

                    LastDamageTimer = 0;
                }
            }
            else
            {
                if (LastDamageTimer < DamageCooldown)
                    return;

                if (_Target.Lives <= 0)
                    return;

                Projectile? proj = Shoot();
                if (proj != null)
                {
                    ShootMeUp.Projectiles.Add(proj);

                    LastDamageTimer = 0;
                }
            }
        }

        /// <summary>
        /// Shoot a projectile
        /// </summary>
        public Projectile? Shoot()
        {
            // Shoot an arrow from the player's position to the cursor's position if they are alive
            if (Lives > 0)
            {
                float fltTargetX = _Target.Position.X + _Target.Size.Width/2;
                float fltTargetY = _Target.Position.Y + _Target.Size.Height/2;

                // Slow the projectile down by dividing its GameSettings.Current.GameSpeedValue reference by 2
                int intFakeGameSpeed = GameSettings.Current.GameSpeedValue/2;

                // Fire a new projectile if possible
                return new(_ProjectileType, this, fltTargetX, fltTargetY, intFakeGameSpeed);
            }

            return null;
        }

        /// <summary>
        /// Damage the given CFrame if it's a character or an obstacle
        /// </summary>
        /// <param name="singularCFrame">The given CFrame</param>
        public void Damage(CFrame singularCFrame)
        {
            // Get the enemy's damage
            int intDamage = 1;
            switch (CharType)
            {
                case Type.WitherSkeleton:
                    intDamage = 3;
                    break;
            }

            if (singularCFrame is Character player)
            {
                player.Lives -= intDamage;
            }
            else if (singularCFrame is Obstacle obstacle)
            {
                obstacle.Health -= intDamage;

            }
        }
    }
}
