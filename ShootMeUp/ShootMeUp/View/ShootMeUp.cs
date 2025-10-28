using Accessibility;
using ShootMeUp.Helpers;
using ShootMeUp.Model;
using System.Drawing;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;

namespace ShootMeUp
{
    /// <summary>
    /// The ShootMeUp class is used to serve as a 2D playspace for the player and enemies.
    /// </summary>
    public partial class ShootMeUp : Form
    {
        /// <summary>
        /// Width of the game area
        /// </summary>
        public static readonly int WIDTH = 1024;
        
        /// <summary>
        /// Height of the game area
        /// </summary>
        public static readonly int HEIGHT = 1024;

        /// <summary>
        /// The game's speed multiplier (movement, 
        /// </summary>
        public static readonly int GAMESPEED = 6;

        /// <summary>
        /// Any obstacle's height and length
        /// </summary>
        public static readonly int OBSTACLE_SIZE = 32;

        /// <summary>
        /// The default size for characters and enemies
        /// </summary>
        public static readonly int DEFAULT_CHARACTER_SIZE = OBSTACLE_SIZE - 8;

        /// <summary>
        /// The player's max hp
        /// </summary>
        public static readonly int PLAYER_MAXHP = 10;

        /// <summary>
        /// The number of barrier blocks shown on screen
        /// </summary>
        private static int _intBorderSize = 29;

        /// <summary>
        /// The player
        /// </summary>
        private Character _player;

        /// <summary>
        /// The list of keys currently held down
        /// </summary>
        private List<Keys> _lst_keysHeldDown;

        /// <summary>
        /// A character handler to store every character
        /// </summary>
        private CharacterHandler _characterHandler;

        /// <summary>
        /// A collision handler to create obstacles
        /// </summary>
        private CollisionHandler _collisionHandler;

        /// <summary>
        /// A projectile handler to store every projectile
        /// </summary>
        private ProjectileHandler _projectileHandler;

        /// <summary>
        /// The player's score
        /// </summary>
        private int _intScore;

        /// <summary>
        /// The player's score
        /// </summary>
        public int Score
        {
            get { return _intScore; }
            set { _intScore = value; }
        }


        BufferedGraphicsContext currentContext;
        BufferedGraphics playspace;

