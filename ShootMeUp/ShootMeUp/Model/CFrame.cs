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

        private bool _blnFloatPosition;

        private float _fltX;
        private float _fltY;

        private int _intLength;
        private int _intHeight;

        public int X
        {
            get { return _intX; }
            set { _intX = value; }
        }

        public int Y
        {
            get { return _intY; }
            set { _intY = value; }
        }

        public float FloatX
        {
            get { return _fltX; }
            set { _fltX = value; }
        }

        public float FloatY
        {
            get { return _fltY; }
            set { _fltY = value; }
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
            _intX = X;
            _intY = Y;

            _blnFloatPosition = false;

            _fltX = 0;
            _fltY = 0;

            _intLength = intLength;
            _intHeight = intHeight;
        }

        public CFrame(float X, float Y, int intLength, int intHeight, bool blnFloatPosition)
        {
            _intX = (int)X;
            _intY = (int)Y;

            _blnFloatPosition = true;

            _fltX = X;
            _fltY = Y;

            _intLength = intLength;
            _intHeight = intHeight;
        }

        public override string ToString()
        {
            if (_blnFloatPosition)
                return $"{{{FloatX},{FloatY}}},{{{length},{height}}}";
            else
                return $"{{{X},{Y}}},{{{length},{height}}}";
        }
    }
}
