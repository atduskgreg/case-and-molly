//
//  OTVideoCaptureOSXDefault.m
//  otkit-objc-libs
//
//  Created by Charley Robinson on 11/15/13.
//
//

#import "OTVideoCaptureOSXDebugger.h"

@implementation OTVideoCaptureOSXDebugger
{
    id<OTVideoCaptureConsumer> _videoCaptureConsumer;
    OTVideoFrame* _videoFrame;
    
    uint32_t _captureWidth;
    uint32_t _captureHeight;
    NSString* _capturePreset;
    
    AVCaptureSession *_session;
    AVCaptureDeviceInput *_videoInput;
    AVCaptureVideoDataOutput *_videoOutput;
    dispatch_queue_t _capture_queue;
	BOOL capturing;
    
}

@synthesize captureSession = _session;
@synthesize videoInput = _videoInput, videoOutput = _videoOutput;
@synthesize videoCaptureConsumer = _videoCaptureConsumer;
@synthesize currentDeviceOrientation;

-(id)init {
    self = [super init];
    if (self) {
        //        _captureWidth = 192;
        //        _captureHeight = 144;
        //        _capturePreset = AVCaptureSessionPresetLow;
        //        _captureWidth = 352;
        //        _captureHeight = 288;
        //        _capturePreset = AVCaptureSessionPreset352x288;
        _captureWidth = 640;
        _captureHeight = 480;
        _capturePreset = AVCaptureSessionPreset640x480;
        currentDeviceOrientation = OTVideoOrientationUp;
        //        _captureWidth = 1280;
        //        _captureHeight = 720;
        //        _capturePreset = AVCaptureSessionPreset1280x720;
        _capture_queue = dispatch_queue_create("com.tokbox.OTVideoCapture", 0);
        _videoFrame = [[OTVideoFrame alloc] initWithFormat:
                       [OTVideoFormat videoFormatNV12WithWidth:_captureWidth
                                                        height:_captureHeight]];
    }
    return self;
}

- (void) initCapture {
    //-- Setup Capture Session.
    
	_session = [[AVCaptureSession alloc] init];
    [_session beginConfiguration];
    [_session setSessionPreset:_capturePreset];
    
    //-- Creata a video device and input from that Device.
    //Add the input to the capture session.
    AVCaptureDevice * videoDevice = [self frontFacingCamera];
    if(videoDevice == nil)
        assert(0);
    
    NSError* error;
    
    //-- Add the device to the session.
    _videoInput = [AVCaptureDeviceInput deviceInputWithDevice:videoDevice
                                                        error:&error];
    
    if(error)
        assert(0); //TODO: Handle error
    
    [_session addInput:_videoInput];
    
    //-- Create the output for the capture session.
    _videoOutput = [[AVCaptureVideoDataOutput alloc] init];
    [_videoOutput setAlwaysDiscardsLateVideoFrames:YES];
    
    // OSX: Video settings may be overridden by the sample buffer output,
    // So we'll use the defaults from the capture session preset (assumed to
    // always be incorrect), then override with a call to setSessionPreset on
    // this class, which will also ensure correct video settings.
    [_videoOutput setVideoSettings:nil];
    
    [_videoOutput setSampleBufferDelegate:self queue:_capture_queue];
    
    AVFrameRateRange* range =
    [_videoInput.device.activeFormat.videoSupportedFrameRateRanges
     objectAtIndex:0];
    if ([_videoInput.device lockForConfiguration:&error]) {
        [_videoInput.device setActiveVideoMinFrameDuration:range.minFrameDuration];
        //[_videoInput.device setActiveVideoMaxFrameDuration:range.maxFrameDuration];
        [_videoInput.device unlockForConfiguration];
    }
    
    [_session addOutput:_videoOutput];
	[_session commitConfiguration];
    
    [self setCaptureSessionPreset:_capturePreset];
    
    [_session startRunning];
}

-(void)releaseCapture {
    [_session stopRunning];
    [_session release];
    _videoInput = nil;
    [_videoOutput release];
}

- (int32_t)captureSettings:(OTVideoFormat*)videoFormat {
    videoFormat.pixelFormat = OTPixelFormatNV12;
    videoFormat.imageWidth = _captureWidth;
    videoFormat.imageHeight = _captureHeight;
    return 0;
}

- (void)dealloc {
	[self stopCapture];
    [self releaseCapture];
    if (_capture_queue) {
        dispatch_release(_capture_queue);
        _capture_queue = nil;
    }
    [_videoFrame release];
    [super dealloc];
}

