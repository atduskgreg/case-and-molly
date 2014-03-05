//
//  GABAppDelegate.m
//  OpenTokClient
//
//  Created by Greg Borenstein on 1/17/14.
//  Copyright (c) 2014 Greg Borenstein. All rights reserved.
//

#import "GABAppDelegate.h"

@implementation GABAppDelegate


//========iOS===========
static NSString* const kApiKey = @"44604152";    // Replace with your OpenTok API key
static NSString* const kSessionId = @"2_MX40NDYwNDE1Mn5-VGh1IEphbiAwOSAxMjowNDo1MCBQU1QgMjAxNH4wLjY4NTE4ODh-";
static NSString* const kToken = @"T1==cGFydG5lcl9pZD00NDYwNDE1MiZzZGtfdmVyc2lvbj10YnJ1YnktdGJyYi12MC45MS4yMDExLTAyLTE3JnNpZz0zZTQzMmUzYTM4YmVmNzk3OTJiNGQ2NDc5ZjAxOGI5OWE5ODY5Yzg2OnJvbGU9c3Vic2NyaWJlciZzZXNzaW9uX2lkPTJfTVg0ME5EWXdOREUxTW41LVZHaDFJRXBoYmlBd09TQXhNam93TkRvMU1DQlFVMVFnTWpBeE5INHdMalk0TlRFNE9EaC0mY3JlYXRlX3RpbWU9MTM5MjIyNTUxNCZub25jZT0wLjQ4MTg3MjM5MDQzMzUyODAzJmV4cGlyZV90aW1lPTEzOTQ4MTc1MDImY29ubmVjdGlvbl9kYXRhPQ==";

//========iOS HIGH DEF===========
//static NSString* const kApiKey = @"44675712";
//// Replace with your generated session ID
//static NSString* const kSessionId = @"1_MX40NDY3NTcxMn5-TW9uIE1hciAwMyAxMjozMjowNCBQU1QgMjAxNH4wLjA0NDMxMTEwNn4";
//// Replace with your generated token
//static NSString* const kToken = @"T1==cGFydG5lcl9pZD00NDY3NTcxMiZzZGtfdmVyc2lvbj10YnJ1YnktdGJyYi12MC45MS4yMDExLTAyLTE3JnNpZz05MTAwOGEyOTc3YzFmMzQxMjM3Mzk0YWIzYzI5MzhhMjBmZTI5MjQzOnJvbGU9cHVibGlzaGVyJnNlc3Npb25faWQ9MV9NWDQwTkRZM05UY3hNbjUtVFc5dUlFMWhjaUF3TXlBeE1qb3pNam93TkNCUVUxUWdNakF4Tkg0d0xqQTBORE14TVRFd05uNCZjcmVhdGVfdGltZT0xMzkzODc4NzM0Jm5vbmNlPTAuODQ5Mjc3MjQ3MDM2NTQzNSZleHBpcmVfdGltZT0xMzk2NDcwNzE5JmNvbm5lY3Rpb25fZGF0YT0=";

//=======BROWSER=========
/*static NSString* const kSessionId = @"1_MX40NDYwNDE1Mn5-TW9uIEphbiAyMCAxMzowNjowMyBQU1QgMjAxNH4wLjUwMjg5NjY3fg"; // Replace with your generated session ID
static NSString* const kToken = @"T1==cGFydG5lcl9pZD00NDYwNDE1MiZzZGtfdmVyc2lvbj10YnJ1YnktdGJyYi12MC45MS4yMDExLTAyLTE3JnNpZz00MTRiODlkYmMyYjU2ZTUwMzA5YjQwYWFiZjY4OWRkYmMyMmY2MTAzOnJvbGU9c3Vic2NyaWJlciZzZXNzaW9uX2lkPTFfTVg0ME5EWXdOREUxTW41LVRXOXVJRXBoYmlBeU1DQXhNem93Tmpvd015QlFVMVFnTWpBeE5INHdMalV3TWpnNU5qWTNmZyZjcmVhdGVfdGltZT0xMzkwMjUxOTcxJm5vbmNlPTAuMDM0ODg0MjYzMDE1OTE4ODUmZXhwaXJlX3RpbWU9MTM5Mjg0Mzk3MiZjb25uZWN0aW9uX2RhdGE9";*/

// Replace with your generated token (use the Dashboard or an OpenTok server-side library)

//static bool subscribeToSelf = NO; // Chaange to NO to subscribe streams other than your own



- (void)applicationDidFinishLaunching:(NSNotification *)aNotification
{
    
    // Insert code here to initialize your application
    [self doConnect];
    [videoView setupSyphon];
    
    everConnected = false;
    
    NSString* path = [[NSBundle mainBundle] pathForResource:@"connectionLost" ofType:@"png"];
    lostTexture = [GLKTextureLoader textureWithContentsOfFile:path options:NULL error:NULL];
    [videoView sendTexture:lostTexture];

//    [NSTimer scheduledTimerWithTimeInterval:1
//                                     target:self
//                                   selector:@selector(checkConnection:)
//                                   userInfo:nil repeats:YES];
}

-(void) checkConnection:(NSTimer *) timer
{
    NSDate* nowDate =[NSDate date];
    NSLog(@"Time between frames: %.20f", [nowDate timeIntervalSinceDate:lastFrameTime]);
    
    if([nowDate timeIntervalSinceDate:lastFrameTime] > 2){
//        [self doDisconnect];
//        [self doConnect];
        NSTimeInterval t = [[NSDate date] timeIntervalSinceDate:lastReconnectTry];
        if(t > 5){
            [self doConnect];
        }
    }
}


- (void)doConnect
{
    lastReconnectTry = [NSDate date];

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
    NSLog(@"streamIDs: %@", [[session streams] allKeys]);

    
    if([[[session streams] allKeys] count] > 0){
        NSObject* keyForFirstStream = [[[session streams] allKeys] objectAtIndex:0];
        NSLog(@"");
        OTStream* stream =[[session streams] objectForKey:keyForFirstStream];
        [self subscribeToStream:stream];
    }
    

}

-(void) subscribeToStream:(OTStream*) stream{
    _subscriber = [[OTSubscriberKit alloc] initWithStream:stream delegate:self];
    _subscriber.subscribeToAudio = YES;
    _subscriber.subscribeToVideo = YES;
    [_subscriber setVideoRender:videoView];
    [_subscriber subscribe];
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

    [self subscribeToStream:stream];
}



- (void)session:(OTSession*)session streamDestroyed:(OTStream*)stream{
    NSLog(@"session: streamDestroyed:");
    [videoView sendTexture:lostTexture];
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
//    if ( (subscribeToSelf && [stream.connection.connectionId isEqualToString: _session.connection.connectionId])
//        ||
//        (!subscribeToSelf && ![stream.connection.connectionId isEqualToString: _session.connection.connectionId])
//        ) {
////        if (!_subscriber) {
////            _subscriber = [[OTSubscriber alloc] initWithStream:stream delegate:self];
////            _subscriber.subscribeToAudio = YES;
////            _subscriber.subscribeToVideo = YES;
////        }
////        NSLog(@"subscriber.session.sessionId: %@", _subscriber.session.sessionId);
////        NSLog(@"- stream.streamId: %@", _subscriber.stream.streamId);
////        NSLog(@"- subscribeToAudio %@", (_subscriber.subscribeToAudio ? @"YES" : @"NO"));
////        NSLog(@"- subscribeToVideo %@", (_subscriber.subscribeToVideo ? @"YES" : @"NO"));
//    }
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
    
      lastFrameTime = [NSDate date];

    
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
