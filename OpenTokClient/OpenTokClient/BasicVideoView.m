//
//  BasicVideoView.m
//  ot_mac_basic
//
//  Created by Charley Robinson on 9/20/13.
//  Copyright (c) 2013 tokbox.com. All rights reserved.
//

#import "BasicVideoView.h"
#import "libyuv.h"

@implementation BasicVideoView {
    NSImage* imageHolder;
    NSBitmapImageRep* imageHolderRep;
    uint8_t* frameHolder;
    CGSize _videoFrameSize;
    NSObject* lock;
    SyphonServer* syphonServer;
    GLKTextureInfo* texture;
    NSBitmapImageRep* imageRep;
//    GLKTextureInfo* prevFrame;
}

- (NSImage*) getImageHolder {
    return imageHolder;
}


- (void) setupSyphon{
    NSLog(@"setupSyphon");
    
    NSOpenGLPixelFormatAttribute attr[] =
    {
		NSOpenGLPFADoubleBuffer,
		NSOpenGLPFAAccelerated,
		NSOpenGLPFADepthSize, 24,
		NSOpenGLPFAMultisample,
		NSOpenGLPFASampleBuffers, 1,
		NSOpenGLPFASamples, 4,
		(NSOpenGLPixelFormatAttribute) 0
    };
    
    // Make our GL Pixel Format
    NSOpenGLPixelFormat* pf = [[NSOpenGLPixelFormat alloc] initWithAttributes:attr];
    
    if(!pf)
        NSLog(@"Could not create pixel format, falling back to simpler pixel format");
    
    NSOpenGLPixelFormatAttribute simpleattr[] =
    {
        NSOpenGLPFADoubleBuffer,
        NSOpenGLPFAAccelerated,
        (NSOpenGLPixelFormatAttribute) 0
    };
    
    pf = [[NSOpenGLPixelFormat alloc] initWithAttributes:simpleattr];
    
    if(!pf)
    {
        NSLog(@"Could not create pixel format, bailing");
        [NSApp terminate:self];
    }
    
    NSOpenGLContext* glContext = [[NSOpenGLContext alloc] initWithFormat:pf shareContext:nil];
    
    [glContext makeCurrentContext];
    
    syphonServer = [[SyphonServer alloc] initWithName:nil
                                              context:[glContext CGLContextObj]
                                              options:nil];
        
}


- (void)clearRenderBuffer {
    dispatch_async(dispatch_get_main_queue(), ^() {
        NSLog(@"clearRenderBuffer");

        NSImage* blankImage = [[NSImage alloc] initWithSize:_videoFrameSize];
        [blankImage setBackgroundColor:[NSColor blackColor]];
        [self setImage:blankImage];
        [blankImage release];
    });
}

- (void)renderVideoFrame:(OTVideoFrame*) frame {
//    NSLog(@"BasicVideoView renderVideoFrame:%ix%i", frame.format.imageWidth, frame.format.imageHeight);
    @synchronized(lock) {
        size_t width = frame.format.imageWidth;
        size_t height = frame.format.imageHeight;
        if (NULL == frameHolder || _videoFrameSize.width != width ||
            _videoFrameSize.height != height)
        {
            if(frameHolder != NULL) {
                free(frameHolder);
            }
            
            frameHolder = malloc(4 * width * height);
            
            _videoFrameSize = CGSizeMake(width, height);
        }
        
        const uint8_t* yPlane = [frame.planes pointerAtIndex:0];
        const uint8_t* uPlane = [frame.planes pointerAtIndex:1];
        const uint8_t* vPlane = [frame.planes pointerAtIndex:2];
        
        int yStride = [[frame.format.bytesPerRow objectAtIndex:0] intValue];
        // multiply chroma strides by 2 as bytesPerRow represents 2x2 subsample
        int uStride = [[frame.format.bytesPerRow objectAtIndex:1] intValue] * 2;
        int vStride = [[frame.format.bytesPerRow objectAtIndex:2] intValue] * 2;
        
        I420ToARGB(yPlane, yStride,
                   vPlane, uStride,
                   uPlane, vStride,
                   frameHolder,
                   frame.format.imageWidth * 4,
                   frame.format.imageWidth, frame.format.imageHeight);
        
	}

//    NSLog(@"renderVideoFrame: %d", (int)[NSThread isMainThread] );
    dispatch_async(dispatch_get_main_queue(), ^() {
        [self offerFrame:frameHolder
                   width:_videoFrameSize.width
                  height:_videoFrameSize.height];
    });

}

