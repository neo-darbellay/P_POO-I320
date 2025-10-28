using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShootMeUp.Model
{
    /// <summary>
    /// A basic Coordinate frame system
    /// </summary>
    public class CFrame
    {
        private int _intX;
        private int _intY;

        private float _fltX;
        private float _fltY;

        private int _intLength;
        private int _intHeight;

        public float FloatX
        {
            get { return _fltX; }
            set
            {
                _fltX = value;
                _intX = (int)value;
            }
        }

        public float FloatY
        {
            get { return _fltY; }
            set
            {
                _fltY = value;
                _intY = (int)value;
            }
        }

        public int X
        {
            get { return _intX; }
            set
            {
                _intX = value;
                _fltX = value;
            }
        }

        public int Y
        {
            get { return _intY; }
            set
            {
                _intY = value;
                _fltY = value;
            }
        }

        public int length
        {
            get { return _intLength; }
            set { _intLength = value; }
        }

        public int height
        {
            get { return _intHeight; }
            set { _intHeight = value; }
        }

        public CFrame(int X, int Y, int intLength, int intHeight)
        {
            this.X = X;
            this.Y = Y;

            _intLength = intLength;
            _intHeight = intHeight;
        }

        public CFrame(float X, float Y, int intLength, int intHeight)
        {
            FloatX = X;
            FloatY = Y;

            _intLength = intLength;
            _intHeight = intHeight;
        }

        public CFrame(int X, int Y, int intLength)
        {
            this.X = X;
            this.Y = Y;

            _intLength = intLength;
            _intHeight = intLength;
        }

        public CFrame(float X, float Y, int intLength)
        {
            FloatX = X;
            FloatY = Y;

            _intLength = intLength;
            _intHeight = intLength;
        }

        public override string ToString()
        {
            return $"{{{FloatX},{FloatY}}},{{{length},{height}}}";
        }
    }
}
