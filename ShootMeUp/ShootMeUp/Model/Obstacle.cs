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
        /// The object's type (border, ...)
        /// </summary>
        private string _strType;

        /// <summary>
        /// The obstacle's health (set to int.MaxValue if invincible)
        /// </summary>
        public int Health
        {
            get { return _intHealth; }
            set { _intHealth = value; }
        }


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
        }

        public void Render(BufferedGraphics drawingSpace)
        {
            if (_strType == "default")
            {
                if (_blnInvincible)
                {
                    drawingSpace.Graphics.DrawImage(Resources.ObstacleUnbreakable, X, Y, length, height);
                }
                else if (_intMaxHealth >= 5)
                {
                    drawingSpace.Graphics.DrawImage(Resources.ObstacleStrong, X, Y, length, height);
                }
                else
                {
                    drawingSpace.Graphics.DrawImage(Resources.ObstacleWeak, X, Y, length, height);
                }

                drawingSpace.Graphics.DrawString($"{this}", TextHelpers.drawFont, TextHelpers.writingBrush, X, Y - 25);
            }
            else if (_strType == "border")
            {
                drawingSpace.Graphics.DrawImage(Resources.ObstacleBorder, X, Y, length, height);
                _intHealth = int.MaxValue;
            }

        }

        public override string ToString()
        {
            if (_blnInvincible)
            {
                return "Invincible";
            }

            return $"{((int)((double)_intHealth)).ToString()}HP";
        }
    }
}