-(void) offerFrame:(uint8_t*)frame width:(size_t)width height:(size_t)height {
//    NSLog(@"offerFrame: %d", (int)[NSThread isMainThread] );
    
    @synchronized(lock) {
//        if (nil != imageHolderRep) {
//            [imageHolder removeRepresentation:imageHolderRep];
//            [imageHolderRep release];
//            imageHolderRep = nil;
//        }
        
        
        imageHolderRep = [[NSBitmapImageRep alloc]
                          initWithBitmapDataPlanes:&frameHolder
                          pixelsWide:width
                          pixelsHigh:height
                          bitsPerSample:8
                          samplesPerPixel:4
                          hasAlpha:YES
                          isPlanar:NO
                          colorSpaceName:NSDeviceRGBColorSpace
                          bytesPerRow:width*4
                          bitsPerPixel:32];
    
        imageHolder = [[NSImage alloc] initWithCGImage:[imageHolderRep CGImage] size:NSMakeSize(width,height)];

        imageRep =[[NSBitmapImageRep alloc] initWithData:[imageHolder TIFFRepresentation]];
//        CGImageRef pixelData = ;
        

        texture = [GLKTextureLoader textureWithCGImage:[imageRep CGImage] options:NULL error:NULL];
        
        NSLog(@"texture: %i %ix%i", texture.name, texture.width, texture.height);
        [syphonServer publishFrameTexture:texture.name
                            textureTarget:GL_TEXTURE_2D
                              imageRegion:NSMakeRect(0, 0, texture.width, texture.height)
                        textureDimensions:NSMakeSize(texture.width, texture.height)
                                  flipped:YES];
        
        [imageHolder release];
//        [texture release];
        [imageRep release];
        
        [imageHolderRep release];
        
        GLuint name = texture.name;
        glDeleteTextures(1, &name);
        
        [self setImage:nil];
   

       [self setNeedsDisplay:YES];
       [self drawRect:NSMakeRect(0, 0, 352, 288)];
    
//        [self.layer setNeedsDisplay];
        
       
    }
//    x++;
//     [self drawRect:NSMakeRect(0, 0, 640, 480)];
}

//- (void)awakeFromNib {
//    
//    //CALayer *newLayer = [CALayer layer];
//}


- (void) publishToSyphonServer:(SyphonServer*) syServer
{



}


- (id)initWithFrame:(NSRect)frame
{
    self = [super initWithFrame:frame];
    if (self) {
        NSLog(@"here: %@", self);
        self.layer = [CALayer layer];
//        self.layer.frame = [self bounds];
//        self.layer.delegate = self;
//        NSLog(@"delegate: %@", self.layer.delegate);
        self.wantsLayer = YES;
        
        imageHolder = [[NSImage alloc] init];
        imageHolderRep = nil;
        frameHolder = nil;
        lock = [[NSObject alloc] init];
        [self setImage:imageHolder];
    }
    return self;
}

- (void)dealloc {
    [imageHolder release];
    [imageHolderRep release];
    if (nil != frameHolder) {
        free(frameHolder);
    }
    [lock release];
    [super dealloc];
}

- (void)drawLayer:(CALayer *)layer inContext:(CGContextRef)ctx {
    NSLog(@"drawLayer:");
//    [self drawRect:NSMakeRect(0, 0, 640, 320)];
}
//
//- (void)drawRect:(NSRect)rect
//{
//   
//	[super drawRect:rect];
//    // Drawing code here.
//    NSRectFill(NSMakeRect(0, 0, 100, 100));
//    [[NSColor blueColor] setFill];
//    x++;
//    NSRectFill(NSMakeRect(50+x, 50, 25, 25));
//
////        [imageHolder drawAtPoint:NSMakePoint(0.0, 0.0)
////                    fromRect: NSMakeRect(0.0, 0.0, 500, 500)
////                   operation: NSCompositeSourceOver
////                    fraction: 1.0];
//    
////    NSLog(@"here, x:%i", x);
//    
//}

@end
