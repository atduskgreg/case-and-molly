//
//  GABAppDelegate.m
//  OpenTokClient
//
//  Created by Greg Borenstein on 1/17/14.
//  Copyright (c) 2014 Greg Borenstein. All rights reserved.
//

#import "GABAppDelegate.h"

@implementation GABAppDelegate

static NSString* const kApiKey = @"44604152";    // Replace with your OpenTok API key
static NSString* const kSessionId = @"1_MX40NDYwNDE1Mn5-TW9uIEphbiAyMCAxMzowNjowMyBQU1QgMjAxNH4wLjUwMjg5NjY3fg"; // Replace with your generated session ID
static NSString* const kToken = @"T1==cGFydG5lcl9pZD00NDYwNDE1MiZzZGtfdmVyc2lvbj10YnJ1YnktdGJyYi12MC45MS4yMDExLTAyLTE3JnNpZz00MTRiODlkYmMyYjU2ZTUwMzA5YjQwYWFiZjY4OWRkYmMyMmY2MTAzOnJvbGU9c3Vic2NyaWJlciZzZXNzaW9uX2lkPTFfTVg0ME5EWXdOREUxTW41LVRXOXVJRXBoYmlBeU1DQXhNem93Tmpvd015QlFVMVFnTWpBeE5INHdMalV3TWpnNU5qWTNmZyZjcmVhdGVfdGltZT0xMzkwMjUxOTcxJm5vbmNlPTAuMDM0ODg0MjYzMDE1OTE4ODUmZXhwaXJlX3RpbWU9MTM5Mjg0Mzk3MiZjb25uZWN0aW9uX2RhdGE9";

// Replace with your generated token (use the Dashboard or an OpenTok server-side library)

static bool subscribeToSelf = NO; // Chaange to NO to subscribe streams other than your own


- (void)applicationDidFinishLaunching:(NSNotification *)aNotification
{
    
    // Insert code here to initialize your application
    [self doConnect];
//    [[[NSApplication sharedApplication] mainWindow] setContentView:videoView];
    
//    [self setupSyphon];
}


- (void)doConnect
{
    _session = [[OTSession alloc] initWithSessionId:kSessionId
                                           delegate:self];
    [_session addObserver:self
               forKeyPath:@"connectionCount"
                  options:NSKeyValueObservingOptionNew
                  context:nil];
    [_session connectWithApiKey:kApiKey token:kToken];
}

- (void)doDisconnect
{
    [_session disconnect];
}



#pragma mark - OTSessionDelegate methods

- (void)sessionDidConnect:(OTSession*)session
{
    NSLog(@"sessionDidConnect: %@", session.sessionId);
    NSLog(@"- connectionId: %@", session.connection.connectionId);
    NSLog(@"- creationTime: %@", session.connection.creationTime);
}

- (void)sessionDidDisconnect:(OTSession*)session
{
    NSLog(@"sessionDidDisconnect: %@", session.sessionId);
}

- (void)session:(OTSession*)session didFailWithError:(OTError*)error
{
    NSLog(@"session: didFailWithError:");
    NSLog(@"- error code: %ld", (long)error.code);
    NSLog(@"- description: %@", error.localizedDescription);
}

- (void)session:(OTSession*)session streamCreated:(OTStream*)stream{
    NSLog(@"session: streamCreated:");
    _subscriber = [[OTSubscriberKit alloc] initWithStream:stream delegate:self];
    _subscriber.subscribeToAudio = NO;
    _subscriber.subscribeToVideo = YES;
    [_subscriber setVideoRender:videoView];
    [_subscriber subscribe];
    
}



- (void)session:(OTSession*)session streamDestroyed:(OTStream*)stream{
    NSLog(@"session: streamDestroyed:");

}



- (void)session:(OTSession*)mySession didReceiveStream:(OTStream*)stream
{
    NSLog(@"session: didReceiveStream:");
    NSLog(@"- connection.connectionId: %@", stream.connection.connectionId);
    NSLog(@"- connection.creationTime: %@", stream.connection.creationTime);
    NSLog(@"- session.sessionId: %@", stream.session.sessionId);
    NSLog(@"- streamId: %@", stream.streamId);
    NSLog(@"- type %@", stream.type);
    NSLog(@"- creationTime %@", stream.creationTime);
    NSLog(@"- name %@", stream.name);
    NSLog(@"- hasAudio %@", (stream.hasAudio ? @"YES" : @"NO"));
    NSLog(@"- hasVideo %@", (stream.hasVideo ? @"YES" : @"NO"));
    if ( (subscribeToSelf && [stream.connection.connectionId isEqualToString: _session.connection.connectionId])
        ||
        (!subscribeToSelf && ![stream.connection.connectionId isEqualToString: _session.connection.connectionId])
        ) {
//        if (!_subscriber) {
//            _subscriber = [[OTSubscriber alloc] initWithStream:stream delegate:self];
//            _subscriber.subscribeToAudio = YES;
//            _subscriber.subscribeToVideo = YES;
//        }
//        NSLog(@"subscriber.session.sessionId: %@", _subscriber.session.sessionId);
//        NSLog(@"- stream.streamId: %@", _subscriber.stream.streamId);
//        NSLog(@"- subscribeToAudio %@", (_subscriber.subscribeToAudio ? @"YES" : @"NO"));
//        NSLog(@"- subscribeToVideo %@", (_subscriber.subscribeToVideo ? @"YES" : @"NO"));
    }
}

- (void)session:(OTSession*)session didDropStream:(OTStream*)stream
{
    NSLog(@"session didDropStream (%@)", stream.streamId);
//    if (!subscribeToSelf
//        && _subscriber
//        && [_subscriber.stream.streamId isEqualToString: stream.streamId]) {
//        _subscriber = nil;
//        _unsubscribeButton.hidden = YES;
//        [self updateSubscriber];
//    }
}

- (void)session:(OTSession *)session didCreateConnection:(OTConnection *)connection {
    NSLog(@"session didCreateConnection (%@)", connection.connectionId);
}

- (void) session:(OTSession *)session didDropConnection:(OTConnection *)connection {
    NSLog(@"session didDropConnection (%@)", connection.connectionId);
}



#pragma mark - OTSubscriberKitDelegate methods

- (void)subscriberDidConnectToStream:(OTSubscriberKit*)subscriber
{
    NSLog(@"subscriberDidConnectToStream (%@)", subscriber.stream.connection.connectionId);

}

- (void)subscriber:(OTSubscriberKit*)subscriber didFailWithError:(OTError*)error
{

    NSLog(@"subscriber: %@ didFailWithError: ", subscriber.stream.streamId);
    NSLog(@"- code: %ld", (long)error.code);
    NSLog(@"- description: %@", error.localizedDescription);
}

- (void)subscriberVideoDataReceived:(OTSubscriberKit*)subscriber
{
//    [videoView publishToSyphonServer:syphonServer];
//    [videoView drawRect:NSMakeRect(0, 0, 640, 480)];

    
//    NSLog(@"- hasVideo %@", (subscriber.stream.hasVideo ? @"YES" : @"NO"));

//    NSLog(@"subscriberVideoDataReceived (%@)", subscriber.stream.streamId);
    //NSLog(@"videoView: %@", [videoView imageFrameStyle]);
    
//    NSImage* img = [videoView getImageHolder];
    
//    NSLog(@"img:%@", img);
//    
//        [img drawAtPoint:NSMakePoint(0.0, 0.0)
//                    fromRect: NSMakeRect(0.0, 0.0, 640, 480)
//                   operation: NSCompositeSourceOver
//                    fraction: 1.0];

}


@end
