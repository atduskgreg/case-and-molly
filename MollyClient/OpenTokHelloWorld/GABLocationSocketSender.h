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
}

- (void) connect;
- (void) send:(id)message;
- (void) disconnect;

@property (nonatomic, retain) CLLocation* lastLocation;
@property (nonatomic, retain) UIView* outputView;


//@property (nonatomic, retain) CLLocationManager *locationManager;

- (void)locationManager:(CLLocationManager *)manager
	 didUpdateLocations:(NSArray *)locations;

- (void)locationManager:(CLLocationManager *)manager
       didFailWithError:(NSError *)error;

- (void)webSocket:(SRWebSocket *)webSocket didReceiveMessage:(id)message;

- (void)webSocketDidOpen:(SRWebSocket *)webSocket;
- (void)webSocket:(SRWebSocket *)webSocket didFailWithError:(NSError *)error;

-(void) sendLocation;

@end
