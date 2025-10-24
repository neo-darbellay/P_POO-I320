using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShootMeUp.Model
{
    /// <summary>
    /// This class is used to store every character
    /// </summary>
    public class Handler
    {
        /// <summary>
        /// Create a new Handler
        /// </summary>
        public Handler() { }

        /// <summary>
        /// A helper method to check if two CFrames overlap
        /// </summary>
        /// <param name="cfrA">The first CFrame</param>
        /// <param name="cfrB">The second CFrame</param>
        /// <returns></returns>
        protected bool IsOverlapping(CFrame cfrA, CFrame cfrB)
        {
            bool overlapX = cfrA.FloatX < cfrB.FloatX + cfrB.length && cfrA.FloatX + cfrA.length > cfrB.FloatX;
            bool overlapY = cfrA.FloatY < cfrB.FloatY + cfrB.height && cfrA.FloatY + cfrA.height > cfrB.FloatY;
            return overlapX && overlapY;
        }
    }
}
