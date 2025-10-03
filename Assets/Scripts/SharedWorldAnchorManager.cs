using System.Collections.Generic;
using UnityEngine;

public class SharedWorldAnchorManager : MonoBehaviour
{
    [SerializeField] private string _broadcastServerURL;
    [SerializeField] private GameObject GizmoPrefab;

    private SharedWorldAnchors _sharedWorldAnchors;
    private DebugLogger _debugLogger;

    private Dictionary<string, Transform> _userPoses = new Dictionary<string, Transform>();

    public void Awake()
    {
        _sharedWorldAnchors = new SharedWorldAnchors();
        _sharedWorldAnchors.Init();
        _sharedWorldAnchors.OnPose.AddListener(OnPose);
    }

    private void OnPose(string id, Pose pose) {
        if (_userPoses.ContainsKey(id))
        {
            _userPoses[id].SetPositionAndRotation(pose.position, pose.rotation);
        }
        else
        {
            var go = Instantiate(GizmoPrefab, pose.position, pose.rotation);
            _userPoses.Add(id, go.transform);
        }
    }

    private void OnEnable()
    {
        _sharedWorldAnchors.Start(_broadcastServerURL);
    }

    private void OnDisable()
    {
        _sharedWorldAnchors.Stop();
        _sharedWorldAnchors.OnPose.RemoveAllListeners();
    }
}