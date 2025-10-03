import ARKit

public typealias notifyUnsupportedDelegate = @convention(c) () -> Void
public typealias broadcastDelegate = @convention(c) (UnsafePointer<UInt8>, Int32) -> Void
public typealias broadcastTransformDelegate = @convention(c) (Float, Float, Float, Float,
                                                        Float, Float, Float, Float,
                                                        Float, Float, Float, Float,
                                                        Float, Float, Float, Float) -> Void

@available(visionOS 26.0, *)
var sharedCoordinateSpaceManager: SharedCoordinateSpaceManager? = nil

@available(visionOS 26.0, *)
@_cdecl("initSharedCoordinateSpaceManager")
public func initSharedCoordinateSpaceManager(
    dlgNotifyUnsupported: @escaping notifyUnsupportedDelegate,
    dlgBroadcast: @escaping broadcastDelegate,
    dlgBroadcastTransform: @escaping broadcastTransformDelegate) {
    sharedCoordinateSpaceManager = SharedCoordinateSpaceManager(dlgNotifyUnsupported, dlgBroadcast, dlgBroadcastTransform)
}

@available(visionOS 26.0, *)
@_cdecl("startVisionCoordinator")
public func startVisionCoordinator() {
    sharedCoordinateSpaceManager!.startVisionCoordinator()
}

@available(visionOS 26.0, *)
@_cdecl("stopVisionCoordinator")
public func stopVisionCoordinator() {
    sharedCoordinateSpaceManager!.stopVisionCoordinator()
}

@available(visionOS 26.0, *)
@_cdecl("onCoordinateData")
public func onCoordinateData(data: UnsafePointer<UInt8>, count: Int){
    let data = Data(bytes: data, count: count);
    sharedCoordinateSpaceManager!.pushCoordinateData(data)
}
