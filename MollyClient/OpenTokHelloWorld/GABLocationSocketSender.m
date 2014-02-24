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
        
        [self connect];
    }
    return self;
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
}

- (void) disconnect
{
    [webSocket close];
}


- (void) send:(id)message{
    if(socketIsOpen == YES){
        [webSocket send:message];
        NSLog(@"sent: %@", message);
    }
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
   // NSLog(@"lss received: %f,%f", lastLocation.coordinate.latitude, lastLocation.coordinate.longitude);
  //  [self sendLocation];
    [self sendLocation];
    
}

- (void)locationManager:(CLLocationManager *)manager
       didFailWithError:(NSError *)error
{
    NSLog(@"locationManager didFailWithError: %@", error );
}

- (void)webSocket:(SRWebSocket *)ws didReceiveMessage:(id)message
{
    NSLog(@"webSocket:didReceiveMessage: %@", message);
    
    if(message != NULL){
        NSDictionary *result = [NSJSONSerialization JSONObjectWithData:[message dataUsingEncoding:NSUTF8StringEncoding] options:NSJSONReadingMutableLeaves error:nil];
    
    
        if([result objectForKey:@"case"] != nil){
            NSLog(@"case: %@", [result objectForKey:@"case"]);
            NSInteger caseSignal = [[result objectForKey:@"case"] integerValue];
            if(caseSignal == 1){
//                [_outputView setBackgroundColor:[UIColor colorWithRed:0.0 green:1.0 blue:0.0 alpha:1.0]];
                [_squareView setImage:[UIImage imageNamed:@"black_square.jpg"]];
            } else {
//                [_outputView setBackgroundColor:[UIColor colorWithRed:1.0 green:0.0 blue:0.0 alpha:1.0]];
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
    socketIsOpen = YES;
    NSLog(@"webSocketDidOpen");
    [webSocket send:@"join"];
}

- (void)webSocket:(SRWebSocket *)ws didFailWithError:(NSError *)error
{
    NSLog(@"webSocket didFailWithError: %@", error);
}

@end
