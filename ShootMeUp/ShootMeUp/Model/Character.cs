using ShootMeUp.Properties;
using System.Numerics;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;

namespace ShootMeUp.Model
{
    /// <summary>
    /// The base class for characters.
    /// </summary>
    public class Character : CFrame
    {
        protected int _GAMESPEED;

        /// <summary>
        /// The character's speed in the X and Y direction
        /// </summary>
        protected (float X, float Y) _fltSpeed;

        /// <summary>
        /// The character's base speed
        /// </summary>
        protected float _fltBaseSpeed;

        // Variables used for projectile cooldown
        protected float _arrowCooldownTimer = 0;
        protected float _fireballCooldownTimer = 0;

        private readonly float _arrowCooldown;
        private readonly float _fireballCooldown;

        protected Type _Type;

        /// <summary>
        /// The character's remaining lives
        /// </summary>
        public int Lives { get; set; }

        /// <summary>
        /// The character's type (player, ...)
        /// </summary>
        public enum Type
        {
            Player,
            Zombie,
            Skeleton,
            Baby_Zombie,
            Blaze,
            Zombie_Pigman,
            SpiderJockey,
            WitherSkeleton,
            Dragon,
            Wither
        }

        // <summary>
        /// The character's current type
        /// </summary>
        public Type CharType
        {
            get { return _Type; }
        }

        /// <summary>
        /// The character's constructor
        /// </summary>
        /// <param name="x">Its starting X position</param>
        /// <param name="y">Its starting Y position</param>
        /// <param name="length">The length of the character</param>
        /// <param name="type">The character's type (player, enemy)</param>
        /// <param name="GameSpeedValue">The game's speed</param>
        public Character(float x, float y, int length, Character.Type type, int GameSpeedValue) : base(x - (length / 2f), y - length / 2f, length)
        {
            _GAMESPEED = GameSpeedValue;
            Lives = 10;
            _Type = type;
            _fltBaseSpeed = 1;

            _arrowCooldown = 1.5f / GameSettings.Current.GameSpeedValue * 60;
            _fireballCooldown = 4.5f / GameSettings.Current.GameSpeedValue * 60;
        }

        protected (bool X, bool Y) CheckObstacleCollision()
        {
            (bool X, bool Y) blnColliding = (false, false);

            foreach (Obstacle obstacle in ShootMeUp.GetObstaclesNear(Position.X, Position.Y, Size.Width, Size.Height, expandChunks: 1))
            {
                // Skip the current obstacle if it has no collisions
                if (!obstacle.CanCollide)
                    continue;

                // Collision checks that simulate movement
                if (ShootMeUp.IsOverlapping(obstacle, Position.X + _fltSpeed.X, Position.Y, Size.Width, Size.Height))
                {
                    blnColliding.X = true; // Collision if moved along X axis
                }

                if (ShootMeUp.IsOverlapping(obstacle, Position.X, Position.Y + _fltSpeed.Y, Size.Width, Size.Height))
                {
                    blnColliding.Y = true; // Collision if moved along Y axis
                }

                // Early exit if both collisions detected
                if (blnColliding.X && blnColliding.Y)
                    break;
            }

            return blnColliding;
        }

        /// <summary>
        /// Move the character on both axis if they're alive
        /// </summary>
        /// <param name="x">The movement on the x axis</param>
        /// <param name="y">The movement on the y axis</param>
        public void Move(float x, float y)
        {
            if (Lives <= 0) return;

            _fltSpeed.X = x * _fltBaseSpeed;
            _fltSpeed.Y = y * _fltBaseSpeed;

            // Move along X axis
            if (_fltSpeed.X != 0)
                Position.X = MoveAxis(Position.X, Position.Y, _fltSpeed.X * ShootMeUp.DeltaTime, true);

            // Move along Y axis
            if (_fltSpeed.Y != 0)
                Position.Y = MoveAxis(Position.X, Position.Y, _fltSpeed.Y * ShootMeUp.DeltaTime, false);
        }

        /// <summary>
        /// Update the character's timers
        /// </summary>
        public void UpdateTimers()
        {
            float dt = ShootMeUp.DeltaTime;

            _arrowCooldownTimer += dt;
            _fireballCooldownTimer += dt;
        }

        /// <summary>
        /// Incrementally moves along one axis until just before collision
        /// </summary>
        /// <param name="currentX">Current X position</param>
        /// <param name="currentY">Current Y position</param>
        /// <param name="delta">Movement along this axis</param>
        /// <param name="isX">True if moving along X, false if along Y</param>
        /// <returns>The new position along the axis</returns>
        protected float MoveAxis(float currentX, float currentY, float delta, bool isX)
        {
            float sign = Math.Sign(delta);
            float remaining = Math.Abs(delta);

            while (remaining > 0)
            {
                // Move by 1 pixel at a time (or smaller step for faster objects)
                float step = Math.Min(1f, remaining);

                float testX = currentX + (isX ? step * sign : 0);
                float testY = currentY + (isX ? 0 : step * sign);

                // Handle collisions if needed
                if (CanCollide)
                {
                    bool colliding = false;
                    foreach (Obstacle obstacle in ShootMeUp.GetObstaclesNear(testX, testY, Size.Width, Size.Height, expandChunks: 1))
                    {
                        if (!obstacle.CanCollide) continue;

                        if (ShootMeUp.IsOverlapping(obstacle, testX, testY, Size.Width, Size.Height))
                        {
                            colliding = true;
                            break;
                        }
                    }

                    // Don't move in the current axis if they would collide with something 
                    if (colliding)
                        break;
                }

                // Move the character
                currentX = testX;
                currentY = testY;
                remaining -= step;
            }

            return isX ? currentX : currentY;
        }

        /// <summary>
        /// Shoot a projectile
        /// </summary>
        /// <param name="target">The projectile's target</param>
        /// <param name="type">The projectile type</param>
        /// <returns>A projectile if it shot, otherwise none</returns>
        public Projectile? Shoot(CFrame target, Projectile.Type type)
        {
            if (Lives <= 0)
                return null;

            // Shoot a projectile from the player's position to the cursor's position
            if (type == Projectile.Type.Arrow_Big)
            {
                if (_arrowCooldownTimer < _arrowCooldown)
                    return null;

                _arrowCooldownTimer = 0;
                return new Projectile(type, this, target.Position.X, target.Position.Y, _GAMESPEED);
            }
            else if (type == Projectile.Type.Fireball_Big)
            {
                if (_fireballCooldownTimer < _fireballCooldown)
                    return null;

                _fireballCooldownTimer = 0f;
                return new Projectile(type, this, target.Position.X, target.Position.Y, _GAMESPEED);
            }

            return null;
        }

        public override string ToString()
        {
            if (Lives > 0)
                return $"{Lives} HP";
            else
                return "";
        }
    }
}