        // Initialisation de l'espace aérien avec un certain nombre de drones
        public ShootMeUp()
        {
            this.WindowState = FormWindowState.Maximized;

            InitializeComponent();
            ClientSize = new Size(WIDTH, HEIGHT);

            // Gets a reference to the current BufferedGraphicsContext
            currentContext = BufferedGraphicsManager.Current;

            // Creates a BufferedGraphics instance associated with this form, and with
            // dimensions the same size as the drawing surface of the form.
            playspace = currentContext.Allocate(this.CreateGraphics(), this.DisplayRectangle);
            _player = new Character(512, 512, DEFAULT_CHARACTER_SIZE, DEFAULT_CHARACTER_SIZE, "player", PLAYER_MAXHP, 1f, GAMESPEED);

            // Create a new list of keys held down
            _lst_keysHeldDown = new List<Keys>();

            // Create a new CharacterHandler, CollisionHandler and ProjectileHandler
            _characterHandler = new CharacterHandler();
            _collisionHandler = new CollisionHandler();
            _projectileHandler = new ProjectileHandler();

            // Reset the game
            _characterHandler.RemoveAllCharacters();
            _collisionHandler.RemoveAllObstacles();
            Score = 0;

            // Add the player to the character handler
            _characterHandler.AddCharacter(_player);

            // Define the play area size in increments of 32
            _intBorderSize = 29;

            // Create a new border
            for (int x = 0; x <= _intBorderSize; x++)
            {
                for (int y = 0; y <= _intBorderSize; y++)
                {
                    if (x == 0 || x == _intBorderSize || y == 0 || y == _intBorderSize)
                    {
                        // Create a border piece
                        Obstacle obsBorder = new Obstacle(32 * (2 + x), 32 * (2 + y), 32, 32, 0, "border");


                        // Add the border piece to the collision handler
                        _collisionHandler.AddObstacle(obsBorder);
                    }
                }
            }

            // Create a variable to store the border's size
            int intBorderLength = _intBorderSize * 32 + 32;

            //// Creating the world environment ////

            // Top left corner
            _collisionHandler.AddObstacle(new Obstacle(32 * 3, 32 * 3, OBSTACLE_SIZE, OBSTACLE_SIZE, 0));
            _collisionHandler.AddObstacle(new Obstacle(32 * 4, 32 * 3, OBSTACLE_SIZE, OBSTACLE_SIZE, 0));
            _collisionHandler.AddObstacle(new Obstacle(32 * 3, 32 * 4, OBSTACLE_SIZE, OBSTACLE_SIZE, 0));

            // Top right corner
            _collisionHandler.AddObstacle(new Obstacle(intBorderLength, 32 * 3, OBSTACLE_SIZE, OBSTACLE_SIZE, 0));
            _collisionHandler.AddObstacle(new Obstacle(intBorderLength - 32, 32 * 3, OBSTACLE_SIZE, OBSTACLE_SIZE, 0));
            _collisionHandler.AddObstacle(new Obstacle(intBorderLength, 32 * 4, OBSTACLE_SIZE, OBSTACLE_SIZE, 0));

            // Bottom left corner
            _collisionHandler.AddObstacle(new Obstacle(32 * 3, intBorderLength, OBSTACLE_SIZE, OBSTACLE_SIZE, 0));
            _collisionHandler.AddObstacle(new Obstacle(32 * 4, intBorderLength, OBSTACLE_SIZE, OBSTACLE_SIZE, 0));
            _collisionHandler.AddObstacle(new Obstacle(32 * 3, intBorderLength - 32, OBSTACLE_SIZE, OBSTACLE_SIZE, 0));

            // Bottom right corner
            _collisionHandler.AddObstacle(new Obstacle(intBorderLength, intBorderLength, OBSTACLE_SIZE, OBSTACLE_SIZE, 0));
            _collisionHandler.AddObstacle(new Obstacle(intBorderLength   - 32, intBorderLength, OBSTACLE_SIZE, OBSTACLE_SIZE, 0));
            _collisionHandler.AddObstacle(new Obstacle(intBorderLength, intBorderLength - 32, OBSTACLE_SIZE, OBSTACLE_SIZE, 0));


            // The pillars' health value
            int intPillarHealth = 25;

            // Top left pillars
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    _collisionHandler.AddObstacle(new Obstacle(160 + (160*x), 160 + (160*y), OBSTACLE_SIZE*2, OBSTACLE_SIZE*2, intPillarHealth));
                }
            }

