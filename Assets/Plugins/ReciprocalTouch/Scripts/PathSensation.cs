using UnityEngine;
using System;
using Ultraleap.Haptics;
using SVector3 = System.Numerics.Vector3;
using System.Collections.Generic;
using Unity.Netcode;

public class PathSensation : NetworkBehaviour
{
    [Tooltip("Allows the attached GameObject to persist between scenes")]
    public bool IsPersistent;
    
    [Tooltip("Available UltraHaptics device IDs")]
    public string[] UHDeviceIDs = { "USX:000005F5", "USX:00000018" };
    
    [Tooltip("Print the point position with the Manual Alignment applied")]
    public bool DebugOn;

    public float ManualAlignmentX;
    public float ManualAlignmentY;
    public float ManualAlignmentZ;

    [Range(0, 1)]
    public float Intensity = 1f;
    public float DynamicIntensity;

    private bool _newPathAvailable;
    private Vector3[] _incomingPath;
    private int _incomingLength;
    private Vector3[] _path;
    private int[] _incomingEdgeIncrements = new int[0];
    //private int[] _edgeIncrements;
    private int _edgeIncrementSize;
    private float _incomingSeparation;
    private float _separation;

    private Vector3[] _bufferPath = new Vector3[2048];
    private int[] _bufferEdgeIncrements = new int[2048];

    private int _pathIncrement;
    private int _edgeIncrement;
    private int _pointIncrement;
    private int _length;

    private Vector3 position;

    private StreamingEmitter _emitter;
    private Ultraleap.Haptics.Transform _transform;

    float x;
    float y;
    float z;

    private IDevice device;
    SVector3 _p = new SVector3();

    protected override void OnNetworkPostSpawn()
    {
        base.OnNetworkPostSpawn();

        if (!IsOwner)
            return;
        
        if (IsPersistent)
            DontDestroyOnLoad(this.gameObject);
        
        ConnectToEmitter();
    }

    private void ConnectToEmitter()
    {
        DynamicIntensity = Intensity;

        Library lib = new Library();
        lib.Connect();
        _emitter = new StreamingEmitter(lib);
        
        // Find a connected UltraHaptics device
        device = lib.FindDevice();
        if (device == null)
        {
            Debug.LogError("Trying cached UHDeviceIDs");
            foreach (var UHDeviceID in UHDeviceIDs)
                device = lib.FindDevice(UHDeviceID);
        }
        if(device == null)
        {
            Debug.LogError("No UltraHaptics device found");
            return;
        }
        
        //Check if the device is available
        if (device.HasModificationClaim) {
            Debug.LogError("The UltraHaptics device is already claimed");
            return;
            // Debug.Log(_emitter.Devices.Count);
            //Debug.Log(modifiableDevice.HasModificationClaim);
            //_emitter.Devices.AddAndTakeOwnership(ref modifiableDevice);
        }
        
        //Add the device to the StreamingEmitter
        _emitter.Devices.Add(device);
        
        //Cache the tracking-to-haptics transform
        _transform = device.GetKitTransform();

        _emitter.SetControlPointCount(1, AdjustRate.None);
        _emitter.EmissionCallback = Callback;

        _emitter.Start();    
        Debug.Log("UltraHaptics Emitter Start");
    }

    // This callback is called every time the device is ready to accept new control point information
    private void Callback(StreamingEmitter emitter, StreamingEmitter.Interval interval, DateTimeOffset submissionDeadline)
    {
        try
        {
            if (_newPathAvailable == true)
            {
                _length = _incomingLength;

                for (int i = 0; i < _length; i++)
                {
                    _bufferPath[i] = _incomingPath[i];
                    _bufferEdgeIncrements[i] = _incomingEdgeIncrements[i];
                }

                if (_length < 2)
                {
                    return;
                }

                _pointIncrement = 0;
                _edgeIncrement = 0;
                _separation = _incomingSeparation;
                _length = _incomingLength;
                _newPathAvailable = false;

                _edgeIncrementSize = MathT.GetEdgeIncrement(_bufferPath[0], _bufferPath[1], _separation);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

        if (_length < 2)
        {
            return;
        }

        try
        {
            foreach (var sample in interval)
            {
                position = MathT.InterpolatePath(_bufferPath[_pointIncrement], _bufferPath[_pathIncrement + 1], _separation, _edgeIncrement);
                //_p.X = _path[_pointIncrement].x;
                //_p.Y = _path[_pointIncrement].y;
                //_p.Z = _path[_pointIncrement].z;

                _p.X = position.x + ManualAlignmentX;
                _p.Y = position.y + ManualAlignmentY;
                _p.Z = position.z + ManualAlignmentZ;

                sample.Points[0].Position = _p;
                sample.Points[0].Intensity = DynamicIntensity;

                _edgeIncrement++;
                if (_edgeIncrement == _bufferEdgeIncrements[_pointIncrement])
                {
                    _edgeIncrement = 0;

                    _pointIncrement++;
                    if (_pointIncrement == _length - 1)
                    {
                        _pointIncrement = 0;
                    }
                    //_edgeIncrementSize = Mathf.RoundToInt(Vector3.Distance(_bufferPath[_pointIncrement], _bufferPath[_pointIncrement+1]) / _separation);
                    //_edgeIncrementSize = MathT.GetEdgeIncrements(_bufferPath[_pointIncrement], _bufferPath[_pointIncrement+1], _separation);
                    //Debug.Log(_edgeIncrementSize);
                }

            }
            if (DebugOn)
            {
                Debug.Log("Post: " + _p.X + ", " + _p.Y + ", " + _p.Z);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

    }

    internal void SetEmptyPath()
    {
        _newPathAvailable = true;
        _incomingPath = new Vector3[0];
        _incomingLength = 0;
        //_incomingPath = new List<Vector3>();
        DynamicIntensity = 0f;
    }

    void OnDisable()
    {
        if (_emitter != null)
        {
            _emitter.Stop();
        }
    }

    void OnDestroy()
    {
        if (_emitter != null)
        {
            _emitter.Stop();
        }

    }

    public void SetPath(Vector3[] path, int length, int[] edgeIncrements, float separation, float intensity)
    {
        DynamicIntensity = intensity;
        _newPathAvailable = true;
        _incomingPath = path;
        _incomingEdgeIncrements = edgeIncrements;
        _incomingLength = length;
        _incomingSeparation = separation;
    }
}