using ShootMeUp.Helpers;
using ShootMeUp.Properties;
using System.Numerics;

namespace ShootMeUp.Model
{
    /// <summary>
    /// The base class for characters.
    /// </summary>
    public class Character : CFrame
    {
        /// <summary>
        /// The character's health
        /// </summary>
        private int _intHealth;

        /// <summary>
        /// The character's speed in the X direction
        /// </summary>
        protected float _fltXSpeed;

        /// <summary>
        /// The character's speed in the Y direction
        /// </summary>
        protected float _fltYSpeed;

        /// <summary>
        /// The character's type (player, ...)
        /// </summary>
        private string _strType;

        /// <summary>
        /// The character's base speed
        /// </summary>
        protected float _fltBaseSpeed;

        /// <summary>
        /// A collision handler to check for collisions
        /// </summary>
        protected CollisionHandler _colCollisionHandler;        
        
        /// <summary>
        /// The character's remaining lives
        /// </summary>
        public int Lives
        {
            get { return _intHealth; }
            set { _intHealth = value; }
        }

        // <summary>
        /// The character's type (player, ...)
        /// </summary>
        public string Type 
        {
            get { return _strType; }
        }

        /// <summary>
        /// The character's base speed
        /// </summary>
        public float BaseSpeed
        {
            get { return _fltBaseSpeed; }
        }

        // Variables used for projectile cooldown
        protected DateTime _lastArrowShotTime = DateTime.MinValue;
        protected DateTime _lastFireballShotTime = DateTime.MinValue;

        protected TimeSpan ArrowCooldown = TimeSpan.FromSeconds(1);
        protected TimeSpan FireballCooldown = TimeSpan.FromSeconds(3);


        /// <summary>
        /// The character's constructor
        /// </summary>
        /// <param name="x">Its starting X position</param>
        /// <param name="y">Its starting Y position</param>
        /// <param name="length">The length of the character</param>
        /// <param name="height">The height of the character</param>
        /// <param name="strType">The character's type (player, zombie, skeleton, ...)</param>
        /// <param name="intHealth">The character's HP / lives</param>
        /// <param name="fltBaseSpeed">The base character speed</param>
        /// <param name="GAMESPEED">The game's speed</param>
        public Character(int x, int y, int length, int height, string strType, int intHealth, float fltBaseSpeed, int GAMESPEED) : base(x, y, length, height)
        {
            _intHealth = intHealth;
            _colCollisionHandler = new CollisionHandler();
            _strType = strType;
            _fltBaseSpeed = fltBaseSpeed;

            ArrowCooldown = TimeSpan.FromSeconds(ArrowCooldown.TotalSeconds / GAMESPEED);
            FireballCooldown = TimeSpan.FromSeconds(FireballCooldown.TotalSeconds / GAMESPEED);
        }

        /// <summary>
        /// Update the character's position
        /// </summary>
        virtual public void Update()
        {
            // Variable used for multiplying the speed of the movement
            double dblMultiplicator = 1;

                       
            // Get the current CFrame
            CFrame currentCFrame = (CFrame)this;

            // Check to see if the character is gonna clip in anything
            bool[] tab_blnColliding = _colCollisionHandler.CheckForCollisions(currentCFrame, _fltXSpeed, _fltYSpeed);
            

            // Change the multiplicator for double-axis movement
            if (_fltXSpeed != 0 && _fltYSpeed != 0)
            {
                dblMultiplicator = 0.7;           
            }

            // Use the speed variables to change the character's position if the requirements are met.
            if (!tab_blnColliding[0])
                FloatX += (float)(_fltXSpeed * dblMultiplicator);
            
            if (!tab_blnColliding[1])
                FloatY += (float)(_fltYSpeed * dblMultiplicator);
        }

        /// <summary>
        /// Move the character on both axis if they're alive
        /// </summary>
        /// <param name="x">The movement on the x axis</param>
        /// <param name="y">The movement on the y axis</param>
        public void Move(float x, float y)
        {
            if (Lives > 0)
            {
                _fltXSpeed = x * BaseSpeed;
                _fltYSpeed = y * BaseSpeed;
            }
        }

        virtual public Projectile? Shoot(Point clientPos, string strType, int GAMESPEED)
        {
            // Store the current time
            DateTime now = DateTime.Now;

            // Shoot an arrow from the player's position to the cursor's position if they are alive
            if (Lives > 0)
            {
                // Create variables used for the projectile's generation
                float fltProjectileX = FloatX;
                float fltProjectileY = FloatY;

                int intTargetX = clientPos.X;
                int intTargetY = clientPos.Y;

                int intProjectileLength = length;
                int intProjectileHeight = height;

                // Get the character's center
                float fltCharacterCenterX = FloatX + (length / 2f);
                float fltCharacterCenterY = FloatY + (height / 2f);

                // The projectile should start centered on the character
                fltProjectileX = fltCharacterCenterX;
                fltProjectileY = fltCharacterCenterY - (intProjectileHeight / 2f);

                // Resize the projectile if the aspect ratio is different
                if (strType == "arrow")
                {
                    // 8:29 aspect ratio
                    intProjectileLength = (intProjectileHeight * 8) / 29;
                }

                // Send the corresponding projectile if the character is allowed to
                if (strType == "arrow" && now - _lastArrowShotTime >= ArrowCooldown)
                {
                    _lastArrowShotTime = now;

                    return new Projectile(strType, fltProjectileX, fltProjectileY, intProjectileLength, intProjectileHeight, this, intTargetX, intTargetY, GAMESPEED);
                }
                else if (strType == "fireball" && now - _lastFireballShotTime >= FireballCooldown)
                {
                    _lastFireballShotTime = now;

                    return new Projectile(strType, fltProjectileX, fltProjectileY, intProjectileLength, intProjectileHeight, this, intTargetX, intTargetY, GAMESPEED);
                }
            }

            return null;
        }

        public void Render(BufferedGraphics drawingSpace)
        {
            // Only draw the character if they're alive
            if (Lives > 0)
            {
                if (_strType == "player")
                    drawingSpace.Graphics.DrawImage(Resources.PlayerToken, FloatX, FloatY, length, height);
                else if (_strType == "zombie")
                    drawingSpace.Graphics.DrawImage(Resources.ZombieToken, FloatX, FloatY, length, height);
                else if (_strType == "skeleton")
                    drawingSpace.Graphics.DrawImage(Resources.SkeletonToken, FloatX, FloatY, length, height);
            }

            // Only draw the lives if the character is a player
            if (_strType == "player")
            {
                for (int i = 0; i < Lives; i++)
                {
                    // Draw the PlayerToken as many times as there are lives
                    drawingSpace.Graphics.DrawImage(Resources.PlayerToken, (16 * i) + (6 * i) + 6, 6, 16, 16);
                }
            }
            else
            {
                drawingSpace.Graphics.DrawString($"{this}", TextHelpers.drawFont, TextHelpers.writingBrush, X, Y - 25);
            }
        }

        public override string ToString()
        {

            return $"{((int)((double)Lives)).ToString()}HP";
        }
    }
}
