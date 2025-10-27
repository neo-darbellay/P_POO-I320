using Accessibility;
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
        public static readonly int GAMESPEED = 3;

        /// <summary>
        /// Any character's height and length
        /// </summary>
        public static readonly int CHARACTER_SIZE = 32;

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
            _player = new Character(256, 256, CHARACTER_SIZE, CHARACTER_SIZE, "player", PLAYER_MAXHP, 1f, GAMESPEED);

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
                        Obstacle obsBorder = new Obstacle(32 * (1 + x), 32 * (1 + y), 32, 32, 0, "border");


                        // Add the border piece to the collision handler
                        _collisionHandler.AddObstacle(obsBorder);
                    }
                }
            }

            // Creating the world environment

            // Top Left corner
            _collisionHandler.AddObstacle(new Obstacle(32 * 2, 32 * 2, CHARACTER_SIZE, CHARACTER_SIZE, 0));
            _collisionHandler.AddObstacle(new Obstacle(32 * 3, 32 * 2, CHARACTER_SIZE, CHARACTER_SIZE, 0));
            _collisionHandler.AddObstacle(new Obstacle(32 * 2, 32 * 3, CHARACTER_SIZE, CHARACTER_SIZE, 0));

            // Top Right corner
            _collisionHandler.AddObstacle(new Obstacle(32 * _intBorderSize, 32 * 2, CHARACTER_SIZE, CHARACTER_SIZE, 0));
            _collisionHandler.AddObstacle(new Obstacle(32 * _intBorderSize - 32, 32 * 2, CHARACTER_SIZE, CHARACTER_SIZE, 0));
            _collisionHandler.AddObstacle(new Obstacle(32 * _intBorderSize, 32 * 3, CHARACTER_SIZE, CHARACTER_SIZE, 0));

            // Bottom Left corner
            _collisionHandler.AddObstacle(new Obstacle(32 * 2, 32 * _intBorderSize, CHARACTER_SIZE, CHARACTER_SIZE, 0));
            _collisionHandler.AddObstacle(new Obstacle(32 * 3, 32 * _intBorderSize, CHARACTER_SIZE, CHARACTER_SIZE, 0));
            _collisionHandler.AddObstacle(new Obstacle(32 * 2, 32 * _intBorderSize - 32, CHARACTER_SIZE, CHARACTER_SIZE, 0));

            // Bottom Right corner
            _collisionHandler.AddObstacle(new Obstacle(32 * _intBorderSize, 32 * _intBorderSize, CHARACTER_SIZE, CHARACTER_SIZE, 0));
            _collisionHandler.AddObstacle(new Obstacle(32 * _intBorderSize - 32, 32 * _intBorderSize, CHARACTER_SIZE, CHARACTER_SIZE, 0));
            _collisionHandler.AddObstacle(new Obstacle(32 * _intBorderSize, 32 * _intBorderSize - 32, CHARACTER_SIZE, CHARACTER_SIZE, 0));



            ////////////////// TESTING //////////////////

            Enemy TESTENEMY = new Enemy(512, 512, CHARACTER_SIZE, CHARACTER_SIZE, "zombie", 3, 0.75f, GAMESPEED);
            Enemy TESTENEMY2 = new Enemy(256, 256, CHARACTER_SIZE, CHARACTER_SIZE, "skeleton", 3, 0.5f, GAMESPEED, true, "arrow");
            _characterHandler.AddCharacter(TESTENEMY);
            _characterHandler.AddCharacter(TESTENEMY2);

            // Create two new temporary obstacles
            Obstacle TEST1 = new Obstacle(128, 128, CHARACTER_SIZE, CHARACTER_SIZE, 10);
            Obstacle TEST2 = new Obstacle(128, 256, CHARACTER_SIZE, CHARACTER_SIZE, 5);
            Obstacle TEST3 = new Obstacle(128, 384, CHARACTER_SIZE, CHARACTER_SIZE, 3);

            _collisionHandler.AddObstacle(TEST1);
            _collisionHandler.AddObstacle(TEST2);
            _collisionHandler.AddObstacle(TEST3);
        }

        /// <summary>
        /// Render the playspace along with the player
        /// </summary>
        private void Render()
        {
            playspace.Graphics.Clear(Color.FromArgb(217, 217, 217));

            playspace.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(250, 231, 172)), new Rectangle(32, 32, 32*_intBorderSize + 32, 32*_intBorderSize + 32));

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

            playspace.Render();
        }

        // Calcul du nouvel état après que 'interval' millisecondes se sont écoulées
        private void Update(int interval)
        {
            // Remove any inactive projectiles/characters/obstacles
            _projectileHandler.Projectiles.RemoveAll(projectile => !projectile.Active);
            _characterHandler.Characters.RemoveAll(character => character.Lives <= 0);
            _collisionHandler.Obstacles.RemoveAll(obstacle => obstacle.Health <= 0);


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