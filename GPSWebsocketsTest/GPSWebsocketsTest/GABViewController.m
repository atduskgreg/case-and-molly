//
//  GABViewController.m
//  GPSWebsocketsTest
//
//  Created by Greg Borenstein on 1/23/14.
//  Copyright (c) 2014 Greg Borenstein. All rights reserved.
//

#import "GABViewController.h"

@interface GABViewController ()

@end

@implementation GABViewController

- (void)viewDidLoad
{
    [super viewDidLoad];
    locationSender = [[GABLocationSocketSender alloc] init];
    // HERE: do this when correct websocket flag is set
    [indicatorView setBackgroundColor:[UIColor colorWithRed:1.0 green:0.0 blue:0.0 alpha:1.0]];
}

- (IBAction)segmentedControlChanged:(id)sender
{
    UISegmentedControl *s = (UISegmentedControl *)sender;
    
    NSLog(@"selectedSegmentIndex: %i", s.selectedSegmentIndex );
    float lat = 0;
    float lng = 0;
    
    switch(s.selectedSegmentIndex){
            case 0:
                //rad lab
                lat = 42.361811;
                lng = -71.090308;
                break;
            case 1:
                // e15
                lat = 42.360884;
                lng = -71.08783;
                break;
            case 2:
                // near hayward st
                lat = 42.361683;
                lng = -71.085443;
            break;
            case 3:
                //saxon tennis
                lat = 42.359516;
                lng = -71.087712;
            break;
    }
    [locationSender send:[[NSString alloc] initWithFormat:@"{\"location\":{\"lat\":%f,\"lng\":%f}}",lat, lng]];

}


-(IBAction)sendSwitchMessage:(id)sender
{
    [locationSender send:@"switch"];
}

-(IBAction)sendLocationMessage:(id)sender
{
    [locationSender sendLocation];
}

-(IBAction)connect:(id)sender
{
    NSLog(@"connect clicked");
    [locationSender connect];
}

-(IBAction)disconnect:(id)sender
{
    NSLog(@"disconnect clicked");
    [locationSender disconnect];
}

- (void)didReceiveMemoryWarning
{
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}

@end
