using System.Collections.Generic;
using UnityEngine;

public static class ListExtension
{
    public static List<object> CleanList(this List<object> list)
    {
        list.RemoveAll(x => x == null);

        return list;
    }
    public static List<GameObject> CleanList(this List<GameObject> list)
    {
        list.RemoveAll(x => x == null);

        return list;
    }
}