//
//  OTVideoCaptureOSXDefault.h
//  otkit-objc-libs
//
//  Created by Charley Robinson on 11/15/13.
//
//

#import <Foundation/Foundation.h>
#import <OpenTok-OSX/OpenTok.h>

@interface OTVideoCaptureOSXDebugger :
NSObject <AVCaptureVideoDataOutputSampleBufferDelegate, OTVideoCapture>

@property (nonatomic, retain) AVCaptureSession *captureSession;
@property (nonatomic, retain) AVCaptureVideoDataOutput *videoOutput;
@property (nonatomic, retain) AVCaptureDeviceInput *videoInput;
@property (nonatomic, assign) NSString* captureSessionPreset;
@property (nonatomic, assign) OTVideoOrientation currentDeviceOrientation;
@property (nonatomic, assign) AVFrameRateRange* frameRateRange;

@property (readonly) NSArray* availableSessionPresets;
@property (readonly) NSArray* availableDeviceOrientations;

- (id)init;
- (void)initCapture;
- (void)releaseCapture;
- (int32_t)startCapture;
- (int32_t)stopCapture;
- (BOOL)isCaptureStarted;
- (int32_t)captureSettings:(OTVideoFormat*)videoFormat;

@end