- (AVCaptureDevice *) cameraWithPosition:(AVCaptureDevicePosition) position {
    NSArray *devices = [AVCaptureDevice devicesWithMediaType:AVMediaTypeVideo];
    for (AVCaptureDevice *device in devices) {
        if ([device position] == position) {
            return device;
        }
    }
    // If none found, just use the first thing you see
    if ([devices count] > 0) {
        return [devices objectAtIndex:0];
    } else {
        return nil;
    }
}

- (AVCaptureDevice *) frontFacingCamera {
    return [self cameraWithPosition:AVCaptureDevicePositionFront];
}

- (AVCaptureDevice *) backFacingCamera {
    return [self cameraWithPosition:AVCaptureDevicePositionBack];
}

- (BOOL) hasMultipleCameras {
    return [[AVCaptureDevice devicesWithMediaType:AVMediaTypeVideo] count] > 1;
}

- (BOOL) hasTorch {
    return [[[self videoInput] device] hasTorch];
}

- (AVCaptureTorchMode) torchMode {
    return [[[self videoInput] device] torchMode];
}

- (void) setTorchMode:(AVCaptureTorchMode) torchMode {
    
    AVCaptureDevice *device = [[self videoInput] device];
    if ([device isTorchModeSupported:torchMode] && [device torchMode] != torchMode) {
        NSError *error;
        if ([device lockForConfiguration:&error]) {
            [device setTorchMode:torchMode];
            [device unlockForConfiguration];
        } else {
            //Handle Error
        }
    }
}

- (BOOL)isFrameRateSupported:(double)frameRate {
    NSArray* ranges =
    _videoInput.device.activeFormat.videoSupportedFrameRateRanges;
    
    for (AVFrameRateRange* range in ranges) {
        if (range.maxFrameRate >= frameRate &&
            range.minFrameRate <= frameRate)
        {
            return true;
        }
    }
    
    return false;
}

+ (void)dimensionsForCapturePreset:(NSString*)preset
                             width:(uint32_t*)width
                            height:(uint32_t*)height
{
    if ([preset isEqualToString:AVCaptureSessionPreset320x240]) {
        *width = 320;
        *height = 240;
    } else if ([preset isEqualToString:AVCaptureSessionPreset352x288]) {
        *width = 352;
        *height = 288;
    } else if ([preset isEqualToString:AVCaptureSessionPreset640x480]) {
        *width = 640;
        *height = 480;
    } else if ([preset isEqualToString:AVCaptureSessionPreset960x540]) {
        *width = 960;
        *height = 540;
    } else if ([preset isEqualToString:AVCaptureSessionPreset1280x720]) {
        *width = 1280;
        *height = 720;
    }
}

+ (NSSet *)keyPathsForValuesAffectingAvailableSessionPresets
{
	return [NSSet setWithObjects:@"captureSession", nil];
}

- (NSArray *)availableSessionPresets
{
	NSArray *allSessionPresets = [NSArray arrayWithObjects:
								  AVCaptureSessionPreset320x240,
								  AVCaptureSessionPreset352x288,
								  AVCaptureSessionPreset640x480,
								  AVCaptureSessionPreset960x540,
								  AVCaptureSessionPreset1280x720,
								  nil];
	
	NSMutableArray *availableSessionPresets = [NSMutableArray arrayWithCapacity:9];
	for (NSString *sessionPreset in allSessionPresets) {
		if ([[self captureSession] canSetSessionPreset:sessionPreset])
			[availableSessionPresets addObject:sessionPreset];
	}
	
	return availableSessionPresets;
}

- (NSArray*)availableDeviceOrientations {
    return [NSArray arrayWithObjects:
            [NSNumber numberWithInt:OTVideoOrientationUp],
            [NSNumber numberWithInt:OTVideoOrientationDown],
            [NSNumber numberWithInt:OTVideoOrientationLeft],
            [NSNumber numberWithInt:OTVideoOrientationRight],
            nil];
}

- (NSString*)captureSessionPreset {
    return [[self captureSession] sessionPreset];
}

