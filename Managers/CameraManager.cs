using System;
using JangLib;
using Unity.Cinemachine;
using UnityEngine;

public enum CameraType
{
    TopDown,
    Spike
}

public class CameraManager : SingletonWithScene<CameraManager>, IBaseManager
{
    public bool IsInitialized { get; private set; }

    [SerializeField] private CinemachineCamera _topDownCam;
    [SerializeField] private CinemachineCamera _spikeCam;
    
    public CinemachineCamera SpikeCam => _spikeCam;
    
    public void Init()
    {
        ChangeCamByType(CameraType.TopDown);

        IsInitialized = true;
    }

    public void ChangeCamByType(CameraType type, Action onComplete = null)
    {
        switch (type)
        {
            case CameraType.TopDown:
                ChangeCamToTopdown();
                break;

            case CameraType.Spike:
                ChangeCamToSpike();
                break;
        }

        onComplete?.Invoke();
    }

    [ContextMenu("Change To Topdown")]
    private void ChangeCamToTopdown()
    {
        _topDownCam.Priority = 20;
        _spikeCam.Priority = 10;
    }

    [ContextMenu("Change To Spike")]
    private void ChangeCamToSpike()
    {
        _topDownCam.Priority = 10;
        _spikeCam.Priority = 20;
    }
}
