using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReverseHash
{
    public class Program
    {

        static uint HashString(char[] input)
        {
            uint FNV_prime = 16777619;
            uint offset_basis = 2166136261;
            uint hash = offset_basis;
            byte c = 0;
            foreach (char c_ in input)
            {
                c = (byte)c_;
                c |= 0x20;
                hash ^= c;
                hash *= FNV_prime;
            }
            return hash;
        }

        public static void PrintMatches(
            UInt32 hash,
            string possibleCharacters,
            int targetStringSizeBegin,
            int targetStringSizeEnd,
            string prefix
        )
        {
            DateTime start = DateTime.Now;
            Console.WriteLine("Searching matches for 0x{0:x}\n with prefix '{1}'\n using char set '{2}'\n startLen: {3}\n endLen: {4} (range: {5})",
                hash, prefix, possibleCharacters, targetStringSizeBegin, targetStringSizeEnd, (targetStringSizeEnd - targetStringSizeBegin));
            for (int i = targetStringSizeBegin; i < targetStringSizeEnd; i++)
            {
                Console.WriteLine("searching string length = {0}; guess_len:{1}", i, i - prefix.Length);
                PrintMatches(hash, possibleCharacters, i, prefix);
            }
            DateTime end = DateTime.Now;
            var timeTaken = end - start;
            Console.WriteLine("Finished Searching matches for 0x{0:x}; time: {1}ms ", hash, timeTaken.Milliseconds);
        }

        static void PrintMatches(uint hash, string possibleCharacters, int targetStringSize, string prefix)
        {
            char[] bufferToUse = new char[targetStringSize];
            prefix.CopyTo(0, bufferToUse, 0, prefix.Length);
            GenerateStringsAndCheckHash(prefix.Length, targetStringSize, hash, possibleCharacters, bufferToUse);
        }

        static void GenerateStringsAndCheckHash(int index, int targetStringSize, uint targetHash, string possibleCharacters, char[] bufferToUse)
        {
            if (index == targetStringSize)
            {
                if (HashString(bufferToUse) == targetHash)
                {
                    Console.WriteLine("Match found: " + new string(bufferToUse));
                }
            }
            else
            {
                foreach (char c in possibleCharacters)
                {
                    bufferToUse[index] = c;
                    GenerateStringsAndCheckHash(index + 1, targetStringSize, targetHash, possibleCharacters, bufferToUse);
                }
            }
        }

        static void Main(string[] args)
        {
            uint hash = 0;
            string possibleCharacters = "abcdefghijklmnopqrstuvwxyz.0123456789";
            int maxGuessLen = 8;
            string prefix = "";

            // Parse command line arguments
            if (args.Length < 1)
            {
                Console.Error.WriteLine("Usage: ReverseHash <hash> [-c:<character_set>] [-l:<string_length>] [-p:<prefix>]");
                Console.Error.WriteLine("Defaults:[-c:{0}] [-l:{1}]", possibleCharacters, maxGuessLen);
                Environment.Exit(1);
            }
            else
            {
                string hashStr = args[0];
                if (hashStr.StartsWith("0x"))
                    hash = Convert.ToUInt32(hashStr.Substring(2), 16);
                else
                    hash = Convert.ToUInt32(hashStr);
                
                for (int i = 1; i < args.Length; i++)
                {
                    string arg = args[i];
                    if (arg.StartsWith("-c:"))
                    {
                        possibleCharacters = arg.Substring(3);
                    }
                    else if (arg.StartsWith("-l:"))
                    {
                        maxGuessLen = Convert.ToInt32(arg.Substring(3));
                    }
                    else if (arg.StartsWith("-p:"))
                    {
                        prefix = arg.Substring(3);
                    }
                    else
                    {
                        Console.Error.WriteLine("Invalid argument: " + arg);
                        Environment.Exit(1);
                    }
                }
            }

            //PrintMatches(hash, possibleCharacters, targetStringSize, prefix);
            PrintMatches( hash, possibleCharacters, 
                prefix.Length + 1, prefix.Length + maxGuessLen, prefix );
        }
    }
}