- (void) setCaptureSessionPreset:(NSString*)preset {
    AVCaptureSession *session = [self captureSession];
    
    [OTVideoCaptureOSXDebugger dimensionsForCapturePreset:preset
                                                    width:&_captureWidth
                                                   height:&_captureHeight];
    if ([session canSetSessionPreset:preset] && ![preset isEqualToString:session.sessionPreset]) {
        
        dispatch_sync(_capture_queue, ^{
            [_session beginConfiguration];
            [_session setSessionPreset:preset];
            _capturePreset = preset;
            [_videoFrame setFormat:[OTVideoFormat
                                    videoFormatNV12WithWidth:_captureWidth
                                    height:_captureHeight]];
            
            [_videoOutput setVideoSettings:
             [NSDictionary dictionaryWithObjectsAndKeys:
              [NSNumber numberWithInt:kCVPixelFormatType_420YpCbCr8BiPlanarVideoRange],
              kCVPixelBufferPixelFormatTypeKey,
              [NSNumber numberWithInt:_captureHeight],
              kCVPixelBufferHeightKey,
              [NSNumber numberWithInt:_captureWidth],
              kCVPixelBufferWidthKey,
              nil]];
            
            // For extra credit: go find the best device configuration for this
            // preset. It's not clear whether AVFoundation does this under the
            // hood, but there may be a slight performance gain.
            //[videoDevice lockForConfiguration:&error];
            //AVCaptureDeviceFormat* videoFormat =
            //[self findNearest420vFormat:videoDevice];
            //[videoDevice setActiveFormat:videoFormat];
            //[videoDevice unlockForConfiguration];
            
            [_session commitConfiguration];
        });
    }
}

- (AVCaptureDeviceFormat*)findNearest420vFormat:(AVCaptureDevice*)videoDevice
{
    NSArray* formats = videoDevice.formats;
    for (AVCaptureDeviceFormat* format in formats) {
        FourCharCode pixelFormat =
        CMFormatDescriptionGetMediaSubType(format.formatDescription);
        if ('420v' == pixelFormat) {
            return format;
        }
    }
    return nil;
}

- (AVFrameRateRange *)frameRateRange
{
	AVFrameRateRange *activeFrameRateRange = nil;
	for (AVFrameRateRange *frameRateRange in _videoInput.device.activeFormat.videoSupportedFrameRateRanges)
	{
		if (CMTIME_COMPARE_INLINE(frameRateRange.minFrameDuration, ==, _videoInput.device.activeVideoMinFrameDuration))
		{
			activeFrameRateRange = frameRateRange;
			break;
		}
	}
	
	return activeFrameRateRange;
}

- (void)setFrameRateRange:(AVFrameRateRange *)frameRateRange
{
	NSError *error = nil;
	if ([_videoInput.device.activeFormat.videoSupportedFrameRateRanges containsObject:frameRateRange])
	{
		if ([_videoInput.device lockForConfiguration:&error]) {
			[_videoInput.device setActiveVideoMinFrameDuration:frameRateRange.minFrameDuration];
			//[_videoInput.device setActiveVideoMaxFrameDuration:frameRateRange.maxFrameDuration];
			[_videoInput.device unlockForConfiguration];
		}
	}
}

+ (NSSet *)keyPathsForValuesAffectingFrameRateRange
{
	return [NSSet setWithObjects:@"videoInput.device.activeFormat.videoSupportedFrameRateRanges", @"videoInput.device.activeVideoMinFrameDuration", nil];
}

- (BOOL) isCaptureStarted {
    return capturing;
}

- (int32_t) startCapture {
	capturing = YES;
    return 0;
}

- (int32_t) stopCapture {
	capturing = NO;
    return 0;
}

- (void)captureOutput:(AVCaptureOutput *)captureOutput
didOutputSampleBuffer:(CMSampleBufferRef)sampleBuffer
       fromConnection:(AVCaptureConnection *)connection {
    
    if (capturing && _videoCaptureConsumer) {
		
		CMTime time = CMSampleBufferGetPresentationTimeStamp(sampleBuffer);
		CVImageBufferRef imageBuffer =
        CMSampleBufferGetImageBuffer(sampleBuffer);
		CVPixelBufferLockBaseAddress(imageBuffer, 0);
		
        _videoFrame.timestamp = time;
        size_t height = CVPixelBufferGetHeight(imageBuffer);
        size_t width = CVPixelBufferGetWidth(imageBuffer);
		_videoFrame.format.imageWidth = width;
		_videoFrame.format.imageHeight = height;
        CMTime minFrameDuration =
        _videoInput.device.activeVideoMinFrameDuration;
        _videoFrame.format.estimatedFramesPerSecond =
        minFrameDuration.timescale / minFrameDuration.value;
        
        [_videoFrame clearPlanes];
        for (int i = 0; i < CVPixelBufferGetPlaneCount(imageBuffer); i++) {
            [_videoFrame.planes addPointer:
             CVPixelBufferGetBaseAddressOfPlane(imageBuffer, i)];
        }
        _videoFrame.orientation = [self currentDeviceOrientation];
        
		[_videoCaptureConsumer consumeFrame:_videoFrame];
        
		CVPixelBufferUnlockBaseAddress(imageBuffer, 0);
    }
}


@end
