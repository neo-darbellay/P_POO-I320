using ShootMeUp.Helpers;
using ShootMeUp.Properties;
using System.Numerics;

namespace ShootMeUp.Model
{
    /// <summary>
    /// The enemy class, with more attributes than the regular character
    /// </summary>
    public class Enemy : Character
    {
        /// <summary>
        /// A reference to the GAMESPEED readonly variable
        /// </summary>
        private int _GAMESPEED;

        /// <summary>
        /// Whether or not the enemy can shoot
        /// </summary>
        private bool _blnShoots;

        /// <summary>
        /// The enemy's projectile type
        /// </summary>
        private string _strProjectileType;

        /// <summary>
        /// The score that the enemy gives when it dies
        /// </summary>
        private int _intScore;

        /// <summary>
        /// The time until the enemy's next update
        /// </summary>
        private DateTime _nextUpdateTime = DateTime.MinValue;

        /// <summary>
        /// How long the cooldown lasts after damaging a player
        /// </summary>
        private TimeSpan DamageCooldown = TimeSpan.FromSeconds(5);

        /// <summary>
        /// A player handler to check for collisions
        /// </summary>
        private CharacterHandler _characterHandler;

        /// <summary>
        /// A projectile handler to store every projectile
        /// </summary>
        private ProjectileHandler _projectileHandler;

        /// <summary>
        /// The score that the enemy gives when it dies
        /// </summary>
        public int Score
        {
            get { return _intScore; }
        }

        /// <summary>
        /// The shooting enemy's constructor
        /// </summary>
        /// <param name="x">Its starting X position</param>
        /// <param name="y">Its starting Y position</param>
        /// <param name="length">The length of the character</param>
        /// <param name="strType">The character's type (player, zombie, skeleton, ...)</param>
        /// <param name="GAMESPEED">The game's speed</param>
        public Enemy(int x, int y, int length, string strType, int GAMESPEED) : base(x, y, length, strType, GAMESPEED)
        {
            _GAMESPEED = GAMESPEED;

            // Set the default values up, before changing them depending on the enemy type
            _intScore = 0;
            _intHealth = 0;
            _fltBaseSpeed = 0;
            _strProjectileType = "";
            _blnShoots = false;

            // Set up the enemy depending on the current type
            SetupEnemy(strType);

            _characterHandler = new CharacterHandler();
            _projectileHandler = new ProjectileHandler();
            
            DamageCooldown = TimeSpan.FromSeconds(DamageCooldown.TotalSeconds / GAMESPEED);
            ArrowCooldown = TimeSpan.FromSeconds(20 / GAMESPEED);
            FireballCooldown = TimeSpan.FromSeconds(20 / GAMESPEED);
            _lastArrowShotTime = DateTime.Now;
            _lastFireballShotTime = DateTime.Now;
        }

