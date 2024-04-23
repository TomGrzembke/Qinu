using UnityEngine;

public static class LayerMaskExtensions
{
    public static int GetLayerID(this LayerMask layerMask)
    {
        int amountsDivided = 0;
        int value = layerMask.value;
        while (value != 1 && value != 0)
        {
            value /= 2;
            amountsDivided++;
        }

        return amountsDivided;
    }
}