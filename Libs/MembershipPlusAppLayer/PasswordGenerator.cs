//
//
// Inspired by work in http://www.codeproject.com/Articles/2393/A-C-Password-Generator
// Generate method rewritten.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Archymeta.Web.MembershipPlus.AppLayer
{
    public class PasswordGenerator
    {
        public PasswordGenerator()
        {
            this.Minimum = DefaultMinimum;
            this.Maximum = DefaultMaximum;
            this.ConsecutiveCharacters = false;
            this.RepeatCharacters = true;
            this.ExcludeSymbols = false;
            this.Exclusions = null;
            rng = new RNGCryptoServiceProvider();
        }

        protected int GetCryptographicRandomNumber(int lBound, int uBound)
        {
            // Assumes lBound >= 0 && lBound < uBound
            // returns an int >= lBound and < uBound
            uint urndnum;
            byte[] rndnum = new Byte[4];
            if (lBound == uBound - 1)
            {
                // test for degenerate case where only lBound can be returned   
                return lBound;
            }

            uint xcludeRndBase = (uint.MaxValue - (uint.MaxValue % (uint)(uBound - lBound)));

            do
            {
                rng.GetBytes(rndnum);
                urndnum = System.BitConverter.ToUInt32(rndnum, 0);
            } while (urndnum >= xcludeRndBase);

            return (int)(urndnum % (uBound - lBound)) + lBound;
        }

        protected char GetRandomCharacter()
        {
            int upperBound = pwdCharArray.GetUpperBound(0);

            if (true == this.ExcludeSymbols)
            {
                upperBound = PasswordGenerator.UBoundDigit;
            }

            int randomCharPosition = GetCryptographicRandomNumber(pwdCharArray.GetLowerBound(0), upperBound);

            char randomChar = pwdCharArray[randomCharPosition];

            return randomChar;
        }

        public string Generate()
        {
            // Pick random length between minimum and maximum   
            int pwdLength = GetCryptographicRandomNumber(this.Minimum, this.Maximum);
            string pwdBuffer = "";
            // Generate random characters
            char lastCharacter, nextCharacter;
            // Initial dummy character flag
            lastCharacter = nextCharacter = '\n';
            Func<char, char, bool> IsInvalid = (l, c) =>
            {
                return !ConsecutiveCharacters && l == c ||
                        !RepeatCharacters && pwdBuffer.Contains(c) ||
                        Exclusions != null && Exclusions.Contains(c);
            };
            while (pwdBuffer.Length < pwdLength)
            {
                nextCharacter = GetRandomCharacter();
                while (IsInvalid(lastCharacter, nextCharacter))
                    nextCharacter = GetRandomCharacter();
                pwdBuffer += nextCharacter;
                lastCharacter = nextCharacter;
            }
            return pwdBuffer;
        }

        public bool Validate(string password)
        {
            int iCount = 0;
            if (password.Length < minSize || password.Length > maxSize)
                return (false);

            // check for Consecutive characters
            if (!hasConsecutive) // cannot have consecutive characters
            {
                for (iCount = 0; iCount < password.Length - 1; iCount++)
                {
                    if (password[iCount] == password[iCount + 1])
                        return (false);
                }
            }

            if (!hasRepeating) // cannot have repeating characters
            {
                for (iCount = 0; iCount < password.Length; iCount++)
                {
                    int index = password.IndexOf(password[iCount]);
                    while (index != -1)
                    {
                        if (index != iCount)
                            return (false);
                        index = password.IndexOf(password[iCount]);
                    }
                }
            }

            if (Exclusions != null)	// cannot have characters from exclusion string
            {
                for (iCount = 0; iCount < password.Length; iCount++)
                {
                    if (Exclusions.IndexOf(password[iCount]) != -1)
                        return (false);
                }
            }

            if (ExcludeSymbols) // cannot contain 'symbols'
            {
                for (iCount = UBoundDigit; iCount < pwdCharArray.GetUpperBound(0); iCount++)
                {
                    if (password.IndexOf(pwdCharArray[iCount]) != -1)
                        return (false);
                }
            }

            return (true);
        }

        public string Exclusions
        {
            get { return this.exclusionSet; }
            set { this.exclusionSet = value; }
        }

        public int Minimum
        {
            get { return this.minSize; }
            set
            {
                this.minSize = value;
                if (PasswordGenerator.DefaultMinimum > this.minSize)
                {
                    this.minSize = PasswordGenerator.DefaultMinimum;
                }
            }
        }

        public int Maximum
        {
            get { return this.maxSize; }
            set
            {
                this.maxSize = value;
                if (this.minSize >= this.maxSize)
                {
                    this.maxSize = PasswordGenerator.DefaultMaximum;
                }
            }
        }

        public bool ExcludeSymbols
        {
            get { return this.hasSymbols; }
            set { this.hasSymbols = value; }
        }

        public bool RepeatCharacters
        {
            get { return this.hasRepeating; }
            set { this.hasRepeating = value; }
        }

        public bool ConsecutiveCharacters
        {
            get { return this.hasConsecutive; }
            set { this.hasConsecutive = value; }
        }

        private const int DefaultMinimum = 6;
        private const int DefaultMaximum = 10;
        private const int UBoundDigit = 61;

        private RNGCryptoServiceProvider rng;
        private int minSize;
        private int maxSize;
        private bool hasRepeating;
        private bool hasConsecutive;
        private bool hasSymbols;
        private string exclusionSet;
        private char[] pwdCharArray = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789`~!@#$%^&*()-_=+[]{}\\|;:'\",<.>/?".ToCharArray();
    }
}
