using ShootMeUp.Helpers;
using ShootMeUp.Model;
using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace ShootMeUp
{
    /// <summary>
    /// The ShootMeUp class is used to serve as a 2D playspace for the player and enemies.
    /// </summary>
    public partial class ShootMeUp : Form
    {
        /// <summary>
        /// Any obstacle's height and length
        /// </summary>
        public static readonly int OBSTACLE_SIZE = 32;

        /// <summary>
        /// The default size for characters and enemies
        /// </summary>
        public static readonly int DEFAULT_CHARACTER_SIZE = OBSTACLE_SIZE - 8;

        /// <summary>
        /// The number of tiles in one chunk
        /// </summary>
        private static readonly int CHUNK_SIZE_IN_TILES = 16;

        private bool isResizing = false;

        /// <summary>
        /// The current state of the game
        /// </summary>
        private enum GameState
        {
            running,
            loading,
            paused,
            finished
        }
        private GameState _gameState;

        /// <summary>
        /// The player
        /// </summary>
        private Character? _player;

        /// <summary>
        /// The list of keys currently held down
        /// </summary>
        private readonly List<Keys> _keysHeldDown;

        /// <summary>
        /// The current wave number
        /// </summary>
        private int _intWaveNumber;

        /// <summary>
        /// The game's title screen
        /// </summary>
        private Label? _titleLabel;

        /// <summary>
        /// The game's play button
        /// </summary>
        private Button? _playButton;

        /// <summary>
        /// The game's settings button
        /// </summary>
        private Button? _settingsButton;

        /// <summary>
        /// The settings' title
        /// </summary>
        private Label? _settingsTitle;

        /// <summary>
        /// The settings' done button 
        /// </summary>
        private Button? _settingsDone;

        /// <summary>
        /// The settings' speed label
        /// </summary>
        private Label? _settingsSpeedLabel;

        /// <summary>
        /// The settings' speed panel
        /// </summary>
        private Panel? _settingsSpeedPanel;

        /// <summary>
        /// The settings' chunk label
        /// </summary>
        private Label? _settingsChunkLabel;

        /// <summary>
        /// The settings' chunk panel
        /// </summary>
        private Panel? _settingsChunkPanel;

        /// <summary>
        /// A list that contains all the projectiles
        /// </summary>
        private static List<Projectile> _projectiles = [];
        public static List<Projectile> Projectiles
        {
            get { return _projectiles; }
            set { _projectiles = value; }
        }

        /// <summary>
        /// A list that contains all the obstacles
        /// </summary>
        private static List<Obstacle> _obstacles = [];
        public static List<Obstacle> Obstacles
        {
            get { return _obstacles; }
            set { _obstacles = value; }
        }

        /// <summary>
        /// A list that contains all the characters
        /// </summary>
        private static List<Character> _characters = [];
        public static List<Character> Characters
        {
            get { return _characters; }
            set { _characters = value; }
        }

        /// <summary>
        /// The player's score
        /// </summary>
        [DefaultValue(0)]
        public int Score { get; set; }

        /// <summary>
        /// The RNG seed
        /// </summary>
        public int GAMESEED;
        private readonly Random rnd;

        private Bitmap? backBuffer;
        private Graphics? bufferG;

        public static float CameraX { get; private set; }
        public static float CameraY { get; private set; }

        //create a modal and a button for the pause menu
        private readonly PictureBox pauseModale;
        private readonly Button resumeButton;
        private readonly Button quitButton;

        // Variables used for time handling
        public static long LastFrameTime { get; set; }
        public static float DeltaTime { get; set; }

        /// <summary>
        /// The game's zoom (the smaller it is, the more you see)
        /// </summary>
        public static float Zoom { get; set; }
        private enum Biome
        {
            Plains,
            Desert,
            Mountain
        }

        private struct BiomeInfo
        {
            public int X;
            public int Y;
            public Biome Type;
        }

        private static readonly List<BiomeInfo> Biomes = [];

        /// <summary>
        /// Whether the world has generated or not
        /// </summary>
        private bool _worldReady;


        public ShootMeUp()
        {
            InitializeComponent();

            GAMESEED = (new Random()).Next(int.MinValue, int.MaxValue); // In the future, I'll make custom seeds work

            this.MaximizeBox = true;
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.Sizable;

            _intWaveNumber = 1;
            _gameState = GameState.finished;

            // Create a new list of keys held down
            _keysHeldDown = [];

            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
              ControlStyles.OptimizedDoubleBuffer |
              ControlStyles.UserPaint, true);
            this.DoubleBuffered = true;

            BackColor = ColorTranslator.FromHtml("#7f7f7f");

            rnd = new(GAMESEED);

            Zoom = 1;

            CameraX = 0;
            CameraY = 0;

            _worldReady = false;

            GameSettings.Load();

            pauseModale = new()
            {
                Top = 0,
                Left = 0,
                Width = this.ClientRectangle.Width,
                Height = this.ClientRectangle.Height,
                BackColor = Color.FromArgb(75, 100, 100, 100)
            };

            resumeButton = new()
            {
                Height = 64,
                Width = 256,
                AutoSize = false,
                Text = "Resume Game",
                BackColor = Color.White,
                Font = TextHelpers.GetCachedFont(24),
            };

            quitButton = new()
            {
                Height = 32,
                Width = 64,
                Text = "Quit",
                BackColor = Color.White,
                Font = TextHelpers.GetCachedFont(12),
            };

            resumeButton.Click += ResumeButton_Click;
            quitButton.Click += QuitButton_Click;

            backBuffer = new Bitmap(ClientSize.Width, ClientSize.Height);
            bufferG = Graphics.FromImage(backBuffer);
            bufferG.InterpolationMode = InterpolationMode.NearestNeighbor;
            bufferG.PixelOffsetMode = PixelOffsetMode.Half;
            bufferG.SmoothingMode = SmoothingMode.None;

            ShowTitle();
        }

        private void ResizeBackbuffer()
        {
            backBuffer?.Dispose();
            backBuffer = null;
            bufferG?.Dispose();
            bufferG = null;

            backBuffer = new Bitmap(ClientSize.Width, ClientSize.Height);
            bufferG = Graphics.FromImage(backBuffer);
            bufferG.InterpolationMode = InterpolationMode.NearestNeighbor;
            bufferG.PixelOffsetMode = PixelOffsetMode.Half;
            bufferG.SmoothingMode = SmoothingMode.None;
        }

        private void CenterTitleUI()
        {
            if (_titleLabel != null)
            {
                _titleLabel.Left = (ClientSize.Width - _titleLabel.Width) / 2;
                _titleLabel.Top = ClientSize.Height / 3 - _titleLabel.Height / 2;
            }

            if (_playButton != null)
            {
                _playButton.Left = (ClientSize.Width - _playButton.Width) / 2;
                _playButton.Top = (_titleLabel?.Bottom ?? (ClientSize.Height / 3)) + 128;
            }

            if (_settingsButton != null)
            {
                _settingsButton.Left = (ClientSize.Width - _settingsButton.Width) / 2;
                _settingsButton.Top = (_playButton?.Bottom ?? (ClientSize.Height / 3)) + 64;
            }
        }

        private void StopRound()
        {
            _gameState = GameState.finished;

            // Stop tickers / timers
            ticker.Stop();

            // Dispose sprite caches
            Sprites.Reset();

            // Dispose UI background images
            BackgroundImage?.Dispose();
            BackgroundImage = null;

            // Clear game entities
            _player = null;
            Characters.Clear();
            Obstacles.Clear();
            Projectiles.Clear();

            // Clear chunk cache
            ClearChunkCaches();

            // Force GC cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Recreate backbuffer fresh
            ResizeBackbuffer();
            ShowTitle();
        }

        /// <summary>
        /// Shows the game's title screen
        /// </summary>
        private void ShowTitle()
        {
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;

            BackgroundImage?.Dispose();

            BackgroundImage = new Bitmap(Sprites.Stone, ShootMeUp.OBSTACLE_SIZE, ShootMeUp.OBSTACLE_SIZE);
            BackgroundImageLayout = ImageLayout.Tile;

            // Remove any previous controls
            if (_titleLabel != null)
            {
                Controls.Remove(_titleLabel);
                _titleLabel.Dispose();
            }

            if (_playButton != null)
            {
                Controls.Remove(_playButton);
                _playButton.Dispose();
            }

            if (_settingsButton != null)
            {
                Controls.Remove(_settingsButton);
                _settingsButton.Dispose();
            }

            // Create the title label
            _titleLabel = new Label
            {
                Text = "Craft Me Up",
                Font = TextHelpers.GetCachedFont(48),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent,
            };


            // Create the play button
            _playButton = new Button
            {
                Text = "Play the game",
                Font = TextHelpers.GetCachedFont(24),
                BackColor = Color.White,
                Size = new(384, 64),
                AutoSize = false
            };

            // Create the settings button
            _settingsButton = new Button
            {
                Text = "Settings",
                Font = TextHelpers.GetCachedFont(18),
                BackColor = Color.White,
                Size = new(224, 48),
                AutoSize = false
            };

            // Add controls to the form
            Controls.Add(_titleLabel);
            Controls.Add(_playButton);
            Controls.Add(_settingsButton);

            // Set up the play button
            _playButton.Click += PlayButton_Click;

            // Set up the pause button

            _settingsButton.Click += SettingsButton_Click;

            // Center the ui
            CenterTitleUI();
        }

        private async void PlayButton_Click(object? sender, EventArgs e)
        {
            await StartGame();
            await StartWaves();
        }

        private void CenterSettingsUI()
        {
            if (_settingsTitle != null)
            {
                _settingsTitle.Left = (ClientSize.Width - _settingsTitle.Width) / 2;
                _settingsTitle.Top = ClientSize.Height / 4 - _settingsTitle.Height / 2;
            }

            if (_settingsSpeedLabel != null)
            {
                _settingsSpeedLabel.Left = (ClientSize.Width - _settingsSpeedLabel.Width) / 2;
                _settingsSpeedLabel.Top = (_settingsTitle?.Bottom ?? (ClientSize.Height / 3)) + 48;
            }

            if (_settingsSpeedPanel != null)
            {
                _settingsSpeedPanel.Left = (ClientSize.Width - _settingsSpeedPanel.Width) / 2;
                _settingsSpeedPanel.Top = (_settingsSpeedLabel?.Bottom ?? 0) + 8;

                int spacing = 16;

                List<Button> buttons = [.. _settingsSpeedPanel.Controls.OfType<Button>()];

                if (buttons.Count > 0)
                {
                    int totalWidth =
                        buttons.Sum(b => b.Width) +
                        spacing * (buttons.Count - 1);

                    int startX = (_settingsSpeedPanel.Width - totalWidth) / 2;
                    int y = (_settingsSpeedPanel.Height - buttons[0].Height) / 2;

                    for (int i = 0; i < buttons.Count; i++)
                    {
                        buttons[i].Left = startX;
                        buttons[i].Top = y;
                        startX += buttons[i].Width + spacing;
                    }
                }
            }

            if (_settingsChunkLabel != null)
            {
                _settingsChunkLabel.Left = (ClientSize.Width - _settingsChunkLabel.Width) / 2;
                _settingsChunkLabel.Top = (_settingsSpeedPanel?.Bottom ?? (ClientSize.Height / 3)) + 48;
            }

            if (_settingsChunkPanel != null)
            {
                _settingsChunkPanel.Left = (ClientSize.Width - _settingsChunkPanel.Width) / 2;
                _settingsChunkPanel.Top = (_settingsChunkLabel?.Bottom ?? 0) + 8;

                int spacing = 16;

                List<Button> buttons = [.. _settingsChunkPanel.Controls.OfType<Button>()];

                if (buttons.Count > 0)
                {
                    int totalWidth =
                        buttons.Sum(b => b.Width) +
                        spacing * (buttons.Count - 1);

                    int startX = (_settingsChunkPanel.Width - totalWidth) / 2;
                    int y = (_settingsChunkPanel.Height - buttons[0].Height) / 2;

                    for (int i = 0; i < buttons.Count; i++)
                    {
                        buttons[i].Left = startX;
                        buttons[i].Top = y;
                        startX += buttons[i].Width + spacing;
                    }
                }
            }

            if (_settingsDone != null)
            {
                _settingsDone.Left = (ClientSize.Width - _settingsDone.Width) / 2;
                _settingsDone.Top = (_settingsChunkPanel?.Bottom ?? (_settingsTitle?.Bottom ?? 0)) + 64;
            }
        }

        private void RefreshSpeedButtons()
        {
            if (_settingsSpeedPanel == null)
                return;

            foreach (Control c in _settingsSpeedPanel.Controls)
            {
                if (c is Button b)
                    b.BackColor = Color.White;
            }

            int index = GameSettings.Current.GameSpeed switch
            {
                GameSettings.GameSpeedOption.Slow => 0,
                GameSettings.GameSpeedOption.Normal => 1,
                GameSettings.GameSpeedOption.Fast => 2,
                _ => 1
            };

            if (index < _settingsSpeedPanel.Controls.Count &&
                _settingsSpeedPanel.Controls[index] is Button selected)
            {
                selected.BackColor = Color.LightGreen;
            }
        }

        private void RefreshChunkButtons()
        {
            if (_settingsChunkPanel == null)
                return;

            foreach (Control c in _settingsChunkPanel.Controls)
            {
                if (c is Button b)
                    b.BackColor = Color.White;
            }

            int index = GameSettings.Current.ChunkAmount switch
            {
                GameSettings.ChunkAmountOption.Small => 0,
                GameSettings.ChunkAmountOption.Normal => 1,
                GameSettings.ChunkAmountOption.Medium => 2,
                GameSettings.ChunkAmountOption.Large => 3,
                _ => 1
            };

            if (index < _settingsChunkPanel.Controls.Count &&
                _settingsChunkPanel.Controls[index] is Button selected)
            {
                selected.BackColor = Color.LightGreen;
            }
        }

        private static Button CreateOptionButton(string text, Size size, Action onClick, bool selected = false)
        {
            // Create the button
            Button newButton = new()
            {
                Text = text,
                Size = size,
                BackColor = selected ? Color.LightGreen : Color.White,
                Font = TextHelpers.GetCachedFont(12),
                FlatStyle = FlatStyle.Flat
            };

            // Bind onClick to it
            newButton.Click += (_, _) => onClick();

            return newButton;
        }

        /// <summary>
        /// Displays the settings
        /// </summary>
        private void DisplaySettings()
        {
            // Removes main menu controls
            Controls.Remove(_titleLabel);
            Controls.Remove(_playButton);
            Controls.Remove(_settingsButton);

            _settingsTitle = new Label
            {
                Text = "Settings",
                Font = TextHelpers.GetCachedFont(32),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent,
            };

            // Speed settings
            _settingsSpeedLabel = new()
            {
                Text = "Game Speed",
                Font = TextHelpers.GetCachedFont(16),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent,
            };

            _settingsSpeedPanel = new()
            {
                Width = 512,
                Height = 64,
                BackColor = Color.Transparent
            };

            Button speedSlowBtn = CreateOptionButton("Slow", new(100, 40), () =>
            {
                GameSettings.Current.GameSpeed = GameSettings.GameSpeedOption.Slow;
                RefreshSpeedButtons();
            });

            Button speedNormalBtn = CreateOptionButton("Normal", new(100, 40), () =>
            {
                GameSettings.Current.GameSpeed = GameSettings.GameSpeedOption.Normal;
                RefreshSpeedButtons();
            });

            Button speedFastBtn = CreateOptionButton("Fast", new(100, 40), () =>
            {
                GameSettings.Current.GameSpeed = GameSettings.GameSpeedOption.Fast;
                RefreshSpeedButtons();
            });

            _settingsSpeedPanel?.Controls.AddRange([speedSlowBtn, speedNormalBtn, speedFastBtn]);

            RefreshSpeedButtons();

            // Chunk settings
            _settingsChunkLabel = new()
            {
                Text = "Chunk amount",
                Font = TextHelpers.GetCachedFont(16),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent,
            };

            _settingsChunkPanel = new()
            {
                Width = 640,
                Height = 64,
                BackColor = Color.Transparent
            };

            Button chunkSmallBtn = CreateOptionButton("Small (16x16)", new(128, 64), () =>
            {
                GameSettings.Current.ChunkAmount = GameSettings.ChunkAmountOption.Small;
                RefreshChunkButtons();
            });

            Button chunkNormalBtn = CreateOptionButton("Normal (32x32)", new(128, 64), () =>
            {
                GameSettings.Current.ChunkAmount = GameSettings.ChunkAmountOption.Normal;
                RefreshChunkButtons();
            });

            Button chunkMediumBtn = CreateOptionButton("Medium (64x64)", new(128, 64), () =>
            {
                GameSettings.Current.ChunkAmount = GameSettings.ChunkAmountOption.Medium;
                RefreshChunkButtons();
            });

            Button chunkLargeBtn = CreateOptionButton("Large (128x128)", new(128, 64), () =>
            {
                GameSettings.Current.ChunkAmount = GameSettings.ChunkAmountOption.Large;
                RefreshChunkButtons();
            });

            _settingsChunkPanel?.Controls.AddRange([chunkSmallBtn, chunkNormalBtn, chunkMediumBtn, chunkLargeBtn]);

            RefreshChunkButtons();

            _settingsDone = new Button
            {
                Text = "Done",
                Font = TextHelpers.GetCachedFont(18),
                BackColor = Color.White,
                Size = new(224, 48),
                AutoSize = false
            };

            Controls.Add(_settingsTitle);
            Controls.Add(_settingsDone);
            Controls.Add(_settingsSpeedLabel);
            Controls.Add(_settingsSpeedPanel);
            Controls.Add(_settingsChunkLabel);
            Controls.Add(_settingsChunkPanel);

            _settingsDone.Click += SettingsDone_Click;

            // Center the ui
            CenterSettingsUI();
        }

        private void SettingsDone_Click(object? sender, EventArgs e)
        {
            GameSettings.Save();

            // Remove everything from the settings
            Controls.Remove(_settingsTitle);
            _settingsTitle?.Dispose();

            Controls.Remove(_settingsDone);
            _settingsDone?.Dispose();

            Controls.Remove(_settingsSpeedLabel);
            _settingsSpeedLabel?.Dispose();

            Controls.Remove(_settingsSpeedPanel);
            _settingsSpeedPanel?.Dispose();

            Controls.Remove(_settingsChunkLabel);
            _settingsChunkLabel?.Dispose();


            Controls.Remove(_settingsChunkPanel);
            _settingsChunkPanel?.Dispose();

            // Add the main menu controls back
            Controls.Add(_titleLabel);
            Controls.Add(_playButton);
            Controls.Add(_settingsButton);
        }

        private void SettingsButton_Click(object? sender, EventArgs e)
        {
            DisplaySettings();
        }

        private void QuitButton_Click(object? sender, EventArgs e)
        {
            DisplayPauseMenu();
            StopRound();
        }

        private void ResumeButton_Click(object? sender, EventArgs e)
        {
            DisplayPauseMenu();
        }

        private void CenterPauseUI()
        {
            if (pauseModale != null)
            {
                pauseModale.Width = ClientSize.Width;
                pauseModale.Height = ClientSize.Height;
            }

            if (resumeButton != null)
            {
                resumeButton.Left = (ClientSize.Width - resumeButton.Width) / 2;
                resumeButton.Top = (ClientSize.Height - resumeButton.Height) / 2;
            }

            if (quitButton != null)
            {
                quitButton.Left = (ClientSize.Width - quitButton.Width) / 2;
                quitButton.Top = (resumeButton?.Bottom ?? (ClientSize.Height / 3)) + 32;

            }
        }

        /// <summary>
        /// Pauses the game and displays the pause menu
        /// </summary>
        private void DisplayPauseMenu()
        {
            if (this._gameState == GameState.running)
            {
                Controls.Add(resumeButton);
                Controls.Add(pauseModale);
                Controls.Add(quitButton);

                quitButton.BringToFront();
                resumeButton.BringToFront();

                //stops the ticker to pause the game
                this.ticker.Stop();
                this._gameState = GameState.paused;

                CenterPauseUI();
            }
            else if (this._gameState == GameState.paused)
            {
                Controls.Remove(pauseModale);
                Controls.Remove(resumeButton);
                Controls.Remove(quitButton);

                long now = Environment.TickCount64;
                DeltaTime = (now - LastFrameTime) / 1000f; // seconds
                LastFrameTime = now;

                //restart the ticker
                this.ticker.Start();
                this._gameState = GameState.running;
            }

        }

        /// <summary>
        /// Get a character's sprite
        /// </summary>
        /// <param name="GivenType">The Character.Type of the given character</param>
        /// <returns>A sprite</returns>
        private static Bitmap GetSprite(Character.Type GivenType)
        {
            return Sprites.GetCharacterSprite(GivenType);
        }

        /// <summary>
        /// Get a obstacle's sprite
        /// </summary>
        /// <param name="GivenType">The Obstacle.Type of the given obstacle</param>
        /// <returns>A sprite</returns>
        private static Bitmap GetSprite(Obstacle.Type GivenType, (int Width, int Height) Size)
        {
            return Sprites.GetObstacleSprite(GivenType, Size.Width, Zoom);
        }

        /// <summary>
        /// Get a projectile sprite
        /// </summary>
        /// <param name="GivenType">The Projectile.Type of the given projectile</param>
        /// <param name="fltRotationAngle">The projetile's rotation angle</param>
        /// <returns>A sprite</returns>
        private static Bitmap GetSprite(Projectile.Type GivenType, float fltRotationAngle)
        {
            if (GivenType == Projectile.Type.Arrow_Small || GivenType == Projectile.Type.Arrow_Big || GivenType == Projectile.Type.Arrow_Jockey)
            {
                fltRotationAngle -= 45f;
            }

            return Sprites.GetProjectileSprite(GivenType, fltRotationAngle);
        }

        /// <summary>
        /// Start the game up
        /// </summary>
        private async Task StartGame()
        {
            // Reset projectile rotations immediately
            Sprites.InitializeProjectileRotations();


            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            BackgroundImage = null;
            BackgroundImageLayout = ImageLayout.None;

            // Remove title screen controls
            Controls.Remove(_titleLabel);
            Controls.Remove(_playButton);
            Controls.Remove(_settingsButton);
            _titleLabel?.Dispose();
            _playButton?.Dispose();
            _settingsButton?.Dispose();

            // Reset values
            Score = 0;
            _intWaveNumber = 1;
            LastFrameTime = Environment.TickCount64;

            Characters.Clear();
            Obstacles.Clear();
            Projectiles.Clear();
            ClearChunkCaches();

            GenerateBiomeInfo();

            // Force redraw
            RenderFrame();
            Invalidate();

            _gameState = GameState.loading;
            _worldReady = false;

            await Task.Run(() => GenerateWorld());

            _worldReady = true;
            _gameState = GameState.running;


            LastFrameTime = Environment.TickCount64;
            ticker.Start();
        }

        private static readonly Dictionary<(int, int), Biome> ChunkBiomeCache = [];
        private static readonly Dictionary<(int, int), List<Obstacle>> ChunkObstacleCache = [];


        // Clears all spatial caches so old obstacle references cannot linger
        private static void ClearChunkCaches()
        {
            foreach (var kv in ChunkObstacleCache.ToList())
            {
                kv.Value.Clear();   // clear the per-chunk lists
                                    // remove the dictionary entry
                ChunkObstacleCache.Remove(kv.Key);
            }
            ChunkBiomeCache.Clear();
        }


        /// <summary>
        /// Noise algorithm used for biome generation
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        private float BiomeNoise(int x, int y)
        {
            // Unchecked keyword used to ignore overflow and roll back to 0
            unchecked
            {
                int n = x * 1619 + y * 31337 + GAMESEED * 1013;
                n = (n << 13) ^ n;
                return 1f - ((n * (n * n * 60493 + 19990303) + 1376312589)
                             & 0x7fffffff) / 1073741824f;
            }
        }

        private float SmoothBiomeNoise(float x, float y)
        {
            int x0 = (int)Math.Floor(x);
            int y0 = (int)Math.Floor(y);

            float xFrac = x - x0;
            float yFrac = y - y0;

            float n00 = BiomeNoise(x0, y0);
            float n10 = BiomeNoise(x0 + 1, y0);
            float n01 = BiomeNoise(x0, y0 + 1);
            float n11 = BiomeNoise(x0 + 1, y0 + 1);

            float u = Fade(xFrac);
            float v = Fade(yFrac);

            float nx0 = Lerp(n00, n10, u);
            float nx1 = Lerp(n01, n11, u);

            return Lerp(nx0, nx1, v);
        }

        static float Fade(float t)
        {
            // Perlin fade function (smooth curve)
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        /// <summary>
        /// Generate biome informations
        /// </summary>
        private void GenerateBiomeInfo()
        {
            Biomes.Clear();

            // Seed count proportional to map area
            int seedCount = Math.Max(10, (GameSettings.Current.ChunkAmountValue * GameSettings.Current.ChunkAmountValue) / 150);

            for (int i = 0; i < seedCount; i++)
            {
                Biomes.Add(new BiomeInfo
                {
                    X = rnd.Next(GameSettings.Current.ChunkAmountValue),
                    Y = rnd.Next(GameSettings.Current.ChunkAmountValue),
                    Type = (Biome)rnd.Next(3)
                });
            }
        }

        /// <summary>
        /// Gets the chunk's biome info and uses it
        /// </summary>
        /// <param name="chunkX"></param>
        /// <param name="chunkY"></param>
        /// <returns></returns>
        private Biome GetChunkBiome(int chunkX, int chunkY)
        {
            (int chunkX, int chunkY) key = (chunkX, chunkY);

            if (ChunkBiomeCache.TryGetValue(key, out Biome cached))
                return cached;

            BiomeInfo closest = Biomes[0];
            float bestDist = float.MaxValue;

            float biomeScale = 1f;
            float distortion = SmoothBiomeNoise(chunkX * 0.1f, chunkY * 0.1f) * 1.5f;

            foreach (BiomeInfo seed in Biomes)
            {
                float dx = (chunkX - seed.X) * biomeScale;
                float dy = (chunkY - seed.Y) * biomeScale;

                float dist = dx * dx + dy * dy + distortion * distortion;

                if (dist < bestDist)
                {
                    bestDist = dist;
                    closest = seed;
                }
            }

            ChunkBiomeCache[key] = closest.Type;
            return closest.Type;
        }

        /// <summary>
        /// Gets a list of obstacles near the given AABB, by checking the chunks that are covered by it. This is used for collision checks, to avoid checking every obstacle in the world.
        /// </summary>
        public static IEnumerable<Obstacle> GetObstaclesNear(float x, float y, float w, float h, int expandChunks = 1)
        {
            // Get chunk range covering the AABB, with a small expansion to be safe
            var (cx0, cy0) = GetChunkCoord(x, y);
            var (cx1, cy1) = GetChunkCoord(x + w, y + h);

            cx0 -= expandChunks; cy0 -= expandChunks;
            cx1 += expandChunks; cy1 += expandChunks;

            for (int cx = cx0; cx <= cx1; cx++)
            {
                for (int cy = cy0; cy <= cy1; cy++)
                {
                    if (ChunkObstacleCache.TryGetValue((cx, cy), out var list))
                    {
                        foreach (var o in list)
                            yield return o;
                    }
                }
            }
        }

        private static readonly int CHUNK_SIZE_PX = CHUNK_SIZE_IN_TILES * OBSTACLE_SIZE;

        private static (int, int) GetChunkCoord(float x, float y)
        {
            return ((int)(x / CHUNK_SIZE_PX), (int)(y / CHUNK_SIZE_PX));
        }
        private IEnumerable<(int, int)> GetVisibleChunks()
        {
            float viewLeft = CameraX;
            float viewTop = CameraY;
            float viewRight = CameraX + ClientSize.Width / Zoom;
            float viewBottom = CameraY + ClientSize.Height / Zoom;

            int x0 = (int)Math.Floor(viewLeft / CHUNK_SIZE_PX);
            int x1 = (int)Math.Floor(viewRight / CHUNK_SIZE_PX);
            int y0 = (int)Math.Floor(viewTop / CHUNK_SIZE_PX);
            int y1 = (int)Math.Floor(viewBottom / CHUNK_SIZE_PX);

            for (int x = x0; x <= x1; x++)
                for (int y = y0; y <= y1; y++)
                    yield return (x, y);
        }
        private static void AddObstacleToChunk(Obstacle obstacle)
        {
            var chunk = GetChunkCoord(obstacle.Position.X, obstacle.Position.Y);

            if (!ChunkObstacleCache.TryGetValue(chunk, out var list))
                ChunkObstacleCache[chunk] = list = [];

            list.Add(obstacle);
        }

        /// <summary>
        /// Generate the game itself
        /// </summary>
        private void GenerateWorld()
        {
            float fltMapSize = CHUNK_SIZE_IN_TILES * GameSettings.Current.ChunkAmountValue * OBSTACLE_SIZE;
            float fltAreaCenterX = fltMapSize / 2;

            _player = new(fltAreaCenterX, fltAreaCenterX, DEFAULT_CHARACTER_SIZE, Character.Type.Player, GameSettings.Current.GameSpeedValue);
            Characters.Add(_player);

            // Generate border
            for (int x = -32; x <= fltMapSize; x += 32)
            {
                for (int y = -32; y <= fltMapSize; y += 32)
                {
                    if (x == -32 || y == -32 || x == fltMapSize || y == fltMapSize)
                        Obstacles.Add(new(x, y, OBSTACLE_SIZE, Obstacle.Type.Barrier));
                }
            }

            int intChunkLength = CHUNK_SIZE_IN_TILES * OBSTACLE_SIZE;
            int intMaxHealthInAChunk = 160;

            // Precompute biome obstacles once
            Dictionary<Obstacle.Type, Obstacle.Type[]> biomeObstacles = new()
            {
                [Obstacle.Type.Grass] = [Obstacle.Type.Bush, Obstacle.Type.Wood],
                [Obstacle.Type.Sand] = [Obstacle.Type.Dirt],
                [Obstacle.Type.Stone] = [Obstacle.Type.CobbleStone]
            };

            for (int chunkX = 0; chunkX < GameSettings.Current.ChunkAmountValue; chunkX++)
            {
                for (int chunkY = 0; chunkY < GameSettings.Current.ChunkAmountValue; chunkY++)
                {
                    Biome biome = GetChunkBiome(chunkX, chunkY);

                    Obstacle.Type randomFloor = biome switch
                    {
                        Biome.Plains => Obstacle.Type.Grass,
                        Biome.Desert => Obstacle.Type.Sand,
                        Biome.Mountain => Obstacle.Type.Stone,
                        _ => Obstacle.Type.Barrier
                    };

                    float fltX = chunkX * intChunkLength;
                    float fltY = chunkY * intChunkLength;

                    Obstacle floor = new(fltX, fltY, intChunkLength, randomFloor);
                    Obstacles.Add(floor);

                    // Create a per-chunk obstacle list for fast collision checks
                    List<Obstacle> localObstacles = [];
                    ChunkObstacleCache[(chunkX, chunkY)] = localObstacles;

                    float fltHealthInChunk = 0;

                    while (fltHealthInChunk < intMaxHealthInAChunk)
                    {
                        float fltRemainingHealth = intMaxHealthInAChunk - fltHealthInChunk;

                        Obstacle.Type[] valid = [.. biomeObstacles[randomFloor]
                            .Where(t =>
                                (t == Obstacle.Type.Bush && 1.25 <= fltRemainingHealth) ||
                                (t == Obstacle.Type.Dirt && 10 <= fltRemainingHealth) ||
                                (t == Obstacle.Type.Wood && 20 <= fltRemainingHealth) ||
                                (t == Obstacle.Type.CobbleStone && 25 <= fltRemainingHealth))];

                        if (valid.Length == 0)
                            break;

                        Obstacle.Type randomObstacleType = valid[rnd.Next(valid.Length)];

                        float fltCurrentObstacleHealth = randomObstacleType switch
                        {
                            Obstacle.Type.Bush => 1.25f,
                            Obstacle.Type.Dirt => 10,
                            Obstacle.Type.Wood => 20,
                            Obstacle.Type.CobbleStone => 25,
                            _ => 0
                        };

                        fltHealthInChunk += fltCurrentObstacleHealth;

                        Obstacle newObstacle = new(0, 0, OBSTACLE_SIZE, randomObstacleType);

                        // Reduced attempts to avoid lag
                        int attempts = 0;
                        bool blnOk;

                        do
                        {
                            attempts++;

                            (float X, float Y) randomPos = (
                                rnd.Next(0, CHUNK_SIZE_IN_TILES) * OBSTACLE_SIZE,
                                rnd.Next(0, CHUNK_SIZE_IN_TILES) * OBSTACLE_SIZE
                            );

                            randomPos.X += chunkX * intChunkLength;
                            randomPos.Y += chunkY * intChunkLength;

                            newObstacle.Position = randomPos;

                            blnOk = true;

                            // Only check collisions against objects inside this chunk
                            foreach (Obstacle obstacle in localObstacles)
                            {
                                if (IsOverlapping(newObstacle, obstacle))
                                {
                                    blnOk = false;
                                    break;
                                }
                            }

                            // Check player overlap
                            if (blnOk && IsOverlapping(newObstacle, _player))
                                blnOk = false;

                            if (blnOk)
                            {
                                Obstacles.Add(newObstacle);
                                AddObstacleToChunk(newObstacle);
                                localObstacles.Add(newObstacle);
                            }

                        } while (!blnOk && attempts < 100);
                    }
                }
            }
        }

        /// <summary>
        /// Generate random waves of enemies
        /// </summary>
        /// <param name="waveNumber">The current wave's number</param>
        /// <returns></returns>
        private List<Enemy> GenerateWaves(int waveNumber)
        {
            if (_player == null || _player.Lives <= 0)
                return [];

            List<Enemy> WaveEnemies = [];

            // Base number of enemies plus some random variation
            int baseEnemies = 3;
            int totalEnemies = baseEnemies + rnd.Next(waveNumber, waveNumber * 3);

            // Define enemy types with score values
            List<(Character.Type Type, int Weight, float SizeMultiplier)> enemyTypes =
            [
                (Character.Type.Zombie,         1,       1f),
                (Character.Type.Skeleton,       3,       1f),
                (Character.Type.Baby_Zombie,    5,       0.75f),
                (Character.Type.Zombie_Pigman,  10,      1f),
                (Character.Type.Blaze,          20,      1f)
            ];

            while (totalEnemies > 0)
            {
                // Weighted probability for each enemy type
                float[] weights = [.. enemyTypes.Select(e =>
                {
                    // Base chance inversely proportional to Weight (rarer = lower chance)
                    float baseChance = 1f / e.Weight;

                    // Wave effect: increases chance for higher Weight as wave progresses
                    float waveEffect = 1f + (waveNumber * 0.05f * e.Weight);

                    return baseChance * waveEffect;
                })];

                // Normalize and select enemy
                float weightSum = weights.Sum();
                float roll = (float)(rnd.NextDouble() * weightSum);

                Character.Type selectedType = enemyTypes[0].Type;
                float sizeMultiplier = 1f;
                float cumulative = 0;

                for (int i = 0; i < enemyTypes.Count; i++)
                {
                    cumulative += weights[i];
                    if (roll <= cumulative)
                    {
                        selectedType = enemyTypes[i].Type;
                        sizeMultiplier = enemyTypes[i].SizeMultiplier;
                        break;
                    }
                }

                // Determine enemy size
                int intCharSize = (int)(DEFAULT_CHARACTER_SIZE * sizeMultiplier);

                // Add enemy to wave
                WaveEnemies.Add(new Enemy(0, 0, intCharSize, selectedType, GameSettings.Current.GameSpeedValue, _player));

                // Reduce totalEnemies based on Weight (rarer/stronger enemies "cost" more)
                totalEnemies -= Math.Max(1, enemyTypes.First(e => e.Type == selectedType).Weight);
            }

            // Adds bosses at specific waves
            if (waveNumber % 5 == 0)
            {
                // Create multiple checks
                bool isMultipleOf50 = waveNumber % 50 == 0;
                bool isMultipleOf25 = waveNumber % 25 == 0;
                bool isMultipleOf10 = waveNumber % 10 == 0;

                // Initialize spawn variables
                int intBossAmount;
                float fltSizeMultiplicator;
                Character.Type BossType;

                if (isMultipleOf50)
                {
                    intBossAmount = waveNumber / 50;
                    fltSizeMultiplicator = 8;
                    BossType = Character.Type.Dragon;
                }
                else if (isMultipleOf25)
                {
                    intBossAmount = waveNumber / 25;
                    fltSizeMultiplicator = 6;
                    BossType = Character.Type.Wither;
                }
                else if (isMultipleOf10)
                {
                    intBossAmount = waveNumber / 10;
                    fltSizeMultiplicator = 1.5f;
                    BossType = Character.Type.WitherSkeleton;
                }
                else
                {
                    intBossAmount = waveNumber / 5;
                    fltSizeMultiplicator = 2;
                    BossType = Character.Type.SpiderJockey;
                }

                for (int i = 0; i < intBossAmount; i++)
                    WaveEnemies.Add(new Enemy(0, 0, (int)(DEFAULT_CHARACTER_SIZE * fltSizeMultiplicator), BossType, GameSettings.Current.GameSpeedValue, _player));
            }

            return WaveEnemies;
        }

        /// <summary>
        /// Wait for a specified amount of time
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        private async Task Wait(int amount)
        {
            do
            {
                do
                {
                    await Task.Delay(1);
                } while (_gameState == GameState.paused);

                amount--;
            } while (amount > 0);
        }

        /// <summary>
        /// Starts the wave system
        /// </summary>
        private async Task StartWaves()
        {
            while (_gameState != GameState.finished && (_player != null && _player.Lives > 0))
            {
                // Add a wait before the next wave
                await Wait(20000 / GameSettings.Current.GameSpeedValue);

                // Get the wave's enemies
                List<Enemy> waveEnemies = GenerateWaves(_intWaveNumber);

                foreach (Enemy enemy in waveEnemies)
                {
                    // End the wave system if the game stopped
                    if (_gameState == GameState.finished)
                        return;

                    // Put the enemy in the right spot
                    bool reroll;
                    do
                    {
                        //determines the coordinates
                        reroll = false;
                        int spawn_angle = new Random().Next(361);
                        float random_X = (float)(Math.Sin(spawn_angle) * 8 + new Random().Next(10)) * DEFAULT_CHARACTER_SIZE;
                        float random_Y = (float)(Math.Cos(spawn_angle) * 8 + new Random().Next(10)) * DEFAULT_CHARACTER_SIZE;
                        if (spawn_angle > 270)
                        {
                            enemy.Position = (
                                _player.Position.X - random_X,
                                _player.Position.Y + random_Y
                            );
                        }
                        else if (spawn_angle > 180)
                        {
                            enemy.Position = (
                                _player.Position.X + random_X,
                                _player.Position.Y + random_Y
                            );
                        }
                        else if (spawn_angle > 90)
                        {
                            enemy.Position = (
                                _player.Position.X + random_X,
                                _player.Position.Y - random_Y
                            );
                        }
                        else
                        {
                            enemy.Position = (
                                _player.Position.X - random_X,
                                _player.Position.Y - random_Y
                            );
                        }

                        // Retry if the enemy is outside of the world
                        if (!IsInsideWorld(enemy))
                        {
                            reroll = true;
                            continue;
                        }

                        //check if the ennemy is in a wall
                        foreach (Obstacle obstacle in Obstacles)
                        {
                            if (IsOverlapping(enemy, obstacle) && obstacle.CanCollide)
                            {
                                reroll = true;
                                break;

                            }
                        }
                        //reroll if the ennemy is in a wall
                    } while (reroll);

                    // Update its LastDamage value
                    enemy.LastDamageTimer = 0;

                    // Add the enemy to the character list
                    Characters.Add(enemy);


                    // Add a wait before adding the next enemy
                    await Wait(20000 / GameSettings.Current.GameSpeedValue);
                }

                // Clear the wave enemies table
                waveEnemies.Clear();

                // Wait until all enemies are dead
                while (Characters.Count > 1 && _gameState != GameState.finished)
                {
                    await Wait(1);
                }

                // Increment the wave number
                _intWaveNumber++;
            }
        }

        /// <summary>
        /// Tests if 2 cframes are overlapping
        /// </summary>
        /// <returns>A bool that is true if the cframes are overlapping</returns>
        public static bool IsOverlapping(CFrame cfr1, CFrame cfr2)
        {
            bool overlapX = cfr1.Position.X < cfr2.Position.X + cfr2.Size.Width && cfr1.Position.X + cfr1.Size.Width > cfr2.Position.X;
            bool overlapY = cfr1.Position.Y < cfr2.Position.Y + cfr2.Size.Height && cfr1.Position.Y + cfr1.Size.Height > cfr2.Position.Y;
            return overlapX && overlapY;
        }

        /// <summary>
        /// Tests if 2 cframes are overlapping
        /// </summary>
        /// <returns>A bool that is true if the cframes are overlapping</returns>
        public static bool IsOverlapping(CFrame cfr, float X1, float Y1, float Width1, float Height1)
        {
            bool overlapX = X1 < cfr.Position.X + cfr.Size.Width && X1 + Width1 > cfr.Position.X;
            bool overlapY = Y1 < cfr.Position.Y + cfr.Size.Height && Y1 + Height1 > cfr.Position.Y;
            return overlapX && overlapY;
        }

        /// <summary>
        /// Tests if 2 cframes are overlapping
        /// </summary>
        /// <returns>A bool that is true if the cframes are overlapping</returns>
        public static bool IsOverlapping(float X1, float Y1, float Width1, float Height1, float X2, float Y2, float Width2, float Height2)
        {
            bool overlapX = X1 < X2 + Width2 && X1 + Width1 > X2;
            bool overlapY = Y1 < Y2 + Height2 && Y1 + Height1 > Y2;
            return overlapX && overlapY;
        }

        /// <summary>
        /// Tests to see if the given cframe is inside or outside the world
        /// </summary>
        /// <param name="cfr"></param>
        /// <returns></returns>
        public static bool IsInsideWorld(CFrame cfr)
        {
            int intMapSize = CHUNK_SIZE_IN_TILES * GameSettings.Current.ChunkAmountValue * OBSTACLE_SIZE;
            return
                cfr.Position.X >= 0 &&
                cfr.Position.Y >= 0 &&
                cfr.Position.X + cfr.Size.Width <= intMapSize &&
                cfr.Position.Y + cfr.Size.Height <= intMapSize;
        }

        private void DrawBackground(Graphics? g)
        {
            Bitmap tile = Sprites.Stone;
            int tileSize = OBSTACLE_SIZE;

            int tilesX = (int)Math.Ceiling(ClientSize.Width / (float)tileSize) + 1;
            int tilesY = (int)Math.Ceiling(ClientSize.Height / (float)tileSize) + 1;

            for (int x = 0; x < tilesX; x++)
            {
                for (int y = 0; y < tilesY; y++)
                {
                    g?.DrawImage(
                        tile,
                        x * tileSize,
                        y * tileSize,
                        tileSize,
                        tileSize
                    );
                }
            }
        }

        public bool IsOnScreen(float x, float y, float w, float h)
        {
            float screenW = ClientSize.Width;
            float screenH = ClientSize.Height;

            float screenX = x * Zoom;
            float screenY = y * Zoom;
            float screenWObj = w * Zoom;
            float screenHObj = h * Zoom;

            return screenX + screenWObj >= 0 &&
                   screenX <= screenW &&
                   screenY + screenHObj >= 0 &&
                   screenY <= screenH;
        }

        /// <summary>
        /// Draws an object whose parent class is CFrame's health
        /// </summary>
        /// <param name="cfrObject"></param>
        private void RenderHealth(CFrame cfrObject, int intHealth, Bitmap HealthSprite, float fltHeartScale, int intMaxHeartsPerRow)
        {
            float fltHeartPadding = 1;

            // Get CFrame related values
            float fltPosX = (cfrObject.Position.X - CameraX);
            float fltPosY = (cfrObject.Position.Y - CameraY);
            float fltWidth = cfrObject.Size.Width;

            // Get the total amount of rows
            int intRows = (int)Math.Ceiling(intHealth / (float)intMaxHeartsPerRow);

            // Get the heart size
            float fltHeartSize = DEFAULT_CHARACTER_SIZE * fltHeartScale;

            // Row padding
            float fltRowSpacing = fltHeartSize * 0.1f;

            // Get the starting position of hearts
            float fltStartY = fltPosY - (intRows * fltHeartSize + (intRows - 1) * fltRowSpacing) - fltRowSpacing;

            // Start the health generation
            int intHeartIndex = 0;

            for (int intRow = 0; intRow < intRows; intRow++)
            {
                int intHearts = Math.Min(intMaxHeartsPerRow, intHealth - intHeartIndex);

                // Row width including horizontal padding
                float rowWidth = intHearts * fltHeartSize + (intHearts - 1) * fltHeartPadding;

                //TODO: make EmptyHeart appear if needed

                // Center horizontally
                float fltStartX = fltPosX + (fltWidth - rowWidth) / 2f;

                for (int intCol = 0; intCol < intHearts; intCol++)
                {
                    float fltDrawX = fltStartX + intCol * (fltHeartSize + fltHeartPadding);

                    float fltDrawY = fltStartY + intRow * (fltHeartSize + fltHeartPadding);

                    bufferG?.DrawImage(
                        HealthSprite,
                        fltDrawX * Zoom,
                        fltDrawY * Zoom,
                        fltHeartSize * Zoom,
                        fltHeartSize * Zoom
                    );

                    intHeartIndex++;
                }
            }
        }


        /// <summary>
        /// Render a new frame
        /// </summary>
        private void RenderFrame()
        {
            if (bufferG == null)
                return;

            // Show a loading text
            if (_gameState == GameState.loading || !_worldReady)
            {
                bufferG.Clear(Color.Black);

                Font font = TextHelpers.GetCachedFont(32);
                SizeF textSize = bufferG.MeasureString("Loading...", font);

                float x = (ClientSize.Width - textSize.Width) / 2f;
                float y = (ClientSize.Height - textSize.Height) / 2f;

                bufferG.DrawString("Loading...", font, Brushes.White, x, y);
                return;
            }

            if (_player != null)
            {
                // Clear the frame
                DrawBackground(bufferG);

                using Font scaledFont = new(TextHelpers.drawFont.FontFamily, TextHelpers.drawFont.Size * Zoom, TextHelpers.drawFont.Style);

                // Draw all the background assets first
                Obstacle.Type[] FloorTypes = [Obstacle.Type.Sand, Obstacle.Type.Grass, Obstacle.Type.Stone];

                foreach (Obstacle Floor in Obstacles)
                {
                    // Only continue if they're from one of the floor types
                    if (FloorTypes.Contains(Floor.ObstType))
                    {
                        float drawX = Floor.Position.X - CameraX;
                        float drawY = Floor.Position.Y - CameraY;

                        if (!IsOnScreen(drawX, drawY, Floor.Size.Width, Floor.Size.Height))
                            continue;

                        // Depending on the zoom, either get the cached floor type, or a 1px wide image
                        Bitmap? chunk = GetSprite(Floor.ObstType, Floor.Size);

                        bufferG.DrawImage(chunk, drawX * Zoom, drawY * Zoom, Floor.Size.Width * Zoom, Floor.Size.Height * Zoom);
                    }
                }

                // Draw all characters (not including player or characters with collisions off)
                foreach (Character character in Characters)
                {
                    if (character.CharType == Character.Type.Player || !character.CanCollide) continue;

                    float drawX = character.Position.X - CameraX;
                    float drawY = character.Position.Y - CameraY;

                    if (!IsOnScreen(drawX, drawY, character.Size.Width, character.Size.Height))
                        continue;
                    bufferG.DrawImage(GetSprite(character.CharType), drawX * Zoom, drawY * Zoom, character.Size.Width * Zoom, character.Size.Height * Zoom);
                }

                // Draw obstacles based on chunks
                foreach ((int, int) chunk in GetVisibleChunks())
                {
                    if (ChunkObstacleCache.TryGetValue(chunk, out List<Obstacle>? currentObstacles))
                    {
                        // Draw obstacles that can collide
                        foreach (Obstacle obstacle in currentObstacles)
                        {
                            // Only continue if they're not from one of the floor types, and have collisions on
                            if (FloorTypes.Contains(obstacle.ObstType) || !obstacle.CanCollide)
                                continue;

                            float drawX = obstacle.Position.X - CameraX;
                            float drawY = obstacle.Position.Y - CameraY;

                            if (!IsOnScreen(drawX, drawY, obstacle.Size.Width, obstacle.Size.Height))
                                continue;

                            bufferG.DrawImage(GetSprite(obstacle.ObstType, obstacle.Size), drawX * Zoom, drawY * Zoom, obstacle.Size.Width * Zoom, obstacle.Size.Height * Zoom);
                        }

                        // Create a new string format for centering text
                        using var sfCenter = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };

                        // Draw obstacle health
                        foreach (Obstacle obstacle in currentObstacles)
                        {
                            // Only continue if they're not from one of the floor types, and aren't invincible
                            if (!(obstacle.ObstType == Obstacle.Type.Sand || obstacle.ObstType == Obstacle.Type.Grass || obstacle.ObstType == Obstacle.Type.Stone) && !obstacle.Invincible)
                            {
                                float drawX = obstacle.Position.X - CameraX;
                                float drawY = obstacle.Position.Y - CameraY;

                                if (!IsOnScreen(drawX, drawY, obstacle.Size.Width, obstacle.Size.Height))
                                    continue;


                                string txt = $"{obstacle}";
                                if (!string.IsNullOrEmpty(txt))
                                {
                                    float screenX = (obstacle.Position.X - CameraX) * Zoom;
                                    float screenY = (obstacle.Position.Y - CameraY) * Zoom;
                                    float obstacleScreenWidth = obstacle.Size.Width * Zoom;
                                    float obstacleScreenHeight = obstacle.Size.Height * Zoom;

                                    var rect = new RectangleF(screenX, screenY, obstacleScreenWidth, obstacleScreenHeight);
                                    bufferG.DrawString(txt, scaledFont, TextHelpers.writingBrush, rect, sfCenter);
                                }

                            }
                        }
                    }
                }

                // Draw all characters (not including player or characters with collisions on)
                foreach (Character character in Characters)
                {
                    if (character.CharType == Character.Type.Player || character.CanCollide) continue;

                    float drawX = character.Position.X - CameraX;
                    float drawY = character.Position.Y - CameraY;

                    if (!IsOnScreen(drawX, drawY, character.Size.Width, character.Size.Height))
                        continue;
                    bufferG.DrawImage(GetSprite(character.CharType), drawX * Zoom, drawY * Zoom, character.Size.Width * Zoom, character.Size.Height * Zoom);
                }

                // Draw projectiles
                foreach (Projectile projectile in Projectiles)
                {
                    float drawX = projectile.Position.X - CameraX;
                    float drawY = projectile.Position.Y - CameraY;

                    if (!IsOnScreen(drawX, drawY, projectile.Size.Width, projectile.Size.Height))
                        continue;

                    bufferG.DrawImage(GetSprite(projectile.ProjType, projectile.RotationAngle), drawX * Zoom, drawY * Zoom, projectile.Size.Width * Zoom, projectile.Size.Height * Zoom);
                }

                // Draw the player
                if (_player != null)
                {
                    float px = _player.Position.X - CameraX;
                    float py = _player.Position.Y - CameraY;

                    bufferG.DrawImage(GetSprite(_player.CharType), px * Zoom, py * Zoom, _player.Size.Width * Zoom, _player.Size.Height * Zoom);
                }

                // Draw all characters' health (not including player)
                foreach (Character character in Characters)
                {
                    if (character.CharType == Character.Type.Player || character is not Enemy enemy) continue;

                    // Draw its health depending on its type
                    Bitmap HeartType = enemy.CharType switch
                    {
                        Character.Type.SpiderJockey or Character.Type.WitherSkeleton or Character.Type.Dragon or Character.Type.Wither => Sprites.BossHeart,
                        _ => Sprites.EnemyHeart,
                    };
                    RenderHealth(character, character.Lives, HeartType, 0.4f, 10);
                }

                // Draw obstacles based on chunks
                foreach ((int, int) chunk in GetVisibleChunks())
                {
                    if (ChunkObstacleCache.TryGetValue(chunk, out List<Obstacle>? currentObstacles))
                    {
                        // Draw obstacles that has collisions off
                        foreach (Obstacle obstacle in currentObstacles)
                        {
                            // Only continue if they're not from one of the floor types, and have collisions off
                            if (FloorTypes.Contains(obstacle.ObstType) || obstacle.CanCollide)
                                continue;

                            float drawX = obstacle.Position.X - CameraX;
                            float drawY = obstacle.Position.Y - CameraY;

                            if (!IsOnScreen(drawX, drawY, obstacle.Size.Width, obstacle.Size.Height))
                                continue;

                            bufferG.DrawImage(GetSprite(obstacle.ObstType, obstacle.Size), drawX * Zoom, drawY * Zoom, obstacle.Size.Width * Zoom, obstacle.Size.Height * Zoom);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Clamp the camera
        /// </summary>
        private void ClampCamera()
        {
            float mapSize = CHUNK_SIZE_IN_TILES * GameSettings.Current.ChunkAmountValue * OBSTACLE_SIZE;

            float viewportW = ClientSize.Width / Zoom;
            float viewportH = ClientSize.Height / Zoom;

            // Center map if viewport is larger than world
            if (viewportW >= mapSize)
            {
                CameraX = ((mapSize + OBSTACLE_SIZE) - viewportW) / 2f;
            }
            else
            {
                CameraX = Math.Clamp(CameraX, -OBSTACLE_SIZE, (mapSize + OBSTACLE_SIZE) - viewportW);
            }

            if (viewportH >= mapSize)
            {
                CameraY = ((mapSize + OBSTACLE_SIZE) - viewportH) / 2f;
            }
            else
            {
                CameraY = Math.Clamp(CameraY, -OBSTACLE_SIZE, (mapSize + OBSTACLE_SIZE) - viewportH);
            }
        }

        private void UpdateCamera()
        {
            if (_player == null) return;

            float viewportW = ClientSize.Width / Zoom;
            float viewportH = ClientSize.Height / Zoom;

            CameraX = _player.Position.X + _player.Size.Width / 2f - viewportW / 2f;
            CameraY = _player.Position.Y + _player.Size.Height / 2f - viewportH / 2f;

            ClampCamera();
        }

        public static GraphicsPath CreateRoundedRectangle(RectangleF rect, float radius)
        {
            float diameter = radius * 2f;
            GraphicsPath path = new();

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);

            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// Draw the user interface
        /// </summary>
        /// <param name="g"></param>
        private void DrawUI(Graphics g)
        {
            if (_player == null)
                return;

            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.SmoothingMode = SmoothingMode.None;

            // Store the UI text
            string strText = $"Wave {_intWaveNumber} | Score: {Score}";

            // Removing this line and putting TextHelpers.drawFont in the code instead of a variable causes "System.ArgumentException: Parameter is not valid." on .MeasureString,
            // even though the font is valid. I have no idea why, but this fixes it so I'm not gonna question it.
            Font font = TextHelpers.drawFont;

            // Layout variables
            float fltPadding = 12f;
            float fltCornerRadius = 8f;
            float fltTopY = 16f;
            float fltSpacingY = 6f;

            int intHeartSize = 24;
            int intHeartSpacing = 30;

            // Measure text
            SizeF textSize = g.MeasureString(strText, font);

            // Hearts width
            int heartsWidth = 10 * intHeartSpacing;
            int heartsHeight = intHeartSize + 4;

            // Panel size
            float panelWidth = Math.Max(textSize.Width, heartsWidth) + fltPadding * 2;
            float panelHeight = textSize.Height + fltSpacingY + heartsHeight + fltPadding * 2;

            // Center panel horizontally
            float panelX = (ClientSize.Width / 2f) - (panelWidth / 2f);
            float panelY = fltTopY;

            RectangleF panelRect = new(panelX, panelY, panelWidth, panelHeight);

            // Rounded background + border
            using (GraphicsPath path = CreateRoundedRectangle(panelRect, fltCornerRadius))
            using (Brush bgBrush = new SolidBrush(Color.FromArgb(150, 40, 40, 40)))
            using (Pen borderPen = new(Color.FromArgb(200, 90, 90, 90), 1.5f))
            {
                g.FillPath(bgBrush, path);
                g.DrawPath(borderPen, path);
            }

            // Draw the text
            float textX = panelX + (panelWidth / 2f) - (textSize.Width / 2f);
            float textY = panelY + fltPadding;

            g.DrawString(strText, TextHelpers.drawFont, TextHelpers.writingBrush, textX, textY);

            // Draw the hearts
            float heartsX = panelX + (panelWidth / 2f) - (heartsWidth / 2f);
            float heartsY = textY + textSize.Height + fltSpacingY + 8;

            for (int i = 0; i < 10; i++)
            {
                Bitmap HeartIcon = (i < _player.Lives) ? Sprites.PlayerHeart : Sprites.EmptyHeart;

                g.DrawImage(
                    HeartIcon,
                    heartsX + (i * intHeartSpacing),
                    heartsY,
                    intHeartSize,
                    intHeartSize
                );
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_gameState == GameState.finished || backBuffer == null)
            {
                base.OnPaint(e);
                return;
            }

            e.Graphics.Clear(Color.Black);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            e.Graphics.DrawImage(backBuffer, 0, 0);

            // Draw UI on top
            DrawUI(e.Graphics);
        }

        /// <summary>
        /// Clean up dead entities, broken obstacles, etc.
        /// </summary>
        private void CleanupEntities()
        {
            // Remove anything inactive
            Projectiles.RemoveAll(p => !p.Active);

            Obstacles.RemoveAll(o => o.Health <= 0 && !o.Invincible);
            foreach ((int X, int Y) chunk in ChunkObstacleCache.Keys.ToList())
            {
                List<Obstacle> list = ChunkObstacleCache[chunk];
                list.RemoveAll(o => o.Health <= 0 && !o.Invincible);

                if (list.Count == 0)
                    ChunkObstacleCache.Remove(chunk);
            }

            // Change the score if there's a dead enemy
            Characters.RemoveAll(c =>
            {
                if (c.Lives <= 0 && c is Enemy enemy)
                {
                    Score += enemy.ScoreValue;
                    return true;
                }
                else if (c.CharType == Character.Type.Player && c.Lives <= 0)
                {
                    StopRound();
                    return true;
                }
                return false;
            });
        }

        private void NewFrame(object sender, EventArgs e)
        {
            long now = Environment.TickCount64;
            DeltaTime = (now - LastFrameTime) / 1000f; // seconds
            LastFrameTime = now;

            if (_player == null)
                return;

            // Update camera position to the player's
            UpdateCamera();

            // Create a new frame
            RenderFrame();

            // Tell the program to render the game again
            Invalidate();

            // Don't continue if the game is finished/paused
            if (_gameState != GameState.running)
                return;

            // Clean up the dead entities
            CleanupEntities();

            // Create movement-related boolean variables
            bool blnLeftHeld = _keysHeldDown.Contains(Keys.A) || _keysHeldDown.Contains(Keys.Left);
            bool blnRightHeld = _keysHeldDown.Contains(Keys.D) || _keysHeldDown.Contains(Keys.Right);
            bool blnUpHeld = _keysHeldDown.Contains(Keys.W) || _keysHeldDown.Contains(Keys.Up);
            bool blnDownHeld = _keysHeldDown.Contains(Keys.S) || _keysHeldDown.Contains(Keys.Down);

            // Create movement-related int variables
            int intMoveX = 0;
            int intMoveY = 0;

            // Increment/decrement the movement-related int variables based off of the boolean variables
            if (blnLeftHeld)
                intMoveX -= 1;
            if (blnRightHeld)
                intMoveX += 1;
            if (blnUpHeld)
                intMoveY -= 1;
            if (blnDownHeld)
                intMoveY += 1;

            // Multiple the movement-related int variables by the game speed
            intMoveX *= GameSettings.Current.GameSpeedValue;
            intMoveY *= GameSettings.Current.GameSpeedValue;

            // Update the player's cooldown
            _player?.UpdateTimers();

            // Move the player if he should
            if (intMoveX != 0 || intMoveY != 0)
                _player?.Move(intMoveX, intMoveY);

            // Update the projectiles
            foreach (Projectile projectile in Projectiles)
                projectile.Update();

            // Update the enemies
            foreach (Character character in Characters)
                if (character is Enemy enemy)
                    enemy.Move();
        }

        private void ShootMeUp_KeyDown(object sender, KeyEventArgs e)
        {
            // Add the key to the list if it's not already in there
            if (!_keysHeldDown.Contains(e.KeyCode))
                _keysHeldDown.Add(e.KeyCode);

            if (_keysHeldDown.Contains(Keys.Escape))
            {
                DisplayPauseMenu();
                _keysHeldDown.Remove(Keys.Escape);
            }
        }

        private void ShootMeUp_KeyUp(object sender, KeyEventArgs e)
        {
            // Remove the key from the list if it's in there
            if (_keysHeldDown.Contains(e.KeyCode))
                _keysHeldDown.Remove(e.KeyCode);
        }

        private static CFrame ScreenToWorld(Point screen)
        {
            return new CFrame(screen.X / Zoom + CameraX, screen.Y / Zoom + CameraY);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            // Small zoom value = zoom out
            // Big zoom value = zoom in

            Zoom += e.Delta > 0 ? 0.1f : -0.1f;
            Zoom = Math.Clamp(Zoom, 0.5f, 2f);
        }

        private void ShootMeUp_MouseClick(object sender, MouseEventArgs e)
        {
            // Only try and shoot something if the player is in game
            if (_gameState != GameState.running || _player == null) return;

            // If it's a left click, shoot an arrow. Otherwise, shoot a fireball.
            Projectile.Type type = (e.Button == MouseButtons.Right)
                ? Projectile.Type.Fireball_Big
                : Projectile.Type.Arrow_Big;

            CFrame target = ScreenToWorld(e.Location);
            Projectile? projectile = _player.Shoot(target, type);

            if (projectile != null)
                Projectiles.Add(projectile);
        }

        protected override void OnResizeBegin(EventArgs e)
        {
            base.OnResizeBegin(e);
            isResizing = true;
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);
            isResizing = false;

            ResizeBackbuffer();

            UpdateCamera();
            RenderFrame();
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            CenterTitleUI();
            CenterPauseUI();
            CenterSettingsUI();

            if (!isResizing)
            {
                ResizeBackbuffer();

                UpdateCamera();
                RenderFrame();
                Invalidate();
            }
        }
    }
}