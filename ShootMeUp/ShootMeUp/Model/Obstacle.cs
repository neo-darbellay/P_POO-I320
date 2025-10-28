using ShootMeUp.Helpers;
using ShootMeUp.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShootMeUp.Model
{
    /// <summary>
    /// A basic obstacle. you can give it a size, position, and health (if none then it's invincible).
    /// </summary>
    public class Obstacle : CFrame
    {
        /// <summary>
        /// The obstacle's max health
        /// </summary>
        private int _intMaxHealth;

        /// <summary>
        /// The obstacle's health
        /// </summary>
        private int _intHealth;

        /// <summary>
        /// Whether the obstacle is invincible or not
        /// </summary>
        private bool _blnInvincible;

        /// <summary>
        /// The obstacle's type (border, ...)
        /// </summary>
        private string _strType;

        /// <summary>
        /// Whether the obstacle has collisions or not
        /// </summary>
        private bool _blnCanCollide;

        /// <summary>
        /// The obstacle's health (set to int.MaxValue if invincible)
        /// </summary>
        public int Health
        {
            get { return _intHealth; }
            set { _intHealth = value; }
        }

        /// <summary>
        /// Whether the obstacle has collisions or not
        /// </summary>
        public bool HasCollisions
        {
            get { return _blnCanCollide; }
        }


        /// <summary>
        /// Whether the obstacle is invincible or not
        /// </summary>
        public bool Invincible
        {
            get { return _blnInvincible; }
        }

        /// <summary>
        /// The obstacle constructor
        /// </summary>
        /// <param name="x">The obstacle's X pos</param>
        /// <param name="y">The obstacle's Y pos</param>
        /// <param name="intLength">The obstacle's Length</param>
        /// <param name="intHeight">The obstacle's height</param>
        /// <param name="intHealth">The obstacle's max health</param>
        public Obstacle(int x, int y, int intLength, int intHeight, int intHealth) : base(x, y, intLength, intHeight)
        {
            if (intHealth == 0)
            {
                _blnInvincible = true;
                _intHealth = int.MaxValue;
            }
            else
            {
                _blnInvincible = false;
                _intHealth = intHealth;
            }

            _intMaxHealth = _intHealth;
            _strType = "default";
            _blnCanCollide = true;
        }

        /// <summary>
        /// The obstacle constructor
        /// </summary>
        /// <param name="x">The obstacle's X pos</param>
        /// <param name="y">The obstacle's Y pos</param>
        /// <param name="intLength">The obstacle's Length</param>
        /// <param name="intHealth">The obstacle's max health</param>
        public Obstacle(int x, int y, int intLength, int intHealth) : base(x, y, intLength)
        {
            if (intHealth == 0)
            {
                _blnInvincible = true;
                _intHealth = int.MaxValue;
            }
            else
            {
                _blnInvincible = false;
                _intHealth = intHealth;
            }

            _intMaxHealth = _intHealth;
            _strType = "default";
            _blnCanCollide = true;
        }



        /// <summary>
        /// The obstacle constructor
        /// </summary>
        /// <param name="x">The obstacle's X pos</param>
        /// <param name="y">The obstacle's Y pos</param>
        /// <param name="intLength">The obstacle's Length</param>
        /// <param name="intHeight">The obstacle's height</param>
        /// <param name="intHealth">The obstacle's max health</param>
        /// <param name="strType">The obstacle's type (border/default)</param>
        public Obstacle(int x, int y, int intLength, int intHeight, int intHealth, string strType) : base(x, y, intLength, intHeight)
        {
            if (intHealth == 0)
            {
                _blnInvincible = true;
                _intHealth = int.MaxValue;
            }
            else
            {
                _blnInvincible = false;
                _intHealth = intHealth;
            }

            _intMaxHealth = _intHealth;
            _strType = strType;

            if (strType == "spawner")
                _blnCanCollide = false;
            else
                _blnCanCollide = true;
        }

        /// <summary>
        /// The obstacle constructor
        /// </summary>
        /// <param name="x">The obstacle's X pos</param>
        /// <param name="y">The obstacle's Y pos</param>
        /// <param name="intLength">The obstacle's Length</param>
        /// <param name="intHealth">The obstacle's max health</param>
        /// <param name="strType">The obstacle's type (border/default)</param>
        public Obstacle(int x, int y, int intLength, int intHealth, string strType) : base(x, y, intLength)
        {
            if (intHealth == 0)
            {
                _blnInvincible = true;
                _intHealth = int.MaxValue;
            }
            else
            {
                _blnInvincible = false;
                _intHealth = intHealth;
            }

            _intMaxHealth = _intHealth;
            _strType = strType;

            if (strType == "spawner")
                _blnCanCollide = false;
            else
                _blnCanCollide = true;
        }

        public void Render(BufferedGraphics drawingSpace)
        {
            if (_strType == "default")
            {
                if (_blnInvincible)
                {
                    drawingSpace.Graphics.DrawImage(Resources.ObstacleUnbreakable, FloatX, FloatY, length, height);
                    _intHealth = int.MaxValue;
                }
                else if (_intMaxHealth > 10)
                {
                    drawingSpace.Graphics.DrawImage(Resources.ObstacleStrong, FloatX, FloatY, length, height);
                }
                else if (_intMaxHealth > 5)
                {
                    drawingSpace.Graphics.DrawImage(Resources.ObstacleNormal, FloatX, FloatY, length, height);
                }
                else
                {
                    drawingSpace.Graphics.DrawImage(Resources.ObstacleWeak, FloatX, FloatY, length, height);
                }

                // Get the text's size
                SizeF textSize = drawingSpace.Graphics.MeasureString($"{this}", TextHelpers.drawFont);

                // Calculate the X coordinate to center the text
                float centeredX = FloatX + (length / 2f) - (textSize.Width/ 2f);

                // Center the text above the obstacle
                drawingSpace.Graphics.DrawString($"{this}", TextHelpers.drawFont, TextHelpers.writingBrush, centeredX, FloatY - 16);
            }
            else if (_strType == "spawner")
            {
                drawingSpace.Graphics.DrawImage(Resources.ObstacleSpawner, FloatX, FloatY, length, height);
                _intHealth = int.MaxValue;
            }
            else if (_strType == "border")
            {
                drawingSpace.Graphics.DrawImage(Resources.ObstacleBorder, FloatX, FloatY, length, height);
                _intHealth = int.MaxValue;
            }

        }

        public override string ToString()
        {
            if (_blnInvincible)
            {
                return "";
            }

            if (_intHealth > 0)
                return $"{((int)((double)_intHealth)).ToString()} HP";
            else
                return "";
        }
    }
}
