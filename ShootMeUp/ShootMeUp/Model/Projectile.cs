using ShootMeUp.Properties;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace ShootMeUp.Model
{
    public class Projectile : CFrame
    {
        /// <summary>
        /// The character that shot the projectile
        /// </summary>
        private readonly Character _shotBy;

        /// <summary>
        /// The amount of damage the projectile deals (in HP)
        /// </summary>
        private readonly int _intDamage;

        /// <summary>
        /// The projectile's movement speed
        /// </summary>
        private readonly float _fltMovementSpeed;

        /// <summary>
        ///  The projectile's speed in the X and Y axis
        /// </summary>
        private (float X, float Y) _fltSpeed;

        /// <summary>
        /// The X and Y position of the target
        /// </summary>
        private (float X, float Y) _fltTarget;

        /// <summary>
        /// The projectile's type (arrow, ...)
        /// </summary>
        public enum Type
        {
            Arrow_Small,
            Arrow_Big,
            Arrow_Jockey,
            Fireball_Small,
            Fireball_Big,
            WitherSkull,
            DragonFireball,
            Undefined
        }

        /// <summary>
        /// Whether or not the projectile is active or not
        /// </summary>
        public bool Active { get; set; }

        // <summary>
        /// The Projectile's current type
        /// </summary>
        public Type ProjType { get; private set; }

        /// <summary>
        /// The rotation angle (in degrees)
        /// </summary>
        public float RotationAngle { get; private set; }

        public Projectile(Type type, Character ShotBy, float fltTargetX, float fltTargetY, int GAMESPEED): base(0, 0)
        {
            ProjType = type;
            _shotBy = ShotBy;
            _fltTarget.X = fltTargetX;
            _fltTarget.Y = fltTargetY;

            Active = true;
            CanCollide = true;

            // Define the different properties depending on the projectile type
            switch (type)
            {
                case Type.Arrow_Big:
                    Size.Width = ShotBy.Size.Width;
                    Size.Height = ShotBy.Size.Height;
                    _intDamage = 2;
                    _fltMovementSpeed = 3;

                    break;
                case Type.Arrow_Small:
                    Size.Width = ShotBy.Size.Width;
                    Size.Height = ShotBy.Size.Height;
                    _intDamage = 1;
                    _fltMovementSpeed = 2;

                    break;
                case Type.Arrow_Jockey:
                    Size.Width = ShotBy.Size.Width / 2;
                    Size.Height = ShotBy.Size.Height / 2;
                    _intDamage = 2;
                    _fltMovementSpeed = 1.8f;

                    break;
                case Type.Fireball_Big:
                    Size.Width = ShotBy.Size.Width;
                    Size.Height = ShotBy.Size.Height;
                    _intDamage = 5;
                    _fltMovementSpeed = 1;

                    break;
                case Type.Fireball_Small:
                    Size.Width = ShotBy.Size.Width;
                    Size.Height = ShotBy.Size.Height;
                    _intDamage = 2;
                    _fltMovementSpeed = 0.75f;

                    break;
                case Type.WitherSkull:
                    Size.Width = ShotBy.Size.Width / 4;
                    Size.Height = ShotBy.Size.Height / 4;
                    _intDamage = 3;
                    _fltMovementSpeed = 1f;

                    break;
                case Type.DragonFireball:
                    Size.Width = ShotBy.Size.Width / 4;
                    Size.Height = ShotBy.Size.Height / 4;
                    _intDamage = 5;
                    _fltMovementSpeed = 1;

                    CanCollide = false;

                    break;
                default:
                    Size.Width = ShotBy.Size.Width;
                    Size.Height = ShotBy.Size.Height;
                    _intDamage = 0;
                    _fltMovementSpeed = -1;
                    break;
            }

            // Center projectile on shooter
            float shooterCenterX = ShotBy.Position.X + ShotBy.Size.Width / 2f;
            float shooterCenterY = ShotBy.Position.Y + ShotBy.Size.Height / 2f;

            Position.X = shooterCenterX - Size.Width / 2f;
            Position.Y = shooterCenterY - Size.Height / 2f;

            // Multiply the movement speed by the game speed
            _fltMovementSpeed *= GAMESPEED;

            // Calculate direction to target
            float projCenterX = Position.X + Size.Width / 2f;
            float projCenterY = Position.Y + Size.Height / 2f;

            float deltaX = _fltTarget.X - projCenterX;
            float deltaY = _fltTarget.Y - projCenterY;

            // Calculate rotation angle in degrees
            // We add 90 here because the image faces upwards
            RotationAngle = (float)(Math.Atan2(deltaY, deltaX) * (180.0 / Math.PI)) + 90;

            // Normalize direction
            float length = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
            if (length != 0)
            {
                deltaX /= length;
                deltaY /= length;
            }

            // Store movement speed in X/Y components
            _fltSpeed.X = (deltaX * _fltMovementSpeed);
            _fltSpeed.Y = (deltaY * _fltMovementSpeed);
        }

        /// <summary>
        /// Update the projectile's position
        /// </summary>
        public void Update()
        {
            float moveX = _fltSpeed.X * ShootMeUp.DeltaTime;
            float moveY = _fltSpeed.Y * ShootMeUp.DeltaTime;

            int steps = (int)Math.Ceiling(Math.Max(Math.Abs(moveX), Math.Abs(moveY)));
            steps = Math.Min(Math.Max(steps, 1), 10);

            if (steps < 1)
                steps = 1;

            float stepX = moveX / steps;
            float stepY = moveY / steps;

            // Check step by step if the arrow will hit anything
            for (int i = 0; i < steps; i++)
            {
                Position.X += stepX;
                Position.Y += stepY;

                CFrame? hit = GetColliding();

                if (hit != null)
                {
                    Active = false;

                    if (hit is Character characterHit)
                        characterHit.Lives -= _intDamage;
                    else if (hit is Obstacle obstacleHit)
                        obstacleHit.Health -= _intDamage;

                    return;
                }
            }
        }

        public CFrame? GetColliding()
        {
            float stepX = _fltSpeed.X * ShootMeUp.DeltaTime;
            float stepY = _fltSpeed.Y * ShootMeUp.DeltaTime;

            foreach (CFrame singularCFrame in ShootMeUp.Characters)
            {
                // Skip the ignored character
                if (singularCFrame == (CFrame)_shotBy)
                    continue;

                // Skip no collision obstacles
                if (singularCFrame is Obstacle obstacle && !obstacle.CanCollide)
                    continue;


                if (ShootMeUp.IsOverlapping(singularCFrame, Position.X + stepX, Position.Y, Size.Width, Size.Height))
                {
                    return singularCFrame;
                }

                if (ShootMeUp.IsOverlapping(singularCFrame, Position.X, Position.Y + stepY, Size.Width, Size.Height))
                {
                    return singularCFrame;
                }
            }

            if (!CanCollide) return null;

            // Query only nearby chunks for better performance
            foreach (CFrame singularCFrame in ShootMeUp.GetObstaclesNear(Position.X, Position.Y, Size.Width, Size.Height, expandChunks: 1))
            {
                // Skip the ignored character
                if (singularCFrame == (CFrame)_shotBy)
                    continue;

                // Skip no collision obstacles
                if (singularCFrame is Obstacle obstacle && !obstacle.CanCollide)
                    continue;


                if (ShootMeUp.IsOverlapping(singularCFrame, Position.X + stepX, Position.Y, Size.Width, Size.Height))
                {
                    return singularCFrame;
                }

                if (ShootMeUp.IsOverlapping(singularCFrame, Position.X, Position.Y + stepY, Size.Width, Size.Height))
                {
                    return singularCFrame;
                }
            }
            
            return null;
        }
    }
}