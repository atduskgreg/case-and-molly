//
//  GABAppDelegate.h
//  OpenTokClient
//
//  Created by Greg Borenstein on 1/17/14.
//  Copyright (c) 2014 Greg Borenstein. All rights reserved.
//

#import <Cocoa/Cocoa.h>
#import "BasicVideoView.h"
#import <Syphon/Syphon.h>

@interface GABAppDelegate : NSObject <NSApplicationDelegate, OTSessionDelegate, OTSubscriberKitDelegate>{
    IBOutlet BasicVideoView* videoView;
    OTSession* _session;
    OTSubscriberKit* _subscriber;
//    SyphonServer* syphonServer;
    NSDate* lastFrameTime;
    BOOL everConnected;
    NSDate* lastReconnectTry;
}

- (void)doConnect;
- (void)doDisconnect;
-(void) checkConnection:(NSTimer *) timer;

@property (assign) IBOutlet NSWindow *window;

@end
