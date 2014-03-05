//
//  GABLocationSocketSender.m
//  GPSWebsocketsTest
//
//  Created by Greg Borenstein on 1/23/14.
//  Copyright (c) 2014 Greg Borenstein. All rights reserved.
//

#import "GABLocationSocketSender.h"

@implementation GABLocationSocketSender

@synthesize lastLocation;

- (id) init {
    self = [super init];
    if (self != nil) {
        locationManager = [[CLLocationManager alloc] init];
        locationManager.desiredAccuracy = kCLLocationAccuracyBest;
        locationManager.delegate = self;
        [locationManager startUpdatingLocation];
        isAttemptingReconnect = NO;
        [self connect];
        lastMsgAt = [NSDate date];
        
        [NSTimer scheduledTimerWithTimeInterval:1.1
                                         target:self
                                       selector:@selector(checkWSConnection:)
                                       userInfo:nil repeats:YES];
    }
    return self;
}

-(void) checkWSConnection:(NSTimer*) timer
{
    NSTimeInterval timeSinceLastMsg = [[NSDate date] timeIntervalSinceDate:lastMsgAt];
    NSTimeInterval timeSinceLastConnectionAttempt = [[NSDate date] timeIntervalSinceDate:lastConnectionAttempt];
    
//    NSLog(@"checkWSConnection: timeSinceLastMsg: %f, timeSinceLastConnectionAttempt: %f",timeSinceLastMsg, timeSinceLastConnectionAttempt);
    if(  (timeSinceLastMsg > 5) && (timeSinceLastConnectionAttempt > 2)){
        NSLog(@"RECONNNNECTING");
        [_lostView setHidden:NO];
        
        NSLog(@"readyState: %d",[webSocket readyState] );
        if([webSocket readyState] != SR_OPEN && [webSocket readyState] != SR_CONNECTING){
            [self connect];
        }
    }
    
}


- (void) connect
{
    NSLog(@"connecting...");
    NSString* url = @"ws://case-and-molly-server.herokuapp.com";
    NSURLRequest* request = [[NSURLRequest alloc] initWithURL:[[NSURL alloc] initWithString:url]];
    webSocket = [[SRWebSocket alloc] initWithURLRequest:request];
    webSocket.delegate = self;
    [webSocket open];
    
    socketIsOpen = NO;
    lastConnectionAttempt = [NSDate date];
}

- (void) disconnect
{
    [webSocket close];
}


- (void) send:(id)message{
    if(socketIsOpen == YES){
        [webSocket send:message];
    }
}

- (void) sendPing
{
    
    [self send:[[NSString alloc] initWithFormat:@"{\"ping\": %i}", (int)([NSDate timeIntervalSinceReferenceDate] * 1000)]];
}

-(void) sendLocation
{
    [self send:[self locationJSON]];
}


-(void) sendHere
{
    [self send:[[NSString alloc] initWithFormat:@"{\"molly\": 1, \"location\":{\"lat\":%f,\"lng\":%f}}", lastLocation.coordinate.latitude, lastLocation.coordinate.longitude ]];
}

-(NSString*) locationJSON
{
    return [[NSString alloc] initWithFormat:@"{\"location\":{\"lat\":%f,\"lng\":%f}}",lastLocation.coordinate.latitude, lastLocation.coordinate.longitude];
}


- (void)locationManager:(CLLocationManager *)manager
	 didUpdateLocations:(NSArray *)locations
{
    lastLocation = [locations lastObject];
    [self sendLocation];
}

- (void)locationManager:(CLLocationManager *)manager
       didFailWithError:(NSError *)error
{
    NSLog(@"locationManager didFailWithError: %@", error );
}

-(NSTimeInterval) elapsedGameTime
{
    if(gameStarted){
        return [[NSDate date] timeIntervalSinceDate:gameStartTime];
    } else {
        return 0.0f;
    }
}

