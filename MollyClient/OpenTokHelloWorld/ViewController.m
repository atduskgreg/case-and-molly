//
//  ViewController.m
//  SampleApp
//
//  Created by Charley Robinson on 12/13/11.
//  Copyright (c) 2011 Tokbox, Inc. All rights reserved.
//

#import "ViewController.h"

@implementation ViewController {
    OTSession* _session;
    OTPublisher* _publisher;
    OTSubscriber* _subscriber;
}
static double widgetHeight = 255;
static double widgetWidth = 300;

// *** Fill the following variables using your own Project info from the Dashboard  ***
// ***                   https://dashboard.tokbox.com/projects                      ***
static NSString* const kApiKey = @"44604152";    // Replace with your OpenTok API key
static NSString* const kSessionId = @"2_MX40NDYwNDE1Mn5-VGh1IEphbiAwOSAxMjowNDo1MCBQU1QgMjAxNH4wLjY4NTE4ODh-"; // Replace with your generated session ID
static NSString* const kToken = @"T1==cGFydG5lcl9pZD00NDYwNDE1MiZzZGtfdmVyc2lvbj10YnJ1YnktdGJyYi12MC45MS4yMDExLTAyLTE3JnNpZz1iNzg4NmJkMjA3MzY1NDFlYjZjZGI3NzVmOWIxMzRmOTBkOGQ0ZmIxOnJvbGU9cHVibGlzaGVyJnNlc3Npb25faWQ9Ml9NWDQwTkRZd05ERTFNbjUtVkdoMUlFcGhiaUF3T1NBeE1qb3dORG8xTUNCUVUxUWdNakF4Tkg0d0xqWTROVEU0T0RoLSZjcmVhdGVfdGltZT0xMzkyMjI1NjMzJm5vbmNlPTAuNDIyNjM1MDM1ODc2MDc5NTMmZXhwaXJlX3RpbWU9MTM5NDgxNzUwMiZjb25uZWN0aW9uX2RhdGE9";     // Replace with your generated token (use the Dashboard or an OpenTok server-side library)

static bool subscribeToSelf = NO; // Change to NO to subscribe to streams other than your own.

#pragma mark - View lifecycle

- (void)viewDidLoad
{
    [super viewDidLoad];
    _session = [[OTSession alloc] initWithSessionId:kSessionId
                                           delegate:self];
    locationSender = [[GABLocationSocketSender alloc] init];
//    [locationSender setOutputView:indicatorView];

    [lostView setHidden:YES];
    
    [locationSender setLostView:lostView];
    [locationSender setTriangleView:triangleView];
    [locationSender setSquareView:squareView];
    [locationSender setCaseStatus:caseStatus];
    [locationSender setHereButton:hereButton];
    
    [squareView setImage:[UIImage imageNamed:@"empty_square.png"]];
    [triangleView setImage:[UIImage imageNamed:@"empty_triangle.png"]];
   
    [hereButton setTitleColor:[UIColor greenColor] forState:UIControlStateHighlighted];
    
    
    
    [NSTimer scheduledTimerWithTimeInterval:0.01
                                     target:self
                                   selector:@selector(showTime:)
                                   userInfo:nil repeats:YES];
    
    [NSTimer scheduledTimerWithTimeInterval:1
                                     target:self
                                   selector:@selector(sendPing:)
                                   userInfo:nil repeats:YES];

    
    [self doConnect];
}

-(void) sendPing:(NSTimer *) timer
{
    [locationSender sendPing];
}


- (BOOL)prefersStatusBarHidden
{
    return YES;
}

-(BOOL)shouldAutorotate
{
    
    return UIInterfaceOrientationMaskLandscape;
    
}

-(NSUInteger)supportedInterfaceOrientations
{
    
    return UIInterfaceOrientationMaskLandscape;
    
}

- (UIInterfaceOrientation)preferredInterfaceOrientationForPresentation
{
    
    return UIInterfaceOrientationLandscapeLeft;
    
}

//- (BOOL)shouldAutorotateToInterfaceOrientation:(UIInterfaceOrientation)interfaceOrientation
//{
//    return NO;
//
//    // Return YES for supported orientations
////    if ([[UIDevice currentDevice] userInterfaceIdiom] == UIUserInterfaceIdiomPhone) {
////        return NO;
////    } else {
////        return YES;
////    }
//}

