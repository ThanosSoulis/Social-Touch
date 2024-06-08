using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(PathSensation))]
public class MammothRenderer : MonoBehaviour
{
    private PathSensation _pathSensation;
    //public UpdateMeshCollider UpdateMeshCollider;

    public Transform ServiceProvider;
    private Vector3 UltraLeapAlignment = new Vector3(0f, 0.1210f, 0f);

    private float MinSmoothingSeparation = 0.008f;

    public bool DisableHaptics;
    //public bool VisualiseRaw;
    //public bool VisualiseExclusion;
    public bool VisualiseInterpolation;
    //public bool VisualiseTwoOpt;
    //public bool VisualiseInitial;
    //public bool VisualiseBarreiro;
    
    [Header("Interpolation")]
    public float DefaultInterpolationSeparation = 0.00035f;
    public float MinimumFrequency = 60f;
    public float MaximumFrequency = 200f;
    public bool DynamicCapping = true;

    private InterpolationClass _interpolationClass = new InterpolationClass();

    public float UpdateRate = 0.1f;
    private float _t;

    private List<Transform> _contacts = new List<Transform>();
    private List<float> _contactIntensities = new List<float>();

    private Vector3[] _bufferPoints;
    private int[] _bufferIds;
    private int _bufferPointsCount;
    public int PointBufferSize = 2048;
    private int[] _bufferEdgeIncrements;

    // For DrawGizmos
    private List<Vector3> preTransformPoints = new List<Vector3>();
    private List<Vector3> _rawPoints = new List<Vector3>();
    private List<Vector3> smoothPoints = new List<Vector3>();
    private Vector3[] twoOptPoints = new Vector3[2048];
    private List<Vector3> barreiroPoints = new List<Vector3>();
    private float _visualizationCount;


    private void OnValidate()
    {
        _interpolationClass.DefaultInterpolationSeparation = DefaultInterpolationSeparation;
        _interpolationClass.MinimumFrequency = MinimumFrequency;
        _interpolationClass.MaximumFrequency = MaximumFrequency;
        _interpolationClass.DynamicCapping = DynamicCapping;
    }

    private void Awake()
    {
        _bufferPoints = new Vector3[PointBufferSize];
        _bufferIds = new int[PointBufferSize];
        _bufferEdgeIncrements = new int[PointBufferSize];

        _pathSensation = GetComponent<PathSensation>();
    }
    void FixedUpdate()
    {
        _t += Time.deltaTime;

        if (_t > UpdateRate)
        {
            GetPointSnapshot();

            /*if (UpdateMeshCollider.isActiveAndEnabled)
            {
                UpdateMeshCollider.UpdateNow();
            }
            //_contacts.Clear();*/

            if (_bufferPointsCount > 0)
            {
                ShortestPath();
            }
            else
            {
                _pathSensation.SetEmptyPath();
            }
            _t -= UpdateRate;
        }
    }

    float time;

    private void ShortestPath()
    {
        // Smoothing: remove points within MinSmoothingSeparation from each other
        _bufferPointsCount = GraphFilters.SmoothPoints(_bufferPoints, _bufferPointsCount, MinSmoothingSeparation);

        // 2-Opt algorithm
        float distance = MathT.TwoOpt(_bufferPoints, _bufferPointsCount, 10);

        //twoOptPoints = new Vector3[_bufferPointsCount];
        if (VisualiseInterpolation == true)
        {
            Array.Copy(_bufferPoints, twoOptPoints, _bufferPointsCount);
            _visualizationCount = _bufferPointsCount;
        }

        // Transform from LeapMotion coordinate space to UltraHaptics space, accounting for position in Unity
        TransformContacts(_bufferPoints, _bufferPointsCount);

        // Get the dynamic interpolation separation
        float interpolationSeparation = MathT.GetInterpolationSeparation(distance, _interpolationClass, 40000);

        //Debug.Log("Frequency: " + (40000f / (distance/interpolationSeparation)));

        // Get the number ofinterpolated points to use per edge
        _bufferEdgeIncrements = MathT.GetEdgeIncrements(_bufferEdgeIncrements, _bufferPoints, _bufferPointsCount, interpolationSeparation);

        float intensity = _contactIntensities.Average();

        // Send to UltraHaptics
        if (DisableHaptics == false)
        {
            _pathSensation.SetPath(_bufferPoints, _bufferPointsCount, _bufferEdgeIncrements, interpolationSeparation, intensity);
        }
    }

    
    private void TransformContacts(Vector3[] contacts, int length)
    {
        for (int i = 0; i < length; i++)
        {
            OffsetPosition(contacts, i);
            TransformPosition(contacts, i);
        }
    }

