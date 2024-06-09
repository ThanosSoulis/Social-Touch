using System;
using System.Collections.Generic;
using UnityEngine;

public static class MathT
{
    public static int[] GetEdgeIncrements(int[] edgeIncrements, Vector3[] bufferPoints, int bufferPointsCount, float interpolationSeparation)
    {
        float edgeDistance;
        float separationMult = 1f / interpolationSeparation;

        for (int i = 0; i < (bufferPointsCount - 1); i++)
        {
            edgeDistance = Vector3.Distance(bufferPoints[i], bufferPoints[i+1]);

            edgeIncrements[i] = Mathf.RoundToInt(edgeDistance * separationMult);
        }

        return edgeIncrements;
    }

    public static int GetEdgeIncrement(Vector3 start, Vector3 target, float interpolationSeparation)
    {
        return Mathf.RoundToInt(Vector3.Distance(start, target) / interpolationSeparation);
    }

    public static Vector3 InterpolatePath(Vector3 start, Vector3 target, float separation, int edgeIncrement)
    {
        Vector3 normal = target - start;
        float edgeDistance = Vector3.Distance(start, target);

        return start + ((separation * edgeIncrement) / edgeDistance) * normal;
    }

    public static float GetInterpolationSeparation(float distance, InterpolationClass interpolationClass, int uHSampleRate)
    {
        float minimumDistance = interpolationClass.DefaultInterpolationSeparation * uHSampleRate / interpolationClass.MaximumFrequency;
        float maximumDistance = interpolationClass.DefaultInterpolationSeparation * uHSampleRate / interpolationClass.MinimumFrequency;

        float separation;

        if (distance == 0)
        {
            return 0f;
        }

        if (interpolationClass.DynamicCapping == true)
        {
            if (distance < minimumDistance)
            {
                separation = interpolationClass.MaximumFrequency * distance / uHSampleRate;
            }
            else if (distance > maximumDistance)
            {
                separation = interpolationClass.MinimumFrequency * distance / uHSampleRate;
            }
            else
            {
                separation = interpolationClass.DefaultInterpolationSeparation;
            }
        }
        else
        {
            separation = interpolationClass.DefaultInterpolationSeparation;
        }
        return separation;
    }

    public static float DistanceSegmentSqrt(Vector3 start, Vector3 end)
    {
        return (end.x - start.x) * (end.x - start.x)
                + (end.y - start.y) * (end.y - start.y)
                + (end.z - start.z) * (end.z - start.z);
    }

    public static float TwoOpt(Vector3[] points, int length, int max2OptIterations)
    {
        float lengthDelta;
        int n = length - 1;

        int deltaCount = 0;

        bool foundImprovement = true;
        int improvementCount = 0;
        while (foundImprovement == true)
        {
            foundImprovement = false;
            improvementCount++;
            for (int v1 = 0; v1 < (n - 1); v1++)
            {
                for (int v2 = v1 + 1; v2 < n; v2++)
                {

                    lengthDelta = -DistanceSegmentSqrt(points, v1, v1 + 1) - DistanceSegmentSqrt(points, v2, v2 + 1)
                                  + DistanceSegmentSqrt(points, v1 + 1, v2 + 1) + DistanceSegmentSqrt(points, v1, v2);
                    deltaCount++;

                    if (lengthDelta < 0)
                    {
                        TwoOptSwapFast(points, v1, v2);
                        foundImprovement = true;
                    }
                }
            }
            if (improvementCount >= max2OptIterations)
            {
                break;
            }
        }
        return CalculateTotalDistance(points, length);
    }

    public static float DistanceSegmentSqrt(Vector3[] p, int start, int end)
    {
        return (p[end].x - p[start].x) * (p[end].x - p[start].x)
                + (p[end].y - p[start].y) * (p[end].y - p[start].y)
                + (p[end].z - p[start].z) * (p[end].z - p[start].z);
    }

    private static void TwoOptSwapFast(Vector3[] points, int v1, int v2)
    {
        int vToSwap = v1 + 1;
        for (int i = v2; i >= (v1 + 1); i--)
        {
            if (i <= vToSwap)
            {
                break;
            }
            Swap(points, i, vToSwap);
            vToSwap++;
        }
    }

    public static void Swap(Vector3[] array, int indexA, int indexB)
    {
        Vector3 tmp = array[indexA];
        array[indexA] = array[indexB];
        array[indexB] = tmp;
    }

    public static float CalculateTotalDistance(Vector3[] points, int length)
    {
        float distance = 0;

        for (int i = 0; i < length - 1; i++)
        {
            distance += Vector3.Distance(points[i], points[i + 1]);
        }
        return distance;
    }


}