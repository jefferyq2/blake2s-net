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
using System.Text;
using Blake2s.Exceptions;

namespace Blake2s
{
    /// <summary>
    /// BLAKE2s hash function.
    /// </summary>
    public static class Blake2S
    {
        private const int BytesMin = 1;
        private const int BytesMax = 64;
        private const int KeyBytesMin = 16;
        private const int KeyBytesMax = 64;
        private const int OutBytes = 64;
        private const int SaltBytes = 16;
        private const int PersonalBytes = 16;

        public static Hasher Create()
        {
            return Create(new Blake2sConfig());
        }

        public static Hasher Create(Blake2sConfig config)
        {
            return new Blake2sHasher(config);
        }

        public static byte[] ComputeHash(byte[] data, int start, int count)
        {
            return ComputeHash(data, start, count, null);
        }

        public static byte[] ComputeHash(byte[] data)
        {
            return ComputeHash(data, 0, data.Length, null);
        }

        public static byte[] ComputeHash(byte[] data, Blake2sConfig config)
        {
            return ComputeHash(data, 0, data.Length, config);
        }

        public static byte[] ComputeHash(byte[] data, int start, int count, Blake2sConfig config)
        {
            var hasher = Create(config);
            hasher.Update(data, start, count);
            return hasher.Finish();
        }

        /// <summary>Hashes a message, with an optional key, using the BLAKE2s primitive.</summary>
        /// <param name="message">The message to be hashed.</param>
        /// <param name="key">The key; may be null, otherwise between 16 and 64 bytes.</param>
        /// <param name="bytes">The size (in bytes) of the desired result.</param>
        /// <returns>Returns a byte array.</returns>
        /// <exception cref="KeyOutOfRangeException"></exception>
        /// <exception cref="BytesOutOfRangeException"></exception>
        /// <exception cref="OverflowException"></exception>
        public static byte[] Hash(string message, string key, int bytes)
        {
            return Hash(message, Encoding.UTF8.GetBytes(key), bytes);
        }

        /// <summary>Hashes a message, with an optional key, using the BLAKE2s primitive.</summary>
        /// <param name="message">The message to be hashed.</param>
        /// <param name="key">The key; may be null, otherwise between 16 and 64 bytes.</param>
        /// <param name="bytes">The size (in bytes) of the desired result.</param>
        /// <returns>Returns a byte array.</returns>
        /// <exception cref="KeyOutOfRangeException"></exception>
        /// <exception cref="BytesOutOfRangeException"></exception>
        /// <exception cref="OverflowException"></exception>
        public static byte[] Hash(string message, byte[] key, int bytes)
        {
            return Hash(Encoding.UTF8.GetBytes(message), key, bytes);
        }

        /// <summary>Hashes a message, with an optional key, using the BLAKE2s primitive.</summary>
        /// <param name="message">The message to be hashed.</param>
        /// <param name="key">The key; may be null, otherwise between 16 and 64 bytes.</param>
        /// <param name="bytes">The size (in bytes) of the desired result.</param>
        /// <returns>Returns a byte array.</returns>
        /// <exception cref="KeyOutOfRangeException"></exception>
        /// <exception cref="BytesOutOfRangeException"></exception>
        /// <exception cref="OverflowException"></exception>
        public static byte[] Hash(byte[] message, byte[] key, int bytes)
        {
            //validate the length of the key
            if (key != null)
            {
                if (key.Length > KeyBytesMax || key.Length < KeyBytesMin)
                {
                    throw new KeyOutOfRangeException(string.Format("key must be between {0} and {1} bytes in length.",
                        KeyBytesMin, KeyBytesMax));
                }
            }
            else
            {
                key = new byte[0];
            }

            //validate output length
            if (bytes > BytesMax || bytes < BytesMin)
                throw new BytesOutOfRangeException("bytes", bytes,
                    string.Format("bytes must be between {0} and {1} bytes in length.", BytesMin, BytesMax));

            var config = new Blake2sConfig
            {
                Key = key,
                OutputSizeInBytes = bytes
            };

            if (message == null)
            {
                message = new byte[0];
            }

            return ComputeHash(message, 0, message.Length, config);
        }

        /// <summary>Generates a hash based on a key, salt and personal strings</summary>
        /// <returns><c>byte</c> hashed message</returns>
        /// <param name="message">Message.</param>
        /// <param name="key">Key.</param>
        /// <param name="salt">Salt.</param>
        /// <param name="personal">Personal.</param>
        /// <param name="bytes">The size (in bytes) of the desired result.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="KeyOutOfRangeException"></exception>
        /// <exception cref="SaltOutOfRangeException"></exception>
        /// <exception cref="PersonalOutOfRangeException"></exception>
        /// <exception cref="BytesOutOfRangeException"></exception>
        public static byte[] HashSaltPersonal(string message, string key, string salt, string personal, int bytes = OutBytes)
        {
            return HashSaltPersonal(Encoding.UTF8.GetBytes(message), Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(salt), Encoding.UTF8.GetBytes(personal), bytes);
        }

        /// <summary>Generates a hash based on a key, salt and personal bytes</summary>
        /// <returns><c>byte</c> hashed message</returns>
        /// <param name="message">Message.</param>
        /// <param name="key">Key.</param>
        /// <param name="salt">Salt.</param>
        /// <param name="personal">Personal string.</param>
        /// <param name="bytes">The size (in bytes) of the desired result.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="KeyOutOfRangeException"></exception>
        /// <exception cref="SaltOutOfRangeException"></exception>
        /// <exception cref="PersonalOutOfRangeException"></exception>
        /// <exception cref="BytesOutOfRangeException"></exception>
        public static byte[] HashSaltPersonal(byte[] message, byte[] key, byte[] salt, byte[] personal, int bytes = OutBytes)
        {
            if (message == null)
                throw new ArgumentNullException("message", "Message cannot be null");

            if (salt == null)
                throw new ArgumentNullException("salt", "Salt cannot be null");

            if (personal == null)
                throw new ArgumentNullException("personal", "Personal string cannot be null");

            if (key != null && (key.Length > KeyBytesMax || key.Length < KeyBytesMin))
                throw new KeyOutOfRangeException(string.Format("key must be between {0} and {1} bytes in length.", KeyBytesMin, KeyBytesMax));

            if (key == null)
                key = new byte[0];

            if (salt.Length != SaltBytes)
                throw new SaltOutOfRangeException(string.Format("Salt must be {0} bytes in length.", SaltBytes));

            if (personal.Length != PersonalBytes)
                throw new PersonalOutOfRangeException(string.Format("Personal bytes must be {0} bytes in length.", PersonalBytes));

            //validate output length
            if (bytes > BytesMax || bytes < BytesMin)
                throw new BytesOutOfRangeException("bytes", bytes,
                  string.Format("bytes must be between {0} and {1} bytes in length.", BytesMin, BytesMax));

            var config = new Blake2sConfig
            {
                Key = key,
                OutputSizeInBytes = bytes,
                Personalization = personal,
                Salt = salt
            };

            return ComputeHash(message, 0, message.Length, config);
        }
    }
}