    private void OffsetPosition(Vector3[] contacts, int i)
    {
        contacts[i] -= ServiceProvider.position;
    }

    private void TransformPosition(Vector3[] contacts, int i)
    {
        contacts[i] = new Vector3(contacts[i].x, -(-contacts[i].z - UltraLeapAlignment.y), contacts[i].y);
    }
    private void GetPointSnapshot()
    {
        _bufferPointsCount = _contacts.Count;
        List<Transform> activeContacts = _contacts;

        for (int i = 0; i < _bufferPointsCount; i++)
        {
            try
            {
                _bufferPoints[i] = activeContacts[i].position;
            }
            catch
            {
                Debug.Log(i);
            }
        }
    }
    public void AddContactPoint(Transform childTransform, float intensity)
    {
        _contacts.Add(childTransform);
        _contactIntensities.Add(intensity);
    }

    public void RemoveContactPoint(Transform childTransform)
    {
        int idx = _contacts.IndexOf(childTransform);
        _contactIntensities.RemoveAt(idx);
        _contacts.RemoveAt(idx);

    }

    public void Disconnect()
    {
        _contacts.Clear();
    }

    void OnDrawGizmosSelected()
    {
        if (VisualiseInterpolation == true)
        {
            /*Gizmos.color = Color.red;

            for (int i = 0; i < preTransformPoints.Count-1; i++)
            {
                Gizmos.DrawSphere(preTransformPoints[i], 0.001f);
            }*/
            Gizmos.color = Color.blue;

            for (int i = 0; i < _visualizationCount - 1; i++)
            {
                Gizmos.DrawLine(twoOptPoints[i], twoOptPoints[i + 1]);
            }
        }

        /*if (VisualiseRaw == true)
        {
            Gizmos.color = Color.green;

            for (int i = 0; i < _rawPoints.Count; i++)
            {
                Gizmos.DrawSphere(_rawPoints[i], 0.001f);
            }
        }

        if (VisualiseExclusion == true)
        {
            Gizmos.color = Color.green;

            for (int i = 0; i < smoothPoints.Count; i++)
            {
                Gizmos.DrawSphere(smoothPoints[i], 0.001f);
            }
        }

        if (VisualiseTwoOpt == true)
        {
            Gizmos.color = Color.blue;

            for (int i = 0; i < twoOptPoints.Length - 1; i++)
            {
                Gizmos.DrawLine(twoOptPoints[i], twoOptPoints[i + 1]);
            }
        }        
        
        if (VisualiseInitial == true)
        {
            Gizmos.color = Color.blue;

            for (int i = 0; i < smoothPoints.Count - 1; i++)
            {
                Gizmos.DrawLine(smoothPoints[i], smoothPoints[i + 1]);
            }
        }

        if (VisualiseBarreiro == true)
        {
            Gizmos.color = Color.red;

            for (int i = 0; i < barreiroPoints.Count - 1; i++)
            {
                Gizmos.DrawSphere(barreiroPoints[i], 0.001f);
            }
            Gizmos.color = Color.blue;

            for (int i = 0; i < twoOptPoints.Length - 1; i++)
            {
                Gizmos.DrawLine(twoOptPoints[i], twoOptPoints[i + 1]);
            }
        }*/
    }
}
