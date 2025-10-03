import simd

@available(visionOS 26.0, *)
public class SharedCoordinateSpaceManager{
    private var notifyUnsupported: notifyUnsupportedDelegate?
    private var broadcast: broadcastDelegate?
    private var broadcastTransform: broadcastTransformDelegate?
    private var visionCoordinator: VisionOSSharedSpaceCoordinator?
    
    init(
        _ dlgNotifyUnsupported: notifyUnsupportedDelegate,
        _ dlgBroadcast: broadcastDelegate,
        _ dlgBroadcastTransform: broadcastTransformDelegate){
        notifyUnsupported = dlgNotifyUnsupported
        broadcast = dlgBroadcast
    }
    
    public func startVisionCoordinator() {
        visionCoordinator = VisionOSSharedSpaceCoordinator(notifyUnsupported!, broadcast!, broadcastTransform!)
        visionCoordinator!.onError = { (error: Error) in
            print("❌ Shared coordinate space error: \(error)")
        }
        Task{
            await visionCoordinator!.start()
        }
    }
    
    @available(visionOS 26.0, *)
    public func stopVisionCoordinator(){
        visionCoordinator?.stop()
    }
    
    @available(visionOS 26.0, *)
    public func pushCoordinateData(_ message: Data){
        visionCoordinator!.pushCoordinateData(message)
    }
}
