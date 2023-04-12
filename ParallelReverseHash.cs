using ReverseHash;
using System;
using System.Threading.Tasks;

public class ParallelReverseHash
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
    public UInt32 HashString(char[] input)
    {
        UInt32 FNV_prime = 16777619;
        UInt32 offset_basis = 2166136261;
        UInt32 hash = offset_basis;
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
        Print(string.Format("Finished Searching matches for 0x{0:x}; time: {1}ms ", hash, timeTaken.TotalMilliseconds));
    }
    public void PrintMatches(UInt32 hash, string possibleCharacters, int targetStringSize, string prefix)
    {
        char[] bufferToUse = new char[targetStringSize];
        Array.Copy(prefix.ToCharArray(), bufferToUse, prefix.Length);

        Parallel.ForEach(possibleCharacters, (c) =>
        {
            char[] parallelBuffer = (char[])bufferToUse.Clone();
            parallelBuffer[prefix.Length] = c;
            GenerateStringsAndCheckHash(prefix.Length + 1, targetStringSize, hash, possibleCharacters, parallelBuffer);
        });
    }

    private void GenerateStringsAndCheckHash(int index, int targetStringSize, UInt32 targetHash, string possibleCharacters, char[] bufferToUse)
    {
        if (index == targetStringSize)
        {
            if (HashString(bufferToUse) == targetHash)
            {
                Print("Match found: " + new string(bufferToUse));
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
