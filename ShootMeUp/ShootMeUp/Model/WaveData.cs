using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShootMeUp.Model
{
    /// <summary>
    /// This class is used to store every wave's enemies
    /// </summary>
    public class WaveData
    {
        /// <summary>
        /// The game's speed multiplier for movement, projectiles, etc.)
        /// </summary>
        private static int _GAMESPEED;

        /// <summary>
        /// The default size for characters and enemies
        /// </summary>
        private static int _DEFAULT_CHARACTER_SIZE;

        /// <summary>
        /// Create a new instance of WaveData
        /// </summary>
        public WaveData(int DEFAULT_CHARACTER_SIZE, int GAMESPEED)
        {
            _DEFAULT_CHARACTER_SIZE = DEFAULT_CHARACTER_SIZE;
            _GAMESPEED = GAMESPEED;
        }

        /// <summary>
        /// Get the current wave's enemies
        /// </summary>
        /// <param name="intWaveNumber">The current wave's number</param>
        /// <returns></returns>
        public List<Enemy> GetWaveEnemies(int intWaveNumber)
        {
            /* ALL THE ENEMIES
            new Enemy(0, 0, _DEFAULT_CHARACTER_SIZE, "zombie", _GAMESPEED)
            new Enemy(0, 0, _DEFAULT_CHARACTER_SIZE, "skeleton", _GAMESPEED)
            new Enemy(0, 0, (int)(_DEFAULT_CHARACTER_SIZE * 0.75), "babyzombie", _GAMESPEED)
            new Enemy(0, 0, _DEFAULT_CHARACTER_SIZE, "blaze", -GAMESPEED)
            new Enemy(0, 0, _DEFAULT_CHARACTER_SIZE, "zombiepigman", _GAMESPEED)
            */

            List<Enemy> WaveEnemies = new List<Enemy>();

            switch (intWaveNumber)
            {
                case 1:
                    WaveEnemies.Add(new Enemy(0, 0, _DEFAULT_CHARACTER_SIZE, "zombie", _GAMESPEED));
                    WaveEnemies.Add(new Enemy(0, 0, _DEFAULT_CHARACTER_SIZE, "zombie", _GAMESPEED));
                    WaveEnemies.Add(new Enemy(0, 0, _DEFAULT_CHARACTER_SIZE, "zombie", _GAMESPEED));

                    break;
                case 2:
                    WaveEnemies.Add(new Enemy(0, 0, _DEFAULT_CHARACTER_SIZE, "zombie", _GAMESPEED));
                    WaveEnemies.Add(new Enemy(0, 0, _DEFAULT_CHARACTER_SIZE, "skeleton", _GAMESPEED));
                    WaveEnemies.Add(new Enemy(0, 0, _DEFAULT_CHARACTER_SIZE, "zombie", _GAMESPEED));

                    break;
                case 3:
                    WaveEnemies.Add(new Enemy(0, 0, _DEFAULT_CHARACTER_SIZE, "skeleton", _GAMESPEED));
                    WaveEnemies.Add(new Enemy(0, 0, _DEFAULT_CHARACTER_SIZE, "zombie", _GAMESPEED));
                    WaveEnemies.Add(new Enemy(0, 0, _DEFAULT_CHARACTER_SIZE, "skeleton", _GAMESPEED));
                    WaveEnemies.Add(new Enemy(0, 0, _DEFAULT_CHARACTER_SIZE, "zombie", _GAMESPEED));

                    break;
                case 4:
                    WaveEnemies.Add(new Enemy(0, 0, _DEFAULT_CHARACTER_SIZE, "skeleton", _GAMESPEED));
                    WaveEnemies.Add(new Enemy(0, 0, (int)(_DEFAULT_CHARACTER_SIZE * 0.75), "babyzombie", _GAMESPEED));
                    WaveEnemies.Add(new Enemy(0, 0, _DEFAULT_CHARACTER_SIZE, "zombie", _GAMESPEED));

                    break;
                case 5:
                    WaveEnemies.Add(new Enemy(0, 0, (int)(_DEFAULT_CHARACTER_SIZE * 0.75), "babyzombie", _GAMESPEED));
                    WaveEnemies.Add(new Enemy(0, 0, (int)(_DEFAULT_CHARACTER_SIZE * 0.75), "babyzombie", _GAMESPEED));
                    WaveEnemies.Add(new Enemy(0, 0, (int)(_DEFAULT_CHARACTER_SIZE * 0.75), "babyzombie", _GAMESPEED));
                    WaveEnemies.Add(new Enemy(0, 0, (int)(_DEFAULT_CHARACTER_SIZE * 0.75), "babyzombie", _GAMESPEED));
                    WaveEnemies.Add(new Enemy(0, 0, (int)(_DEFAULT_CHARACTER_SIZE * 0.75), "babyzombie", _GAMESPEED));

                    break;

                default:
                    break;
            }

            return WaveEnemies;
        }
    }
}
