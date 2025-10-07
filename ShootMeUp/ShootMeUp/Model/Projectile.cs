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

        public Projectile(string strType, int X, int Y, int intLength, int intHeight, Character ShotBy, int intTargetX, int intTargetY) : base(X, Y, intLength, intHeight, true)
        {
            _strType = strType;
            _shotBy = ShotBy;
            _intTargetX = intTargetX;
            _intTargetY = intTargetY;

            // Define the different properties depending on the projectile type
            if (_strType == "arrow")
            {
                _intDamage = 1;
                _fltMovementSpeed = 3f;
            }
            else if (_strType == "fireball")
            {
                _intDamage = 2;
                _fltMovementSpeed = 1.5f;
            }
            else
            {
                // Default
                _intDamage = 0;
                _fltMovementSpeed = 3f;
            }

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
            FloatX += _fltXSpeed;
            FloatY += _fltYSpeed;
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
