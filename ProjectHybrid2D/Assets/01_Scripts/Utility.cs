using System.Diagnostics;
using System;
using Unity.Mathematics;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public static class Utility
{
    public static void Testing ()
    {
        //var costumePart = CostumePart.Shark | CostumePart.Bear | CostumePart.Tiger | CostumePart.Panda | CostumePart.Clown;
        ////Remove
        //costumePart &= ~CostumePart.Clown;
        ////Toggle
        //costumePart ^= CostumePart.Shark;
        //Debug.Log((costumePart & CostumePart.Clown) != 0);

        //int test = 156;
        //test = (test << 5) + (test << 1) + test;
        //Debug.Log(test);

        //test = Utility.BitDivide(150, 35);
        //Debug.Log(test);

        //int multiplyTest = Utility.BitMultiply(654, 213);
        //Debug.Log(multiplyTest);

        //math.abs(multiplyTest);

        //int addTest = Utility.BitAdd(115, 5);
        //Debug.Log(addTest);

        //int addTest2 = Utility.BitAddRecurse(115, 5);
        //Debug.Log(addTest2);

        //Debug.Log(Utility.TimeSpanOfAction(() => Utility.BitAdd(600, 600)));
        //Debug.Log(Utility.TimeSpanOfAction(() => Utility.BitAddRecurse(600, 600)));
        //Debug.Log(Utility.TimeSpanOfAction(() => _ = 600 + 600));

        //Debug.Log(Utility.TimeSpanOfAction(() => { _ = (~-500) - 1; }));
        //Debug.Log(Utility.TimeSpanOfAction(() => { _ = 500 * -1; }));
    }

    public static T GetRandomElementFromArray<T> ( T[] array )
    {
        var rand = new Unity.Mathematics.Random(1);
        var str = array[rand.NextInt(array.Length)];
        return str;
    }

    public static T GetRandomElementFromArray<T> ( List<T> array )
    {
        var rand = new Unity.Mathematics.Random(1);
        var str = array[rand.NextInt(array.Count)];
        return str;
    }

    public static T GetRandomElementFromArray<T> ( T[] array, T hasString, int indexer, int maxIndexer ) where T : class
    {
        var rand = new Unity.Mathematics.Random(1);
        var str = array[rand.NextInt(array.Length)];
        if ( hasString != null && str == hasString && array.Length > 1 && indexer <= maxIndexer )
        {
            indexer++;
            return GetRandomElementFromArray(array, hasString, indexer, maxIndexer);
        }
        else if ( indexer > maxIndexer && hasString != null && str == hasString )
        {
            return null;
        }
        return str;
    }

    public static T GetRandomElementFromList<T> ( List<T> list, T[] options, int indexer, int maxIndexer )
    {
        T obj = GetRandomElementFromArray(options);

        if ( list.Contains(obj) && indexer <= maxIndexer )
        {
            indexer++;
            return GetRandomElementFromList(list, options, indexer, maxIndexer);
        }
        else if ( !list.Contains(obj) )
        {
            return obj;
        }
        else
        {
            return default;
        }
    }

    public static TimeSpan TimeSpanOfAction ( Action action )
    {
        Stopwatch sw = Stopwatch.StartNew();
        action?.Invoke();
        return sw.Elapsed;
    }

    public static IEnumerator WaitForSeconds ( float seconds )
    {
        yield return new WaitForSeconds(seconds);
    }

    public static int BitAdd ( int a, int b )
    {
        while ( b != 0 )
        {
            int carry = a & b;
            a ^= b;
            b = carry << 1;
        }
        return a;
    }

    //Often faster than the regular version. Still ever so slightly slower than the + operator though.
    public static int BitAddRecurse ( int a, int b )
    {
        if ( b == 0 )
        {
            return a;
        }
        else
        {
            return BitAddRecurse(a ^ b, (a & b) << 1);
        }
    }

    public static int BitMultiply ( int a, int b )
    {
        int result = 0;
        bool negative = a < 0 ^ b < 0;
        a = math.abs(a);
        b = math.abs(b);
        while ( b > 0 )
        {
            if ( (b & 1) == 1 )
            {
                result += a;
            }
            b >>= 1;
            a <<= 1;
        }
        return negative ? (~result) - 1 : result;
    }

    public static int BitDivide ( int a, int b )
    {
        int result = 0;
        bool negative = a < 0 ^ b < 0;
        a = math.abs(a);
        b = math.abs(b);
        for ( int i = 31; i > -1; i-- )
        {
            int c = b << i;
            if ( b << i <= a && c > 0 )
            {
                a -= (b << i);
                result += 1 << i;
            }
        }
        return negative ? (~result) - 1 : result;
    }
}