- (void)webSocket:(SRWebSocket *)ws didReceiveMessage:(id)message
{
//    NSLog(@"webSocket:didReceiveMessage: %@", message);
    
    
    if(message != NULL){
        lastMsgAt = [NSDate date];
        [_lostView setHidden:YES];
        NSDictionary *result = [NSJSONSerialization JSONObjectWithData:[message dataUsingEncoding:NSUTF8StringEncoding] options:NSJSONReadingMutableLeaves error:nil];
        
        if([result objectForKey:@"start"] != nil){
            NSInteger startSignal = [[result objectForKey:@"start"] integerValue];
            if(startSignal == 1){
                //start game clock timer
                gameStartTime = [NSDate date];
                gameStarted = YES;
            }
        }
        
        if([result objectForKey:@"level"] != nil){
            NSInteger levelSignal = [[result objectForKey:@"level"] integerValue];
            if(levelSignal == 0){
                _caseStatus.text = @"Watching";
                [_hereButton setHidden:NO];

            } else { // case level
                _caseStatus.text = @"Hacking";
                [_hereButton setHidden:YES];
            }
        }
    
        if([result objectForKey:@"case"] != nil){
            NSLog(@"case: %@", [result objectForKey:@"case"]);
            NSInteger caseSignal = [[result objectForKey:@"case"] integerValue];
            if(caseSignal == 1){
                [_squareView setImage:[UIImage imageNamed:@"black_square.jpg"]];
            } else {
                [_triangleView setImage:[UIImage imageNamed:@"black_triangle.png"]];
            }
            [self fadeOutColor];
        }
    }
}

-(void) fadeOutColor
{
//    [UIView beginAnimations:nil context:NULL];
//    [UIView setAnimationDuration:2];
//    [_outputView setBackgroundColor:[UIColor colorWithRed:1.0 green:1.0 blue:1.0 alpha:1.0]];
//    [UIView commitAnimations];
    
    CABasicAnimation *crossFade = [CABasicAnimation animationWithKeyPath:@"contents"];
    crossFade.duration = 1.0;
    crossFade.fromValue = (id)_squareView.image.CGImage;
    crossFade.toValue = (id)[UIImage imageNamed:@"empty_square.png"].CGImage;
    [_squareView.layer addAnimation:crossFade forKey:@"animateContents"];
    
    CABasicAnimation *crossFadeT = [CABasicAnimation animationWithKeyPath:@"contents"];
    crossFadeT.duration = 1.0;
    crossFadeT.fromValue = (id)_triangleView.image.CGImage;
    crossFade.toValue = (id)[UIImage imageNamed:@"empty_triangle.png"].CGImage;
    [_triangleView.layer addAnimation:crossFadeT forKey:@"animateContents"];
    
    _squareView.image = [UIImage imageNamed:@"empty_square.png"];
    _triangleView.image = [UIImage imageNamed:@"empty_triangle.png"];

}


- (void)webSocketDidOpen:(SRWebSocket *)ws
{
    [reconnectTimer invalidate];
    socketIsOpen = YES;
    isAttemptingReconnect = NO;
    [_lostView setHidden:YES];
    NSLog(@"webSocketDidOpen");
    [webSocket send:@"join"];
}

- (void)webSocket:(SRWebSocket *)ws didFailWithError:(NSError *)error
{
    NSLog(@"webSocket didFailWithError: %@", error);
    [_lostView setHidden:NO];
    [ws close];
//    if([[NSDate date] timeIntervalSinceDate:lastConnectionAttempt] > 2){
//        [self connect];
//    }
//    if(isAttemptingReconnect == NO){
//        [self attemptReconnect:NULL];
//    }
    // give feedback that connection to case was lost (or in a timer that checks?)
//    if([[NSDate date] timeIntervalSinceDate:lastConnectionAttempt] > 2){
//        [self connect];
//    }

}
//
//- (void) attemptReconnect:(NSTimer*) timer
//{
//    
//    NSLog(@"lastattempt: %f", [[NSDate date] timeIntervalSinceDate:lastConnectionAttempt]);
//    if([[NSDate date] timeIntervalSinceDate:lastConnectionAttempt] > 2){
//        //if(!isAttemptingReconnect){
//            if([webSocket readyState] != SR_OPEN ||  [webSocket readyState] != SR_CONNECTING){
//                NSLog(@"attempting reconnect");
//                isAttemptingReconnect = YES;
//
//                [self connect];
//            
//            }
//    } else {
//        reconnectTimer = [NSTimer scheduledTimerWithTimeInterval:1.2
//                                        target:self
//                                        selector:@selector(attemptReconnect:)
//                                        userInfo:nil repeats:NO];
//
//    }
//    
//}
//


- (void)webSocket:(SRWebSocket *)webSocket
 didCloseWithCode:(NSInteger)code
           reason:(NSString *)reason
         wasClean:(BOOL)wasClean
{
    [_lostView setHidden:NO];
//    if(isAttemptingReconnect == NO){
//        [self attemptReconnect:NULL];
//    }

//    [self attemptReconnect:NULL];
    
//    // give feedback that connection to case was lost (or in a timer that checks?)
//    if([[NSDate date] timeIntervalSinceDate:lastConnectionAttempt] > 2){
//        [self connect];
//    }
    
}


@end