        /// <summary>
        /// Sets up the current enemy
        /// </summary>
        /// <param name="strType"></param>
        private void SetupEnemy(string strType)
        {
            switch (strType)
            {
                case "zombie":
                    _intScore = 1;
                    _intHealth = 10;
                    _fltBaseSpeed = 2f / 5f;

                    break;
                case "skeleton":
                    _intScore = 3;
                    _intHealth = 5;
                    _fltBaseSpeed = -0.5f;
                    _blnShoots = true;
                    _strProjectileType = "arrow";
                    
                    break;
                case "babyzombie":
                    _intScore = 2;
                    _intHealth = 3;
                    _fltBaseSpeed = 1.5f;

                    DamageCooldown = TimeSpan.FromSeconds(3);

                    break;
                case "blaze":
                    _intScore = 5;
                    _intHealth = 10;
                    _fltBaseSpeed = -0.25f;
                    _blnShoots = true;
                    _strProjectileType = "fireball";

                    break;
                case "zombiepigman":
                    _intScore = 5;
                    _intHealth = 20;
                    _fltBaseSpeed = 1f / 5f;

                    DamageCooldown = TimeSpan.FromSeconds(8);

                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Update the enemy's position
        /// </summary>
        override public void Update()
        {

            // Add the base class' update
            base.Update();

            // Only deal contact damage if the enemy isn't a shooter
            if (!_blnShoots)
            {
                // Skip the attack check if the enemy is on damage cooldown
                if (DateTime.Now < _nextUpdateTime && !_blnShoots)
                    return;

                // Get the current CFrame
                CFrame currentCFrame = (CFrame)this;

                // Get the character or obstacle in front of the enemy
                Character? characterHit = _characterHandler.GetCollidingCharacter(currentCFrame, 0, 0, this, "player");
                Obstacle? obstacleHit = _colCollisionHandler.GetCollidingObject(currentCFrame, _fltXSpeed, _fltYSpeed);

                // Set the cooldown to the next update if there's anything in front of the enemy
                if (characterHit != null || (obstacleHit != null && !obstacleHit.Invincible))
                {
                    // Set the cooldown to the next update
                    _nextUpdateTime = DateTime.Now + DamageCooldown;
                }

                // Deal damage to the player or the obstacle in front of the enemy
                if (characterHit != null)
                {
                    // Damage the player
                    DamagePlayer(characterHit);

                }
                else if (obstacleHit != null && !obstacleHit.Invincible)
                {
                    DamageObstacle(obstacleHit);

                    // Set the cooldown to the next update
                    _nextUpdateTime = DateTime.Now + DamageCooldown;
                }
            }
            else
            {
                // Skip the update if the enemy is on damage cooldown
                if ((_strProjectileType == "arrow" && DateTime.Now - _lastArrowShotTime < ArrowCooldown) || (_strProjectileType == "fireball" && DateTime.Now - _lastFireballShotTime < FireballCooldown))
                    return;

                // Get the player
                Character? player = _characterHandler.Characters.Find(character => character.Type == "player");

                // Stop trying to shoot if the player doesn't exist
                if (player == null)
                    return;

                // Shoot an arrow using the enemy's shoot method and add it to the projetile list
                Projectile? possibleProjectile = Shoot(new Point(player.X, player.Y), _strProjectileType, _GAMESPEED);
                //
                if (possibleProjectile != null)
                {
                    _projectileHandler.Projectiles.Add(possibleProjectile);

                    // Record the shot time
                    if (_strProjectileType == "arrow")
                        _lastArrowShotTime = DateTime.Now;
                    else if (_strProjectileType == "fireball")
                        _lastFireballShotTime = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// Move the enemy to the player
        /// </summary>
        /// <param name="player"></param>
        public void Move(Character player)
        {
            // Calculate direction to target
            float deltaX = player.FloatX - FloatX;
            float deltaY = player.FloatY - FloatY;

            // Normalize direction
            float length = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            // Divide the delta positions by the length if it isn't equal to 0
            if (length != 0)
            {
                deltaX /= length;
                deltaY /= length;
            }

            // Multiply the movement variables to match the game speed
            deltaX *= _GAMESPEED;
            deltaY *= _GAMESPEED;

            base.Move(deltaX, deltaY);
        }


        override public Projectile? Shoot(Point clientPos, string strType, int GAMESPEED)
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

                // Get the enemy's center
                float fltEnemyCenterX = FloatX + (length / 2f);
                float fltEnemyCenterY = FloatY + (height / 2f);

                // The projectile should start centered on the enemy
                fltProjectileX = fltEnemyCenterX;
                fltProjectileY = fltEnemyCenterY - (intProjectileHeight / 2f);

                // Resize the projectile if the aspect ratio is different
                if (strType == "arrow")
                {
                    // 8:29 aspect ratio
                    intProjectileLength = (intProjectileHeight * 8) / 29;
                }
                else if (strType == "fireball")
                {
                    // Make the fireball smaller
                    intProjectileHeight /= 2;
                    intProjectileLength /= 2;
                }

                // Slow the projectile down by dividing its GAMESPEED reference by 2
                int intFakeGameSpeed = GAMESPEED/2;

                // Fire a new projectile if possible
                return new Projectile(strType, fltProjectileX, fltProjectileY, intProjectileLength, intProjectileHeight, this, intTargetX, intTargetY, intFakeGameSpeed);
            }

            return null;
        }

        /// <summary>
        /// Damages the player
        /// </summary>
        /// <param name="player">The player</param>
        public void DamagePlayer(Character player)
        {
            player.Lives -= 1;
        }

        /// <summary>
        /// Damage an obstacle
        /// </summary>
        /// <param name="obstacle">The obstacle</param>
        public void DamageObstacle(Obstacle obstacle)
        {
            obstacle.Health -= 1;
        }


        public override void Render(BufferedGraphics drawingSpace)
        {
            if (Lives > 0)
            {
                switch (_strType)
                {
                    case "skeleton":
                        drawingSpace.Graphics.DrawImage(Resources.EnemySkeleton, FloatX, FloatY, length, height);

                        break;
                    case "babyzombie":
                    case "zombie":
                        drawingSpace.Graphics.DrawImage(Resources.EnemyZombie, FloatX, FloatY, length, height);

                        break;
                    case "blaze":
                        drawingSpace.Graphics.DrawImage(Resources.EnemyBlaze, FloatX, FloatY, length, height);

                        break;
                    case "zombiepigman":
                        drawingSpace.Graphics.DrawImage(Resources.EnemyZombiePigman, FloatX, FloatY, length, height);

                        break;
                    default:
                        break;
                }

                // Get the text's size
                SizeF textSize = drawingSpace.Graphics.MeasureString($"{this}", TextHelpers.drawFont);

                // Calculate the X coordinate to center the text
                float centeredX = FloatX + (length / 2f) - (textSize.Width / 2f);

                // Center the text above the obstacle
                drawingSpace.Graphics.DrawString($"{this}", TextHelpers.drawFont, TextHelpers.writingBrush, centeredX, FloatY - 16);
            }
        }

    }
}
