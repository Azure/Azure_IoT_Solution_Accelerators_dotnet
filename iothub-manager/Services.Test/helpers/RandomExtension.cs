// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Text;

namespace Services.Test.helpers
{
    /* <summary>
     * This class is a Extension of Random class which is used by Test Classes
     * to generate the next random string
     * </summary>     */
    public static class RandomExtension
    {
        private const string CHARACTERS = @"0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static string NextString(this Random rand, int length = 32, string characters = CHARACTERS)
        {
            var builder = new StringBuilder();

            while (builder.Length < length)
            {
                builder.Append(characters[rand.Next(0, characters.Length)]);
            }

            return builder.ToString();
        }
    }
}
