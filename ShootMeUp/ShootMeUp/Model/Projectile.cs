using ShootMeUp.Properties;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShootMeUp.Model
{
    public class Projectile : CFrame
    {
        /// <summary>
        /// The projectile's type (arrow, ...)
        /// </summary>
        private string _strType;

        /// <summary>
        /// The character that shot the projectile
        /// </summary>
        private Character _shotBy;

        /// <summary>
        /// The amount of damage the projectile deals (in HP)
        /// </summary>
        private int _intDamage;

        /// <summary>
        /// Whether or not the projectile is active or not
        /// </summary>
        private bool _blnActive;

        /// <summary>
        /// The rotation angle (in degrees)
        /// </summary>
        private float _fltRotationAngle;

        /// <summary>
        /// The projectile's movement speed
        /// </summary>
        private float _fltMovementSpeed;

        /// <summary>
        ///  The projectile's speed in the X axis
        /// </summary>
        private float _fltXSpeed;

        /// <summary>
        /// The projectile's speed in the Y axis
        /// </summary>
        private float _fltYSpeed;

        /// <summary>
        /// The X position of the target
        /// </summary>
        private int _intTargetX;

        /// <summary>
        /// The Y position of the target
        /// </summary>
        private int _intTargetY;

        /// <summary>
        /// A character handler to store every character
        /// </summary>
        private CharacterHandler _characterHandler;

        /// <summary>
        /// A collision handler to create obstacles
        /// </summary>
        private CollisionHandler _collisionHandler;

        public bool Active
        {
            get { return _blnActive; }
        }


        public Projectile(string strType, float X, float Y, int intLength, int intHeight, Character ShotBy, int intTargetX, int intTargetY, int GAMESPEED) : base(X, Y, intLength, intHeight)
        {
            _strType = strType;
            _shotBy = ShotBy;
            _intTargetX = intTargetX;
            _intTargetY = intTargetY;

            _blnActive = true;

            _characterHandler = new CharacterHandler();
            _collisionHandler = new CollisionHandler();

            // Define the different properties depending on the projectile type
            if (_strType == "arrow")
            {
                _intDamage = 1;
                _fltMovementSpeed = 3f;
            }
            else if (_strType == "fireball")
            {
                _intDamage = 3;
                _fltMovementSpeed = 1f;
            }
            else
            {
                // Default
                _intDamage = 0;
                _fltMovementSpeed = 3f;
            }

            // Multiply the movement speed by the game speed
            _fltMovementSpeed *= GAMESPEED;

            // Calculate direction to target
            float deltaX = _intTargetX - FloatX;
            float deltaY = _intTargetY - FloatY;

            // Calculate rotation angle in degrees
            // We add 90 here because the image faces upwards
            _fltRotationAngle = (float)(Math.Atan2(deltaY, deltaX) * (180.0 / Math.PI)) + 90;

            // Normalize direction
            float length = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
            if (length != 0)
            {
                deltaX /= length;
                deltaY /= length;
            }

            // Store movement speed in X/Y components
            _fltXSpeed = (deltaX * _fltMovementSpeed);
            _fltYSpeed = (deltaY * _fltMovementSpeed);
        }

        /// <summary>
        /// Update the projectile's position
        /// </summary>
        public void Update()
        {
            // Get the current CFrame
            CFrame currentCFrame = (CFrame)this;

            // Check to see if the projectile is gonna clip in anything
            bool[] tab_blnCharacterColliding;

            if (_shotBy.Type == "player")
            {
                tab_blnCharacterColliding = _characterHandler.CheckForCollisions(currentCFrame, _fltXSpeed, _fltYSpeed, _shotBy);
            }
            else
            {
                tab_blnCharacterColliding = _characterHandler.CheckForCollisions(currentCFrame, _fltXSpeed, _fltYSpeed, _shotBy, "player");
            }

            bool[] tab_blnObstaclesColliding = _collisionHandler.CheckForCollisions(currentCFrame, _fltXSpeed, _fltYSpeed);

            // Move the arrow if it wouldn't hit anything
            if (!(tab_blnCharacterColliding[0] || tab_blnCharacterColliding[1] || tab_blnObstaclesColliding[0] || tab_blnObstaclesColliding[1]))
            {
                FloatX += _fltXSpeed;
                FloatY += _fltYSpeed;
            }
            else
            {
                // Mark the projectile as inactive
                _blnActive = false;

                // Get the object and/or character that's been hit
                Character? characterHit;

                if (_shotBy.Type == "player")
                {
                    characterHit = _characterHandler.GetCollidingCharacter(currentCFrame, _fltXSpeed, _fltYSpeed, _shotBy);
                }
                else
                {
                    characterHit = _characterHandler.GetCollidingCharacter(currentCFrame, _fltXSpeed, _fltYSpeed, _shotBy, "player");
                }
                Obstacle? obstacleHit = _collisionHandler.GetCollidingObject(currentCFrame, _fltXSpeed, _fltYSpeed);

                if (characterHit != null)
                {
                    // Deal damage to the character
                    characterHit.Lives -= _intDamage;
                }
                else if (obstacleHit != null && !obstacleHit.Invincible)
                {
                    // Deal damage to the obstacle
                    obstacleHit.Health -= _intDamage;
                }
            }

        }


        public void Render(BufferedGraphics drawingSpace)
        {
            Image? imgProjectile = null;

            if (_strType == "arrow")
                imgProjectile = Resources.ProjectileArrow;
            else if (_strType == "fireball")
                imgProjectile = Resources.ProjectileFireball;

            if (imgProjectile == null)
                return;

            // Save current transform
            GraphicsState state = drawingSpace.Graphics.Save();

            // Move origin to center of projectile
            drawingSpace.Graphics.TranslateTransform(FloatX + length / 2f, FloatY + height / 2f);

            // Rotate around center
            drawingSpace.Graphics.RotateTransform(_fltRotationAngle);

            // Draw image centered at new origin
            drawingSpace.Graphics.DrawImage(imgProjectile, -length / 2f, -height / 2f, length, height);

            // Restore transform
            drawingSpace.Graphics.Restore(state);
        }
    }
}
