using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShootMeUp.Model
{
    /// <summary>
    /// This class is used to store every projectile
    /// </summary>
    public class ProjectileHandler
    {
        /// <summary>
        /// The list of all the projectiles
        /// </summary>
        private static List<Projectile> _lst_Projectiles = new List<Projectile>();

        /// <summary>
        /// The list of all the projectiles
        /// </summary>
        public List<Projectile> Projectiles { get { return _lst_Projectiles; } }

        /// <summary>
        /// Create a new ProjectileHandler
        /// </summary>
        public ProjectileHandler() : base() { }

        /// <summary>
        /// Add a new projectile to the list
        /// </summary>
        /// <param name="newProjectile">The projectile to add</param>
        public void AddProjectile(Projectile newProjectile)
        {
            _lst_Projectiles.Add(newProjectile);
        }

        /// <summary>
        /// Removes a projectile from the projectile list
        /// </summary>
        /// <param name="projectile">The projectile to remove</param>
        public void RemoveProjectile(Projectile projectile)
        {
            _lst_Projectiles.Remove(projectile);
        }

        /// <summary>
        /// Remove every projectile
        /// Useful for restarting the game
        /// </summary>
        public void RemoveAllProjectiles()
        {
            _lst_Projectiles.Clear();
        }
    }
}
