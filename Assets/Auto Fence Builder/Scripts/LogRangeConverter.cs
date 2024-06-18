using UnityEngine;

public struct LogRangeConverter
{
    public readonly float minValue;
    public readonly float maxValue;

    private readonly float a;
    private readonly float b;
    private readonly float c;

    public LogRangeConverter(float minValue, float centerValue, float maxValue)
    {
        this.minValue = minValue;
        this.maxValue = maxValue;

        a = (minValue * maxValue - (centerValue * centerValue)) / (minValue - 2 * centerValue + maxValue);
        b = ((centerValue - minValue) * (centerValue - minValue)) / (minValue - 2 * centerValue + maxValue);
        c = 2 * Mathf.Log((maxValue - centerValue) / (centerValue - minValue));
    }

    // Converts the value in range 0 - 1 to the value in range of minValue - maxValue
    public float ToRange(float value01)
    {
        float x = a + b * Mathf.Exp(c * value01);
        if (x == 2)
            x = 2.1f;
        //Debug.Log(x+"\n");
        return x;
    }

    //Converts the value in range min-max to a value between 0 and 1 that can be used for a slider
    public float ToNormalized(float rangeValue)
    {
        float x = Mathf.Log((rangeValue - a) / b) / c;
        if (x == 2)
            x = 2.1f;
        return x;
    }
}