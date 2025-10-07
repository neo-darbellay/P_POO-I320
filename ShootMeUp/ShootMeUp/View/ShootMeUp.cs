using ShootMeUp.Model;
using System.Drawing;

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
        public static readonly int WIDTH = 608;
        
        /// <summary>
        /// Height of the game area
        /// </summary>
        public static readonly int HEIGHT = 608;

        /// <summary>
        /// The player
        /// </summary>
        private Character _player;

        /// <summary>
        /// The list of keys currently held down
        /// </summary>
        private List<Keys> _lst_keysHeldDown;

        /// <summary>
        /// The list of projectiles currently in the game
        /// </summary>
        private List<Projectile> _lst_projectiles;

        /// <summary>
        /// A character handler to store every character
        /// </summary>
        private CharacterHandler _characterHandler;
        
        /// <summary>
        /// A collision handler to create obstacles
        /// </summary>
        private CollisionHandler _collisionHandler;

        /// <summary>
        /// The game's speed multiplier (movement, 
        /// </summary>
        public static readonly int GAMESPEED = 2;

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
            InitializeComponent();
            ClientSize = new Size(WIDTH, HEIGHT);


            // Gets a reference to the current BufferedGraphicsContext
            currentContext = BufferedGraphicsManager.Current;

            // Creates a BufferedGraphics instance associated with this form, and with
            // dimensions the same size as the drawing surface of the form.
            playspace = currentContext.Allocate(this.CreateGraphics(), this.DisplayRectangle);
            _player = new Character(ShootMeUp.WIDTH / 2 - 16, ShootMeUp.HEIGHT / 2 - 16, 32, 32, "player", 3);

            // Create a new list of keys held down
            _lst_keysHeldDown = new List<Keys>();

            // Create a new list of projectiles
            _lst_projectiles = new List<Projectile>();

            // Create a new CharacterHandler and COllisionHandler
            _characterHandler = new CharacterHandler();
            _collisionHandler = new CollisionHandler();

            // Reset the game
            _characterHandler.RemoveAllCharacters();
            _collisionHandler.RemoveAllObstacles();
            Score = 0;

            // Define the play area size in increments of 32
            int intBorderSize = 16;

            // Create a new border
            for (int x = 0; x <= intBorderSize; x++)
            {
                for (int y = 0; y <= intBorderSize; y++)
                {
                    if (x == 0 || x == intBorderSize || y == 0 || y == intBorderSize)
                    {
                        // Create a border piece
                        Obstacle obsBorder = new Obstacle(32 * (1 + x), 32 * (1 + y), 32, 32, 0, "border");


                        // Add the border piece to the collision handler
                        _collisionHandler.AddObstacle(obsBorder);
                    }
                }
            }



            // Create two new temporary obstacles
            Obstacle TEST1 = new Obstacle(150, 100, 32, 32, 0, "default");
            Obstacle TEST2 = new Obstacle(150, 250, 32, 32, 5, "default");
            Obstacle TEST3 = new Obstacle(150, 400, 32, 32, 3, "default");

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

            playspace.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(250, 231, 172)), new Rectangle(64, 64, 480, 480));

            _player.Render(playspace);

            // Loop through all of the obstacles and render them
            foreach (Obstacle obstacle in _collisionHandler.Obstacles)
            {
                obstacle.Render(playspace);
            }

            // Render the projectiles
            foreach (Projectile projectile in _lst_projectiles)
            {
                projectile.Render(playspace);
            }

            playspace.Render();
        }

        // Calcul du nouvel état après que 'interval' millisecondes se sont écoulées
        private void Update(int interval)
        {
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
            foreach (Projectile projectile in _lst_projectiles)
            {
                projectile.Update();
            }
        }

        // Méthode appelée à chaque frame
        private void NewFrame(object sender, EventArgs e)
        {
            this.Update(ticker.Interval);
            this.Render();
        }

        private void ShootMeUp_KeyDown(object sender, KeyEventArgs e)
        {
            // If the given key is f (shoot), trigger the player's shoot method with an arrow
            if (e.KeyCode == Keys.F)
            {
                
            }

            // Add the key to the list if it's not already in there
            else if (!_lst_keysHeldDown.Contains(e.KeyCode))
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
            Projectile? possibleProjectile = _player.Shoot(this.PointToClient(Cursor.Position), strType);

            if (possibleProjectile != null)
            {
                _lst_projectiles.Add(possibleProjectile);
            }
        }
    }
}