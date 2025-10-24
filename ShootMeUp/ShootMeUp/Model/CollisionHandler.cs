using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShootMeUp.Model
{
    /// <summary>
    /// This class is used to run checks to see if two objects would collide in a 2d environment
    /// </summary>
    public class CollisionHandler : Handler
    {
        /// <summary>
        /// The list of all the obstacles
        /// </summary>
        private static List<Obstacle> _lst_Obstacles = new List<Obstacle>();

        /// <summary>
        /// The list of all the obstacles
        /// </summary>
        public List<Obstacle> Obstacles { get { return _lst_Obstacles; } }

        /// <summary>
        /// Create a new CollisionHandler
        /// </summary>
        public CollisionHandler() : base() { }

        /// <summary>
        /// Add a new obstacle to the list
        /// </summary>
        /// <param name="NewObstacle">The obstacle to add</param>
        public void AddObstacle(Obstacle NewObstacle)
        {
            _lst_Obstacles.Add(NewObstacle);
        }

        /// <summary>
        /// Removes an obstacle from the obstacle list
        /// </summary>
        /// <param name="obstacle"></param>
        public void RemoveObstacle(Obstacle obstacle)
        {
            _lst_Obstacles.Remove(obstacle);
        }

        /// <summary>
        /// Remove every obstacle
        /// Useful for restarting the game
        /// </summary>
        public void RemoveAllObstacles()
        {
            _lst_Obstacles.Clear();
        }

        /// <summary>
        /// Checks if the given coordinate frame of an object would colide with an obstacle's
        /// </summary>
        /// <param name="Coordinates">The coordinate frame of an object</param>
        /// <param name="fltXMovement">The movement that will happen in the X axis</param>
        /// <param name="fltYMovement">The movement that will happen in the Y axis</param>
        /// <returns>A bool table, where index 0 is the X axis, and index 1 is the Y axis. </returns>
        public bool[] CheckForCollisions(CFrame Coordinates, float fltXMovement, float fltYMovement)
        {
            bool[] blnColliding = new bool[2] { false, false };

            // Create hypothetical CFrames to simulate movement along each axis independently
            CFrame cfrX = new CFrame(Coordinates.FloatX + fltXMovement, Coordinates.FloatY, Coordinates.length, Coordinates.height);
            CFrame cfrY = new CFrame(Coordinates.FloatX, Coordinates.FloatY + fltYMovement, Coordinates.length, Coordinates.height);

            foreach (Obstacle obstacle in _lst_Obstacles)
            {
                if (IsOverlapping(cfrX, obstacle))
                {
                    blnColliding[0] = true; // Collision if moved along X axis
                }

                if (IsOverlapping(cfrY, obstacle))
                {
                    blnColliding[1] = true; // Collision if moved along Y axis
                }

                // Early exit if both collisions detected
                if (blnColliding[0] && blnColliding[1])
                    break;
            }

            return blnColliding;
        }

        /// <summary>
        /// Return the colliding obstacle if there are any
        /// </summary>
        /// <param name="Coordinates">The coordinate frame of an object</param>
        /// <param name="fltXMovement">The movement that will happen in the X axis</param>
        /// <param name="fltYMovement">The movement that will happen in the Y axis</param>
        /// <returns>The colliding obstacle, if any</returns>
        public Obstacle? GetCollidingObject(CFrame Coordinates, float fltXMovement, float fltYMovement)
        {
            // Create hypothetical CFrames to simulate movement along each axis independently
            CFrame cfrX = new CFrame(Coordinates.FloatX + fltXMovement, Coordinates.FloatY, Coordinates.length, Coordinates.height);
            CFrame cfrY = new CFrame(Coordinates.FloatX, Coordinates.FloatY + fltYMovement, Coordinates.length, Coordinates.height);

            foreach (Obstacle obstacle in _lst_Obstacles)
            {
                if (IsOverlapping(cfrX, obstacle))
                {
                    return obstacle;
                }

                if (IsOverlapping(cfrY, obstacle))
                {
                    return obstacle;
                }
            }

            return null;
        }
    }
}
