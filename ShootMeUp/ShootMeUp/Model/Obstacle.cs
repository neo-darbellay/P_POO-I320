///ETML
///10.11.2025
///This is the obstacle class
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
        /// The obstacle's type (Barrier, ...)
        /// </summary>
        public enum Type
        {
            Dirt,
            Wood,
            CobbleStone,

            Barrier,
            Bedrock,

            Spawner,
            Bush,
            Grass,
            Stone,
            Sand,

            Undefined
        }

        /// <summary>
        /// The obstacle's health (set to int.MaxValue if invincible)
        /// </summary>
        public int Health { get; set; }

        /// <summary>
        /// Whether the obstacle is invincible or not
        /// </summary>
        private readonly bool _invincible;
        public bool Invincible
        {
            get { return _invincible; }
        }

        // <summary>
        /// The obstacle's current type
        /// </summary>
        public Type ObstType { get; private set; }

        /// <summary>
        /// The obstacle constructor
        /// </summary>
        /// <param name="x">The obstacle's X pos</param>
        /// <param name="y">The obstacle's Y pos</param>
        /// <param name="intLength">The obstacle's Length</param>
        /// <param name="type">The obstacle's type (Bush, Barrier, ...)</param>
        public Obstacle(float x, float y, int intLength, Obstacle.Type type) : base(x, y, intLength)
        {
            switch (type)
            {
                case Type.Dirt:
                    Health = 5;
                    break;
                case Type.Wood:
                    Health = 10;
                    break;
                case Type.CobbleStone:
                    Health = 25;
                    break;

                case Type.Barrier:
                    _invincible = true;
                    Health = int.MaxValue;
                    break;
                case Type.Bedrock:
                    _invincible = true;
                    break;

                case Type.Spawner:
                    _invincible = true;
                    CanCollide = false;
                    break;
                case Type.Bush:
                    _invincible = true;
                    CanCollide = false;
                    Health = int.MaxValue;
                    break;

                case Type.Grass:
                    CanCollide = false;
                    _invincible = true;
                    break;
                case Type.Stone:
                    CanCollide = false;
                    _invincible = true;
                    break;
                case Type.Sand:
                    CanCollide = false;
                    _invincible = true;
                    break;
                default:
                    CanCollide = false;
                    _invincible = true;
                    break;
            }            

            ObstType = type;
        }

        /// <summary>
        /// Permit to get the dispayed status
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (_invincible)
            {
                return "";
            }

            if (Health > 0)
                return $"{Health} HP";
            else
                return "";
        }
    }
}
