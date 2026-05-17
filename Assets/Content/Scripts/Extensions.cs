using UnityEngine;

public static class Extensions
{
    /// <summary>
    /// returns true if "value" is between "bound1" and "bound2"
    /// </summary>
    public static bool IsInRange(this float value, float bound1, float bound2)
    {
        float minValue = Mathf.Min(bound1, bound2);
        float maxValue = Mathf.Max(bound1, bound2);
        return value > minValue && value < maxValue;
    }

}
