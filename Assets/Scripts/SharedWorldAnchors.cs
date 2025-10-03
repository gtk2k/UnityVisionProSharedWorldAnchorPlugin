using System;
using System.Runtime.InteropServices;
using System.Threading;
using AOT;
using UnityEngine;
using UnityEngine.Events;

public class SharedWorldAnchors
{
    [DllImport("__Internal")]
    private static extern void initSharedCoordinateSpaceManager(
        Action<Int32> NotifyUnsupported,
        Action<IntPtr, Int32> Broadcast,
        Action<float, float, float, float,
                float, float, float, float,
                float, float, float, float,
                float, float, float, float> BroadcastTransform);

    [DllImport("__Internal")]
    private static extern void startVisionCoordinator();

    [DllImport("__Internal")]
    private static extern void stopVisionCoordinator();

    [DllImport("__Internal")]
    private static extern void onCoordinateData(byte[] data, Int32 length);

    [MonoPInvokeCallback(typeof(Action<Int32>))]
    public static void NotifyUnsupported(Int32 hoge)
    {
        Debug.Log($"UnSupported");
    }

    [MonoPInvokeCallback(typeof(Action<IntPtr, Int32>))]
    public static void Broadcast(IntPtr dataPtr, Int32 count)
    {
        var data = new byte[count];
        Marshal.Copy(dataPtr, data, 0, count);
        _bc?.Send(data);
    }

    [MonoPInvokeCallback(typeof(Action<float, float, float, float,
                                        float, float, float, float,
                                        float, float, float, float,
                                        float, float, float, float>))]
    public static void BroadcastTransform(float val00, float val01, float val02, float val03,
                                        float val10, float val11, float val12, float val13,
                                        float val20, float val21, float val22, float val23,
                                        float val30, float val31, float val32, float val33)
    {
        var floatArray = new float[] {val00, val01, val02, val03,
                                    val10, val11, val12, val13,
                                    val20, val21, val22, val23,
                                    val30, val31, val32, val33};
        var data = new byte[16 * 4];
        Buffer.BlockCopy(floatArray, 0, data, 0, floatArray.Length);
        _bc?.Send(data);
    }

    public UnityEvent<string, Pose> OnPose;

    private static BroadcastClient _bc = null;
    private SynchronizationContext _ctx;

    public void Init()
    {
        _ctx = SynchronizationContext.Current;
        initSharedCoordinateSpaceManager(NotifyUnsupported, Broadcast, BroadcastTransform);
    }

    public void Start(string url)
    {
        _bc = new BroadcastClient(_ctx);
        _bc.OnMessage += OnMessage;
        _bc.Connect(url);
        startVisionCoordinator();
    }

    public void Stop()
    {
        stopVisionCoordinator();
        _bc?.Close();
        _bc = null;
    }

    private void OnMessage(string id, byte[] data)
    {
        onCoordinateData(data, data.Length);
        OnPose?.Invoke(id, ConvertToPose(data));
    }

    private Pose ConvertToPose(byte[] src)
    {
        var values = new float[16];
        Buffer.BlockCopy(src, 0, values, 0, src.Length);
        var matrix = new Matrix4x4(
            new Vector4(values[0], values[1], values[2], values[3]),
            new Vector4(values[4], values[5], values[6], values[7]),
            new Vector4(values[8], values[9], values[10], values[11]),
            new Vector4(values[12], values[13], values[14], values[15]));
        var position = matrix.GetPosition();
        position.z *= -1;
        var rotation = matrix.rotation;
        rotation.z *= -1;
        rotation.w *= -1;
        return new Pose(position, rotation);
    }
}

