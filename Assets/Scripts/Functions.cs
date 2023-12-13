using UnityEngine;

public static class Functions
{
    public static float Sigmoid(float x, float center)
    {
        float eToX = Mathf.Exp(x);
        float etoCenter = Mathf.Exp(center);
        return eToX / (etoCenter + eToX);
    }
}