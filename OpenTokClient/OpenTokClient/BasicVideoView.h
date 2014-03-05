//
//  BasicVideoView.h
//  ot_mac_basic
//
//  Created by Charley Robinson on 9/20/13.
//  Copyright (c) 2013 tokbox.com. All rights reserved.
//

#import <Cocoa/Cocoa.h>
#import <OpenTok-OSX/OpenTok.h>
#import <Syphon/Syphon.h>
#import <GLKit/GLKit.h>

@interface BasicVideoView : NSImageView <OTVideoRender>

- (NSImage*) getImageHolder;

- (void) publishToSyphonServer:(SyphonServer*) syServer;

- (void)renderVideoFrame:(OTVideoFrame*) frame;

- (void)offerFrame:(uint8_t*)frame width:(size_t)width height:(size_t)height;

- (void)clearRenderBuffer;
- (void) setupSyphon;
- (void) sendTexture:(GLKTextureInfo*) theTexture;

@end
