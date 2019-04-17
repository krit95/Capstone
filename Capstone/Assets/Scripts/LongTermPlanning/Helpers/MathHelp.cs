using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathHelp
{


    public static int sigfigify(int n, int sigFigs) { return sigfigify((uint)Mathf.Abs(n), sigFigs); }
    public static int sigfigify(uint n, int sigFigs)
    {
        // NOTE: When casting a negative int into a uint the far left "negative marker" will cout as a sig fig

        int sizeOfN = 0;
        uint nPrime = n;
        while (nPrime > 0)
        {
            nPrime /= 10;
            sizeOfN++;
        }

        if (sigFigs >= sizeOfN) { return (int)n; }

        int digitsCut = sizeOfN - sigFigs;
        uint trimmedN = n / (uint)Mathf.Pow(10, digitsCut); // Cut off the right most values
        return (int)(trimmedN * Mathf.Pow(10, digitsCut)); // Replace them all with 0s 
    }

}
