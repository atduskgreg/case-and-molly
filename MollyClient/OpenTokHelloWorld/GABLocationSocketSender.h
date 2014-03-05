//
//  GABLocationSocketSender.h
//  GPSWebsocketsTest
//
//  Created by Greg Borenstein on 1/23/14.
//  Copyright (c) 2014 Greg Borenstein. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <CoreLocation/CoreLocation.h>
#import "SRWebSocket.h"

@interface GABLocationSocketSender : NSObject<CLLocationManagerDelegate,SRWebSocketDelegate>{
    CLLocationManager* locationManager;
    SRWebSocket* webSocket;
    BOOL socketIsOpen;
    CLLocation* lastLocation;
    NSDate* gameStartTime;
    BOOL gameStarted;
    NSDate* lastConnectionAttempt;
    BOOL isAttemptingReconnect;
    NSTimer* reconnectTimer;
    NSDate* lastMsgAt;
}

- (void) connect;
- (void) send:(id)message;
- (void) disconnect;
//- (void) attemptReconnect:(NSTimer*) timer;
-(void) checkWSConnection:(NSTimer*) timer;

@property (nonatomic, retain) CLLocation* lastLocation;
@property (nonatomic, retain) UIView* outputView;
@property (nonatomic, retain) UIImageView* squareView;
@property (nonatomic, retain) UIImageView* triangleView;
@property (nonatomic, retain) UILabel* caseStatus;
@property (nonatomic, retain) UIButton* hereButton;
@property (nonatomic, retain) UIView* lostView;


//@property (nonatomic, retain) CLLocationManager *locationManager;

- (void)locationManager:(CLLocationManager *)manager
	 didUpdateLocations:(NSArray *)locations;

- (void)locationManager:(CLLocationManager *)manager
       didFailWithError:(NSError *)error;

- (void)webSocket:(SRWebSocket *)webSocket didReceiveMessage:(id)message;

- (void)webSocketDidOpen:(SRWebSocket *)webSocket;
- (void)webSocket:(SRWebSocket *)webSocket didFailWithError:(NSError *)error;
- (void)webSocket:(SRWebSocket *)webSocket didCloseWithCode:(NSInteger)code reason:(NSString *)reason wasClean:(BOOL)wasClean;



-(void) sendLocation;

-(void) sendHere;
-(void) sendPing;
-(NSTimeInterval) elapsedGameTime;

@end
