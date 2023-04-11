using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ReverseHash
{

    public interface IAppendText
    {
        void AppendText(string s);
    }

    public class Program
    {
        static string possibleCharacters = "abcdefghijklmnopqrstuvwxyz.0123456789";
        static int maxGuessLen = 8;
        static void Main(string[] args)
        {
            uint hash = 0;

            string prefix = "";

            // Parse command line arguments
            if (args.Length == 0)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new ReverseHashForm());
            }
            else
            {
                if (args[0].StartsWith("-h"))
                {
                    PrintHelp();
                }
                    
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

                //PrintMatches(hash, possibleCharacters, targetStringSize, prefix);
                HashReverse hr = new HashReverse();
                hr.PrintMatches(hash, possibleCharacters,
                    prefix.Length + 1, prefix.Length + maxGuessLen, prefix);
            }
        }

        private static void PrintHelp()
        {
            Console.Error.WriteLine("Usage: ReverseHash <hash> [-c:<character_set>] [-l:<string_length>] [-p:<prefix>]");
            Console.Error.WriteLine("Defaults:[-c:{0}] [-l:{1}]", possibleCharacters, maxGuessLen);
            Console.Error.WriteLine(" Example:\n        ReverseHash.exe 0xa7d91d67 -c:{0} -l:{1} -p:is", possibleCharacters, maxGuessLen);
            Environment.Exit(1);
        }
    }

    public class HashReverse
    {
        private IAppendText appendObj = null;

        public IAppendText AppendObj
        {
            get { return appendObj; }
            set { appendObj = value; }
        }

        void Print(string s)
        {
            if (!s.EndsWith("\n"))
                s += "\n";
            if (appendObj == null)
                Console.Write(s);
            else
                appendObj.AppendText(s);
        }

        uint HashString(char[] input)
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

        public void PrintMatches(
            UInt32 hash,
            string possibleCharacters,
            int targetStringSizeBegin,
            int targetStringSizeEnd,
            string prefix
        )
        {
            DateTime start = DateTime.Now;
            Print(string.Format("Searching matches for 0x{0:x}\n with prefix '{1}'\n using char set '{2}'\n startLen: {3}\n endLen: {4} (range: {5})\n",
                hash, prefix, possibleCharacters, targetStringSizeBegin, targetStringSizeEnd, (targetStringSizeEnd - targetStringSizeBegin)));
            for (int i = targetStringSizeBegin; i < targetStringSizeEnd; i++)
            {
                Print(string.Format("searching string length = {0}; guess_len:{1}", i, i - prefix.Length));
                PrintMatches(hash, possibleCharacters, i, prefix);
            }
            DateTime end = DateTime.Now;
            var timeTaken = end - start;
            Print(string.Format("Finished Searching matches for 0x{0:x}; time: {1}ms ", hash, timeTaken.Milliseconds));
        }

        void PrintMatches(uint hash, string possibleCharacters, int targetStringSize, string prefix)
        {
            char[] bufferToUse = new char[targetStringSize];
            prefix.CopyTo(0, bufferToUse, 0, prefix.Length);
            GenerateStringsAndCheckHash(prefix.Length, targetStringSize, hash, possibleCharacters, bufferToUse);
        }

        void GenerateStringsAndCheckHash(int index, int targetStringSize, uint targetHash, string possibleCharacters, char[] bufferToUse)
        {
            if (index == targetStringSize)
            {
                if (HashString(bufferToUse) == targetHash)
                {
                    Print(string.Format("Match found: '{0}'", new string(bufferToUse)));
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
    }
}