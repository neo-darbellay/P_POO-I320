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
        private int _intSpeedX;

        /// <summary>
        /// The character's speed in the Y direction
        /// </summary>
        private int _intSpeedY;

        /// <summary>
        /// The character's type (player, ...)
        /// </summary>
        private string _strType;

        /// <summary>
        /// A collision handler to check for collisions
        /// </summary>
        private CollisionHandler _colCollisionHandler;        
        
        /// <summary>
        /// The character's remaining lives
        /// </summary>
        public int Lives
        {
            get { return _intHealth; }
            set { _intHealth = value; }
        }

        // VARIABLES USED FOR PROJECTILE COOLDOWN
        private DateTime _lastArrowShotTime = DateTime.MinValue;
        private DateTime _lastFireballShotTime = DateTime.MinValue;

        private readonly TimeSpan ArrowCooldown = TimeSpan.FromSeconds(0.5);
        private readonly TimeSpan FireballCooldown = TimeSpan.FromSeconds(3);


        /// <summary>
        /// The character's constructor
        /// </summary>
        /// <param name="x">Its starting X position</param>
        /// <param name="y">Its starting Y position</param>
        /// <param name="length">The length of the character</param>
        /// <param name="height">The height of the character</param>
        /// <param name="strType">The character's type (player, zombie, ...)</param>
        /// <param name="intHealth">The character's HP / lives</param>
        public Character(int x, int y, int length, int height, string strType, int intHealth) : base(x, y, length, height)
        {
            _intHealth = intHealth;
            _colCollisionHandler = new CollisionHandler();
            _strType = strType;
        }

        // Cette méthode calcule le nouvel état dans lequel le drone se trouve après
        // que 'interval' millisecondes se sont écoulées

        /// <summary>
        /// Update the character's position
        /// </summary>
        public void Update()
        {
            // Variable used for multiplying the speed of the movement
            double dblMultiplicator = 1;

                       
            // Get the current CFrame
            CFrame currentCFrame = (CFrame)this;

            // Check to see if the character is gonna clip in anything
            bool[] tab_blnColliding = _colCollisionHandler.CheckForCollisions(currentCFrame, _intSpeedX, _intSpeedY);
            

            // Change the multiplicator for double-axis movement
            if (_intSpeedX != 0 && _intSpeedY != 0)
            {
                dblMultiplicator = 0.7;           
            }

            // Use the speed variables to change the character's position if the requirements are met.
            if (!tab_blnColliding[0])
            {
                X += (int)(_intSpeedX * dblMultiplicator);
            }
            
            if (!tab_blnColliding[1])
            {
                Y += (int)(_intSpeedY * dblMultiplicator);
            }
        }

        /// <summary>
        /// Move the character on both axis
        /// </summary>
        /// <param name="x">The movement on the x axis</param>
        /// <param name="y">The movement on the y axis</param>
        public void Move(int x, int y)
        {
            _intSpeedX = x;
            _intSpeedY = y;
        }

        public Projectile? Shoot(Point clientPos, string strType)
        {
            // Store the current time
            DateTime now = DateTime.Now;

            // Shoot an arrow from the player's position to the cursor's position if they are alive
            if (Lives > 0)
            {
                // Create variables used for the projectile's generation
                int intProjectileX = X;
                int intProjectileY = Y;

                int intTargetX = clientPos.X;
                int intTargetY = clientPos.Y;

                int intProjectileLength = length;
                int intProjectileHeight = height;

                if (strType == "arrow")
                {
                    intProjectileX += (length / 8);
                    intProjectileY += (height / 8);
                }

                else if (strType == "fireball")
                {
                    intProjectileX += (length / 8);
                    intProjectileY += (height / 8);
                }

                if (strType == "arrow" && now - _lastArrowShotTime >= ArrowCooldown)
                {
                    _lastArrowShotTime = now;

                    return new Projectile(strType, intProjectileX, intProjectileY, intProjectileLength, intProjectileHeight, this, intTargetX, intTargetY);
                }
                else if (strType == "fireball" && now - _lastFireballShotTime >= FireballCooldown)
                {
                    _lastFireballShotTime = now;

                    return new Projectile(strType, intProjectileX, intProjectileY, intProjectileLength, intProjectileHeight, this, intTargetX, intTargetY);
                }

            }

            return null;
        }

        /// //////////////////////////////////////////////////////////////////////////////
        //  
        //  Ce qui suit appartient à la vue, pas au modèle.
        //  Il aurait été préférable de séparer la déclaration de la classe Drone en deux,
        //  Nous regroupons tout ici pour simplifier
        //  
        /// //////////////////////////////////////////////////////////////////////////////

        private Pen droneBrush = new Pen(new SolidBrush(Color.Purple), 3);

        // De manière graphique
        public void Render(BufferedGraphics drawingSpace)
        {
            if (_strType == "player")
                drawingSpace.Graphics.DrawImage(Resources.PlayerToken, X, Y, length, height);
            else if (_strType == "zombie")
                drawingSpace.Graphics.DrawImage(Resources.ZombieToken, X, Y, length, height);
            
            
            

            for (int i = 0; i < Lives; i++)
            {
                // Draw the PlayerToken as many times as there are lives

                drawingSpace.Graphics.DrawImage(Resources.PlayerToken, ((length / 2) * i) + (6 * i) + 6, 6, length / 2, height / 2);
            }
        }

        public override string ToString()
        {
            return $"{((int)((double)Lives)).ToString()}HP";
        }
    }
}
