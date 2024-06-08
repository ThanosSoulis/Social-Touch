using System.Collections.Generic;
using UnityEngine;

public static class GraphFilters
{
    public static int SmoothPoints(Vector3[] points, int length, float minDistance)
    {
        float minDistanceRaised = minDistance * minDistance;

        bool nearFound;

        int uniqueCount = 1;

        for (int i = 1; i < length; i++)
        {
            nearFound = false;
            for (int j = 0; j < uniqueCount; j++)
            {
                if (MathT.DistanceSegmentSqrt(points[i], points[j]) < minDistanceRaised)
                {
                    nearFound = true;
                    break;
                }
            }
            if (nearFound == false)
            {
                points[uniqueCount] = points[i];
                uniqueCount++;
            }
        }
        return uniqueCount;
    }

    public static List<Vector3> MovingAverage(List<Vector3> points, int period)
    {
        if (period > points.Count)
        {
            return points;
        }
        Vector3[] buffer = new Vector3[period];
        List<Vector3> output = new List<Vector3>();
        int current_index = 0;
        Vector3 mva = Vector3.zero;
        for (int i = 0; i < points.Count; i++)
        {
            buffer[current_index] = points[i] / period;
            mva = Vector3.zero;
            for (int j = 0; j < period; j++)
            {
                mva += buffer[j];
            }
            if (i < period)
            {
                mva = mva / (i+1) * period;
            }
            output.Add(mva);
            current_index = (current_index + 1) % period;
        }
        return output;
    }

}