- (void)updateSubscriber {
    for (NSString* streamId in _session.streams) {
        OTStream* stream = [_session.streams valueForKey:streamId];
        if (![stream.connection.connectionId isEqualToString: _session.connection.connectionId]) {
            _subscriber = [[OTSubscriber alloc] initWithStream:stream delegate:self];
            break;
        }
    }
}

-(void) showTime:(NSTimer *) timer
{
    timeDisplay.text = [NSString stringWithFormat:@"%.02f", [locationSender elapsedGameTime]];
}


-(IBAction)sendHere:(id)sender{
    NSLog(@"elapsedGameTime: %f", [locationSender elapsedGameTime]);
    [locationSender sendHere];
}


#pragma mark - OpenTok methods

- (void)doConnect
{
    [_session connectWithApiKey:kApiKey token:kToken];
}

- (void)doPublish
{
    _publisher = [[OTPublisher alloc] initWithDelegate:self];
    [_publisher setName:[[UIDevice currentDevice] name]];
    [_session publish:_publisher];
    [self.view addSubview:_publisher.view];
    [_publisher.view setFrame:CGRectMake(indicatorView.frame.origin.x,indicatorView.frame.origin.y, indicatorView.frame.size.width,indicatorView.frame.size.height)];
}

- (void)sessionDidConnect:(OTSession*)session
{
    NSLog(@"sessionDidConnect (%@)", session.sessionId);
    [self doPublish];
}

- (void)sessionDidDisconnect:(OTSession*)session
{
    NSString* alertMessage = [NSString stringWithFormat:@"Session disconnected: (%@)", session.sessionId];
    NSLog(@"sessionDidDisconnect (%@)", alertMessage);
    [self showAlert:alertMessage];
}


- (void)session:(OTSession*)mySession didReceiveStream:(OTStream*)stream
{
    NSLog(@"session didReceiveStream (%@)", stream.streamId);
    
    // See the declaration of subscribeToSelf above.
    if ( (subscribeToSelf && [stream.connection.connectionId isEqualToString: _session.connection.connectionId])
        ||
        (!subscribeToSelf && ![stream.connection.connectionId isEqualToString: _session.connection.connectionId])
        ) {
        if (!_subscriber) {
            _subscriber = [[OTSubscriber alloc] initWithStream:stream delegate:self];
        }
    }
}

- (void)session:(OTSession*)session didDropStream:(OTStream*)stream{
    NSLog(@"session didDropStream (%@)", stream.streamId);
    NSLog(@"_subscriber.stream.streamId (%@)", _subscriber.stream.streamId);
    if (!subscribeToSelf
        && _subscriber
        && [_subscriber.stream.streamId isEqualToString: stream.streamId])
    {
        _subscriber = nil;
        [self updateSubscriber];
    }
}

- (void)session:(OTSession *)session didCreateConnection:(OTConnection *)connection {
    NSLog(@"session didCreateConnection (%@)", connection.connectionId);
}

- (void) session:(OTSession *)session didDropConnection:(OTConnection *)connection {
    NSLog(@"session didDropConnection (%@)", connection.connectionId);
}

- (void)subscriberDidConnectToStream:(OTSubscriber*)subscriber
{
    NSLog(@"subscriberDidConnectToStream (%@)", subscriber.stream.connection.connectionId);
    [subscriber.view setFrame:CGRectMake(0, widgetHeight, widgetWidth, widgetHeight)];
    [self.view addSubview:subscriber.view];
}

- (void)publisher:(OTPublisher*)publisher didFailWithError:(OTError*) error {
    NSLog(@"publisher didFailWithError %@", error);
    [self showAlert:[NSString stringWithFormat:@"There was an error publishing."]];
}

- (void)subscriber:(OTSubscriber*)subscriber didFailWithError:(OTError*)error
{
    NSLog(@"subscriber %@ didFailWithError %@", subscriber.stream.streamId, error);
    [self showAlert:[NSString stringWithFormat:@"There was an error subscribing to stream %@", subscriber.stream.streamId]];
}

- (void)session:(OTSession*)session didFailWithError:(OTError*)error {
    NSLog(@"sessionDidFail");
    [self showAlert:[NSString stringWithFormat:@"There was an error connecting to session %@", session.sessionId]];
}


- (void)showAlert:(NSString*)string {
    UIAlertView *alert = [[UIAlertView alloc] initWithTitle:@"Message from video session"
                                                    message:string
                                                   delegate:self
                                          cancelButtonTitle:@"OK"
                                          otherButtonTitles:nil];
    [alert show];
}

@end
