using System.Collections.Generic;
using UnityEngine;

public static class ListExtension 
{
    public static List<object> GetLayerID(this List<object> list)
    {
        list.RemoveAll(x => x == null);

        return list;
    }
}