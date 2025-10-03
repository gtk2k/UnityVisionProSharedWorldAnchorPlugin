#if os(visionOS)
import ARKit
import Foundation
import simd
import QuartzCore
import RealityKit
import SwiftUI

@available(visionOS 26.0, *)
class VisionOSSharedSpaceCoordinator {
    private var notifyUnsupported: notifyUnsupportedDelegate?
    private var broadcast: broadcastDelegate?
    private var broadcastTransform: broadcastTransformDelegate?
    private let session = ARKitSession()
    private let worldTracking = WorldTrackingProvider()
    private let sharedProvider = SharedCoordinateSpaceProvider()
    private var coordinateTask: Task<Void, Never>?
    private var eventTask: Task<Void, Never>?
    private var deviceAnchorTask: Task<Void, Never>?
    
    var onError: ((Error) -> Void)?
    
    private(set) var latestDeviceTransform: simd_float4x4 = matrix_identity_float4x4
    
    init(_ dlgNotifyUnsupported: notifyUnsupportedDelegate,
         _ dlgBroadcast: broadcastDelegate,
         _ dlgBroadcastTrasnform: broadcastTransformDelegate){
        notifyUnsupported = dlgNotifyUnsupported
        broadcast = dlgBroadcast
    }
    
    func start() async {
        guard SharedCoordinateSpaceProvider.isSupported, WorldTrackingProvider.isSupported else {
            print("⚠️ Shared coordinate space not supported on this device")
            notifyUnsupported!()
            return
        }
        
        do {
            try await session.run([worldTracking, sharedProvider])
            coordinateTask?.cancel()
            coordinateTask = Task { [weak self] in await self?.pumpCoordinateSpaceData() }
            eventTask?.cancel()
            eventTask = Task { [weak self] in await self?.monitorEvents() }
            deviceAnchorTask?.cancel()
            deviceAnchorTask = Task { [weak self] in await self?.trackDeviceAnchor() }
            print("🌐 Shared coordinate space provider started")
        } catch {
            onError?(error)
            print("❌ Failed to start shared coordinate space provider: \(error)")
        }
    }
    
    func stop() {
        coordinateTask?.cancel()
        eventTask?.cancel()
        deviceAnchorTask?.cancel()
        coordinateTask = nil
        eventTask = nil
        deviceAnchorTask = nil
        session.stop()
        print("🌐 Shared coordinate space provider stopped")
    }
    
    public func pushCoordinateData(_ data: Data) {
        guard let coordinateData = SharedCoordinateSpaceProvider.CoordinateSpaceData(data: data) else {
            print("⚠️ Ignoring invalid coordinate space payload")
            return
        }
        sharedProvider.push(data: coordinateData)
    }
    
    private func pumpCoordinateSpaceData() async {
        while !Task.isCancelled {
            if let data = sharedProvider.nextCoordinateSpaceData {
                data.data.withUnsafeBytes {rawPtr in
                    guard let ptr = rawPtr.baseAddress?.assumingMemoryBound(to: UInt8.self) else { return }
                    broadcast!(ptr, Int32(data.data.count))
                }
            } else {
                try? await Task.sleep(for: .milliseconds(200))
            }
        }
    }
    
    private func monitorEvents() async {
        for await event in sharedProvider.eventUpdates {
            switch event {
            case .sharingEnabled:
                print("=-= sharingEnabled =-=")
            case .sharingDisabled:
                print("=-= sharingDisabled =-=")
            case .connectedParticipantIdentifiers(let participants):
                print("=-= Shared coordinate participants: \(participants) =-=")
            @unknown default:
                print("=-= Shared coordinate unknown event =-=")
            }
        }
    }
    
    private func trackDeviceAnchor() async {
        while !Task.isCancelled {
            if let anchor = worldTracking.queryDeviceAnchor(atTimestamp: CACurrentMediaTime()) {
                latestDeviceTransform = anchor.originFromAnchorTransform
                let c0 = latestDeviceTransform.columns.0
                let c1 = latestDeviceTransform.columns.1
                let c2 = latestDeviceTransform.columns.2
                let c3 = latestDeviceTransform.columns.3
                broadcastTransform!(c0.x, c0.y, c0.z, c0.w,
                                   c1.x, c1.y, c1.z, c1.w,
                                   c2.x, c2.y, c2.z, c2.w,
                                   c3.x, c3.y, c3.z, c3.w)
            }
            try? await Task.sleep(for: .milliseconds(120))
        }
    }
}
#endif
