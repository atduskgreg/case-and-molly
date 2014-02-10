//
//  GABViewController.h
//  GPSWebsocketsTest
//
//  Created by Greg Borenstein on 1/23/14.
//  Copyright (c) 2014 Greg Borenstein. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "GABLocationSocketSender.h"

@interface GABViewController : UIViewController{
    GABLocationSocketSender* locationSender;
    IBOutlet UIView* indicatorView;
}

-(IBAction)sendSwitchMessage:(id)sender;
-(IBAction)sendLocationMessage:(id)sender;

-(IBAction)connect:(id)sender;
-(IBAction)disconnect:(id)sender;

- (IBAction)segmentedControlChanged:(id)sender;

@end
