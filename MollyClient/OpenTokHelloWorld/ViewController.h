//
//  ViewController.h
//  SampleApp
//
//  Created by Charley Robinson on 12/13/11.
//  Copyright (c) 2011 Tokbox, Inc. All rights reserved.
//

#import <UIKit/UIKit.h>
#import <Opentok/Opentok.h>
#import "GABLocationSocketSender.h"

@interface ViewController : UIViewController <OTSessionDelegate, OTSubscriberDelegate, OTPublisherDelegate>{
    GABLocationSocketSender* locationSender;
    IBOutlet UIView* indicatorView;

}
- (void)doConnect;
- (void)doPublish;
- (void)showAlert:(NSString*)string;
-(IBAction)sendLocation:(id)sender;

@end