            // Top right pillars
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    _collisionHandler.AddObstacle(new Obstacle(intBorderLength - 128 - (160 * x), 160 + (160 * y), OBSTACLE_SIZE * 2, OBSTACLE_SIZE * 2, intPillarHealth));
                }
            }

            // Bottom left pillars
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    _collisionHandler.AddObstacle(new Obstacle(160 + (160 * x), intBorderLength - 128 - (160 * y), OBSTACLE_SIZE * 2, OBSTACLE_SIZE * 2, intPillarHealth));
                }
            }

            // Bottom right pillars
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    _collisionHandler.AddObstacle(new Obstacle(intBorderLength - 128 - (160 * x), intBorderLength - 128 - (160 * y), OBSTACLE_SIZE * 2, OBSTACLE_SIZE * 2, intPillarHealth));
                }
            }


            // The barriers' health
            int intBarrierHealth = 10;

            // Top barriers
            for (int x = 0; x < 2; x++)
            {
                _collisionHandler.AddObstacle(new Obstacle(416 + (128 * x), 192, OBSTACLE_SIZE * 2, OBSTACLE_SIZE, intBarrierHealth));
            }

            _collisionHandler.AddObstacle(new Obstacle(480, 320, OBSTACLE_SIZE * 2, OBSTACLE_SIZE, intBarrierHealth));

            // Left barriers
            for (int x = 0; x < 2; x++)
            {
                _collisionHandler.AddObstacle(new Obstacle(192, 416 + (128 * x), OBSTACLE_SIZE, OBSTACLE_SIZE * 2, intBarrierHealth));
            }

            _collisionHandler.AddObstacle(new Obstacle(320, 480, OBSTACLE_SIZE, OBSTACLE_SIZE * 2, intBarrierHealth));

            // Right barriers
            for (int x = 0; x < 2; x++)
            {
                _collisionHandler.AddObstacle(new Obstacle(intBorderLength - 128, 416 + (128 * x), OBSTACLE_SIZE, OBSTACLE_SIZE * 2, intBarrierHealth));
            }

            _collisionHandler.AddObstacle(new Obstacle(intBorderLength - 256, 480, OBSTACLE_SIZE, OBSTACLE_SIZE * 2, intBarrierHealth));

            // Bottom barriers
            for (int x = 0; x < 2; x++)
            {
                _collisionHandler.AddObstacle(new Obstacle(416 + (128 * x), intBorderLength - 128, OBSTACLE_SIZE * 2, OBSTACLE_SIZE, intBarrierHealth));
            }

            _collisionHandler.AddObstacle(new Obstacle(480, intBorderLength - 256, OBSTACLE_SIZE * 2, OBSTACLE_SIZE, intBarrierHealth));


            // The smaller obstacles' health
            int intSmallObstacleHealth = 5;

            // Top left small obstacle
            _collisionHandler.AddObstacle(new Obstacle(256, 256, OBSTACLE_SIZE, OBSTACLE_SIZE, intSmallObstacleHealth));

            // Top right small obstacle
            _collisionHandler.AddObstacle(new Obstacle(intBorderLength - 192, 256, OBSTACLE_SIZE, OBSTACLE_SIZE, intSmallObstacleHealth));

            // Bottom left small obstacle
            _collisionHandler.AddObstacle(new Obstacle(256, intBorderLength - 192, OBSTACLE_SIZE, OBSTACLE_SIZE, intSmallObstacleHealth));

            // Bottom right small obstacle
            _collisionHandler.AddObstacle(new Obstacle(intBorderLength - 192, intBorderLength - 192, OBSTACLE_SIZE, OBSTACLE_SIZE, intSmallObstacleHealth));

            // Middle small obstacles
            _collisionHandler.AddObstacle(new Obstacle(416, 416, OBSTACLE_SIZE, OBSTACLE_SIZE, intSmallObstacleHealth));
            _collisionHandler.AddObstacle(new Obstacle(intBorderLength - 352, 416, OBSTACLE_SIZE, OBSTACLE_SIZE, intSmallObstacleHealth));
            _collisionHandler.AddObstacle(new Obstacle(416, intBorderLength - 352, OBSTACLE_SIZE, OBSTACLE_SIZE, intSmallObstacleHealth));
            _collisionHandler.AddObstacle(new Obstacle(intBorderLength - 352, intBorderLength - 352, OBSTACLE_SIZE, OBSTACLE_SIZE, intSmallObstacleHealth));


            ////////////////// TESTING //////////////////
            _characterHandler.AddCharacter(new Enemy(544, 544, DEFAULT_CHARACTER_SIZE, DEFAULT_CHARACTER_SIZE, "zombie", 10, 1f/3f, GAMESPEED));
            _characterHandler.AddCharacter(new Enemy(384, 384, DEFAULT_CHARACTER_SIZE, DEFAULT_CHARACTER_SIZE, "skeleton", 3, 0.5f, GAMESPEED, true, "arrow"));
        }

        /// <summary>
        /// Render the playspace along with the player
        /// </summary>
        private void Render()
        {
            playspace.Graphics.Clear(Color.FromArgb(217, 217, 217));

            playspace.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(250, 231, 172)), new Rectangle(64, 64, 32 * _intBorderSize + 32, 32 * _intBorderSize + 32));

            // Loop through all of the obstacles and render them
            foreach (Obstacle obstacle in _collisionHandler.Obstacles)
            {
                obstacle.Render(playspace);
            }

            // Render all the enemies
            foreach (Character character in _characterHandler.Characters)
            {
                if (character.Type == "player")
                {
                    continue;    
                }

                character.Render(playspace);
            }

            // Render the player
            _player.Render(playspace);


            // Render the projectiles if they are active
            foreach (Projectile projectile in _projectileHandler.Projectiles)
            {
                projectile.Render(playspace);
            }

            // Draw the score in the top left
            playspace.Graphics.DrawString($"Score: {Score}", TextHelpers.drawFont, TextHelpers.writingBrush, 8, 8);

            playspace.Render();
        }

        // Calcul du nouvel état après que 'interval' millisecondes se sont écoulées
        private void Update(int interval)
        {
            // Remove any inactive projectiles/obstacles
            _projectileHandler.Projectiles.RemoveAll(projectile => !projectile.Active);
            _collisionHandler.Obstacles.RemoveAll(obstacle => obstacle.Health <= 0);

            // Change the score if there's a dead enemy
            foreach (Character character in _characterHandler.Characters)
            {
                if (character.Lives <= 0 && character is Enemy enemy)
                {
                    Score += enemy.Score;
                }
            }

            _characterHandler.Characters.RemoveAll(character => character.Lives <= 0);

            // Create movement-related boolean variables
            bool blnLeftHeld = _lst_keysHeldDown.Contains(Keys.A) || _lst_keysHeldDown.Contains(Keys.Left);
            bool blnRightHeld = _lst_keysHeldDown.Contains(Keys.D) || _lst_keysHeldDown.Contains(Keys.Right);
            bool blnUpHeld = _lst_keysHeldDown.Contains(Keys.W) || _lst_keysHeldDown.Contains(Keys.Up);
            bool blnDownHeld = _lst_keysHeldDown.Contains(Keys.S) || _lst_keysHeldDown.Contains(Keys.Down);

            // Create movement-related int variables
            int intMoveX = 0;
            int intMoveY = 0;

            // Increment/decrement the movement-related int variables based off of the boolean variables
            if (blnLeftHeld)
            {
                intMoveX -= 1;
            }

            if (blnRightHeld)
            {
                intMoveX += 1;
            }

            if (blnUpHeld)
            {
                intMoveY -= 1;
            }

            if (blnDownHeld)
            {
                intMoveY += 1;
            }

            // Multiple the movement-related int variables by the game speed
            intMoveX *= GAMESPEED;
            intMoveY *= GAMESPEED;

            // Move the player
            _player.Move(intMoveX, intMoveY);


            _player.Update();

            // Update the projectiles
            foreach (Projectile projectile in _projectileHandler.Projectiles)
            {
                projectile.Update();
            }

            // Update the enemies
            foreach (Character enemy in _characterHandler.Characters)
            {
                if (enemy.Type != "player")
                {
                    MoveEnemy(enemy);
                    enemy.Update();
                }
            }
        }

        private void MoveEnemy(Character enemy)
        {
            // Calculate direction to target
            float deltaX = _player.FloatX - enemy.FloatX;
            float deltaY = _player.FloatY - enemy.FloatY;

            // Normalize direction
            float length = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            // Divide the delta positions by the length if it isn't equal to 0
            if (length != 0)
            {
                deltaX /= length;
                deltaY /= length;
            }

            // Multiply the movement variables to match the game speed
            deltaX *= GAMESPEED;
            deltaY *= GAMESPEED;

            enemy.Move(deltaX, deltaY);
        }

        // Méthode appelée à chaque frame
        private void NewFrame(object sender, EventArgs e)
        {
            this.Update(ticker.Interval);
            this.Render();
        }

        private void ShootMeUp_KeyDown(object sender, KeyEventArgs e)
        {
            // Add the key to the list if it's not already in there
            if (!_lst_keysHeldDown.Contains(e.KeyCode))
            {
                _lst_keysHeldDown.Add(e.KeyCode);
            }
        }

        private void ShootMeUp_KeyUp(object sender, KeyEventArgs e)
        {
            // Remove the key from the list if it's in there
            if (_lst_keysHeldDown.Contains(e.KeyCode))
            {
                _lst_keysHeldDown.Remove(e.KeyCode);
            }
        }

        private void ShootMeUp_Load(object sender, EventArgs e)
        {

        }

        private void ShootMeUp_MouseClick(object sender, MouseEventArgs e)
        {
            string strType = "";

            // If it's a left click, strType is "arrow". if its a right click, strType is "fireball".
            if (e.Button == MouseButtons.Left)
                strType = "arrow";
            else if (e.Button == MouseButtons.Right)
                strType = "fireball";

            // Shoot an arrow using the player's shoot method and add it to the projetile list
            Projectile? possibleProjectile = _player.Shoot(this.PointToClient(Cursor.Position), strType, GAMESPEED);

            if (possibleProjectile != null)
            {
                _projectileHandler.Projectiles.Add(possibleProjectile);
            }
        }
    }
}