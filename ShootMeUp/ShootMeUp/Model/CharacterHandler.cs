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
    public class CharacterHandler : Handler
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
        /// Create a new CharacterHandler
        /// </summary>
        public CharacterHandler() : base() { }

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

        /// <summary>
        /// Checks if the given coordinate frame of an object would colide with a character's
        /// </summary>
        /// <param name="Coordinates">The coordinate frame of an object</param>
        /// <param name="fltXMovement">The movement that will happen in the X axis</param>
        /// <param name="fltYMovement">The movement that will happen in the Y axis</param>
        /// <param name="ignoredCharacter">The ignored character, for example, the character that shot the projectile</param>
        /// <returns>A bool table, where index 0 is the X axis, and index 1 is the Y axis. </returns>
        public bool[] CheckForCollisions(CFrame Coordinates, float fltXMovement, float fltYMovement, Character ignoredCharacter)
        {
            bool[] blnColliding = new bool[2] { false, false };

            // Create hypothetical CFrames to simulate movement along each axis independently
            CFrame cfrX = new CFrame(Coordinates.FloatX + fltXMovement, Coordinates.FloatY, Coordinates.length, Coordinates.height);
            CFrame cfrY = new CFrame(Coordinates.FloatX, Coordinates.FloatY + fltYMovement, Coordinates.length, Coordinates.height);

            foreach (Character character in _lst_Characters)
            {
                // Skip the ignored character
                if (character == ignoredCharacter)
                    continue;

                if (IsOverlapping(cfrX, character))
                {
                    blnColliding[0] = true; // Collision if moved along X axis
                }

                if (IsOverlapping(cfrY, character))
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
        /// Checks if the given coordinate frame of an object would colide with a character's
        /// </summary>
        /// <param name="Coordinates">The coordinate frame of an object</param>
        /// <param name="fltXMovement">The movement that will happen in the X axis</param>
        /// <param name="fltYMovement">The movement that will happen in the Y axis</param>
        /// <param name="ignoredCharacter">The ignored character, for example, the character that shot the projectile</param>
        /// <param name="strWantedType">The colliding character's string</param>
        /// <returns>A bool table, where index 0 is the X axis, and index 1 is the Y axis. </returns>
        public bool[] CheckForCollisions(CFrame Coordinates, float fltXMovement, float fltYMovement, Character ignoredCharacter, string strWantedType)
        {
            bool[] blnColliding = new bool[2] { false, false };

            // Create hypothetical CFrames to simulate movement along each axis independently
            CFrame cfrX = new CFrame(Coordinates.FloatX + fltXMovement, Coordinates.FloatY, Coordinates.length, Coordinates.height);
            CFrame cfrY = new CFrame(Coordinates.FloatX, Coordinates.FloatY + fltYMovement, Coordinates.length, Coordinates.height);

            foreach (Character character in _lst_Characters)
            {
                // Skip the ignored character
                if (character == ignoredCharacter || character.Type != strWantedType)
                    continue;

                if (IsOverlapping(cfrX, character))
                {
                    blnColliding[0] = true; // Collision if moved along X axis
                }

                if (IsOverlapping(cfrY, character))
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
        /// Return the colliding character if there are any
        /// </summary>
        /// <param name="Coordinates">The coordinate frame of an object</param>
        /// <param name="fltXMovement">The movement that will happen in the X axis</param>
        /// <param name="fltYMovement">The movement that will happen in the Y axis</param>
        /// <param name="ignoredCharacter">The ignored character, for example, the character that shot the projectile</param>
        /// <returns>The colliding character, if any</returns>
        public Character? GetCollidingCharacter(CFrame Coordinates, float fltXMovement, float fltYMovement, Character ignoredCharacter)
        {
            // Create hypothetical CFrames to simulate movement along each axis independently
            CFrame cfrX = new CFrame(Coordinates.FloatX + fltXMovement, Coordinates.FloatY, Coordinates.length, Coordinates.height);
            CFrame cfrY = new CFrame(Coordinates.FloatX, Coordinates.FloatY + fltYMovement, Coordinates.length, Coordinates.height);

            foreach (Character character in _lst_Characters)
            {
                // Skip the ignored character
                if (character == ignoredCharacter)
                    continue;

                if (IsOverlapping(cfrX, character))
                {
                    return character;
                }

                if (IsOverlapping(cfrY, character))
                {
                    return character;
                }
            }

            return null;
        }

        /// <summary>
        /// Return the colliding character if there are any
        /// </summary>
        /// <param name="Coordinates">The coordinate frame of an object</param>
        /// <param name="fltXMovement">The movement that will happen in the X axis</param>
        /// <param name="fltYMovement">The movement that will happen in the Y axis</param>
        /// <param name="ignoredCharacter">The ignored character, for example, the character that shot the projectile</param>
        /// <param name="strWantedType">The colliding character's string</param>
        /// <returns>The colliding character, if any</returns>
        public Character? GetCollidingCharacter(CFrame Coordinates, float fltXMovement, float fltYMovement, Character ignoredCharacter, string strWantedType)
        {
            // Create hypothetical CFrames to simulate movement along each axis independently
            CFrame cfrX = new CFrame(Coordinates.FloatX + fltXMovement, Coordinates.FloatY, Coordinates.length, Coordinates.height);
            CFrame cfrY = new CFrame(Coordinates.FloatX, Coordinates.FloatY + fltYMovement, Coordinates.length, Coordinates.height);

            foreach (Character character in _lst_Characters)
            {
                // Skip the ignored character
                if (character == ignoredCharacter || character.Type != strWantedType)
                    continue;

                if (IsOverlapping(cfrX, character))
                {
                    return character;
                }

                if (IsOverlapping(cfrY, character))
                {
                    return character;
                }
            }

            return null;
        }
    }
}
