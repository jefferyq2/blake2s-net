﻿// Copyright notices:
// ---
// Originally Written in 2012 by Christian Winnerlein  <codesinchaos@gmail.com>
// Rewritten Fall 2014 (for the Blake2s flavor instead of the Blake2b flavor) 
//   by Dustin Sparks <sparkdustjoe@gmail.com>

// To the extent possible under law, the author(s) have dedicated all copyright
// and related and neighboring rights to this software to the public domain
// worldwide. This software is distributed without any warranty.

// You should have received a copy of the CC0 Public Domain Dedication along with
// this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// ---
// Based on BlakeSharp
// by Dominik Reichl <dominik.reichl@t-online.de>
// Web: http://www.dominik-reichl.de/
// If you're using this class, it would be nice if you'd mention
// me somewhere in the documentation of your program, but it's
// not required.
// BLAKE was designed by Jean-Philippe Aumasson, Luca Henzen,
// Willi Meier and Raphael C.-W. Phan.
// BlakeSharp was derived from the reference C implementation.
// ---
// This implementation is based on: https://github.com/SparkDustJoe/miniLockManaged
// Reason:
// - Have a single nuget package for Blake2s
// - Added libsodium-net similar interface for the hash functions. 

using System;

namespace Blake2s
{
    /// <summary>
    /// Class to build the IV.
    /// </summary>
    internal static class Blake2sIvBuilder
    {
        /// <summary>
        /// </summary>
        /// <param name="config">A valid Blake2sConfig.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static uint[] ConfigS(Blake2sConfig config)
        {
            var rawConfig = new uint[8];
            //digest length
            if (config.OutputSizeInBytes <= 0 | config.OutputSizeInBytes > 32) //
                throw new ArgumentOutOfRangeException("config.OutputSize");
            rawConfig[0] |= (uint) config.OutputSizeInBytes; //

            //Key length
            if (config.Key != null)
            {
                if (config.Key.Length > 32) //
                    throw new ArgumentException("config.Key", "Key too long");
                rawConfig[0] |= (uint) (config.Key.Length << 8); //
            }
            // Fan Out =1 and Max Height / Depth = 1
            rawConfig[0] |= 1 << 16;
            rawConfig[0] |= 1 << 24;
            // Leaf Length and Inner Length 0, no need to worry about them
            // Salt
            if (config.Salt != null)
            {
                if (config.Salt.Length != 8)
                    throw new ArgumentException("config.Salt has invalid length");
                rawConfig[4] = Blake2sCore.BytesToUInt32(config.Salt, 0);
                rawConfig[5] = Blake2sCore.BytesToUInt32(config.Salt, 4);
            }
            // Personalization
            if (config.Personalization != null)
            {
                if (config.Personalization.Length != 8)
                    throw new ArgumentException("config.Personalization has invalid length");
                rawConfig[6] = Blake2sCore.BytesToUInt32(config.Personalization, 0);
                rawConfig[7] = Blake2sCore.BytesToUInt32(config.Personalization, 4);
            }

            return rawConfig;
        }
    }
}