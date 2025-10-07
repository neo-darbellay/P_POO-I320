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
    public class CharacterHandler
    {
        /// <summary>
        /// The list of all the characters
        /// </summary>
        private static List<Character> _lst_Characters = new List<Character>();

        /// <summary>
        /// The list of all the characters
        /// </summary>
        public List<Character> Characters { get { return _lst_Characters; } }

        /// <summary>
        /// Create a new CollisionHandler
        /// </summary>
        public CharacterHandler() { }

        /// <summary>
        /// Add a new character to the list
        /// </summary>
        /// <param name="newCharacters">The character to add</param>
        public void AddCharacter(Character newCharacters)
        {
            _lst_Characters.Add(newCharacters);
        }

        /// <summary>
        /// Removes a character from the character list
        /// </summary>
        /// <param name="character"></param>
        public void RemoveCharacter(Character character)
        {
            _lst_Characters.Remove(character);
        }

        /// <summary>
        /// Remove every character
        /// Useful for restarting the game
        /// </summary>
        public void RemoveAllCharacters()
        {
            _lst_Characters.Clear();
        }
    }
}
