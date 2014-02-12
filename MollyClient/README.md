The OpenTokHello Sample App
===========================

The OpenTokHello sample app is a basic sample app that shows the most basic features of the OpenTok iOS SDK.

Once the app connects to the OpenTok session, it publishes an audio-video stream, which is displayed onscreen.
Then, the same audio-video stream shows up as a subscribed stream (along with any other streams currently in the
session).

Before you test the sample app, be sure to read the README file for
[the OpenTok on WebRTC iOS SDK](../../README.md).

Testing the sample app
----------------------

1.  Open OpenTokHelloWorld.xcodeproj in XCode.

2.  Connect your iOS device to a USB port on your Mac. Make sure that your device is provisioned and connected to the
    internet. For more information, see the section on "Setting up your development environment" at 
    [this page](https://developer.apple.com/programs/ios/gettingstarted/) at the Apple iOS Dev Center.

    ***Note:*** the OpenTok iOS SDK does not support publishing streams in the XCode iOS Simulator. To test OpenTok apps
    on an iOS device, you will need to register as an Apple iOS developer at
    [http://developer.apple.com/programs/register/](http://developer.apple.com/programs/register/).

3.  Configure the project to use your API Key, your own Session, and a Token to access it.  If you don't have an 
    API Key [sign up for a Developer Account](https://dashboard.tokbox.com/signups/new). Then to generate the Session ID
    and Token, use the Project Tools on the [Project Details](https://dashboard.tokbox.com/projects) page.

    Since this app will subscribe to its own published stream (for demo purposes), be sure to create a session that
    uses the OpenTok media server -- do not create a peer-to-peer session. (You cannot subscribe to your own stream
    in a peer-to-peer session.)

    Open `ViewController.h` and modify `kApiKey`, `kSessionId`, and `kToken` with your own API key, session ID, and token,
    respectively. 

    Edit the browser_demo.html file and modify the variables `apiKey`, `sessionId`, and `token` with the API key,
    session ID, and token, respectively.

4.  Make sure your attached iOS device is selected in the Scheme chooser. Run the App (press the play button).

    The app should start on your connected device. Once the app connects to the OpenTok session, it publishes an audio-video
    stream, which is displayed onscreen. Then, the same audio-video stream shows up as a subscribed stream (along with any
    other streams currently in the session).

5.  Close the app. Now set up the app to subscribe to audio-video streams other than your own:
    -   Near the top of the `ViewController.m` file, change the `subscribeToSelf` property to be set to `NO`
    -   Run the app on your iOS device again.
    - Add the browser_demo.html file onto a web server.
    - Mute the audio on your Mac. Then a browser on your Mac, load the browser_demo.html file from the web server. 
    (The OpenTok iOS SDK includes echo suppression for subscriber and publisher streams in the same app. However,
    you may experience echoes when testing an app on an iOS device and in a nearby web browser.)
    -   In the web page, click the Connect and Publish buttons.


Understanding the code
----------------------

The `ViewController.m` file contains the main implementation code that includes use of the OpenTok iOS API.

### Initializing an OTSession object and connecting to an OpenTok session

When the view initially loads, the app allocates an OTSession object and sends it the 
`[OTSession initWithSessionId:andDelegate:]` message:

```objc
- (void)viewDidLoad
{
  [super viewDidLoad];

  _session = [[OTSession alloc] initWithSessionId:kSessionId
                                      andDelegate:self];
  [self doConnect];
}
```

The `kSessionId` constant is the session ID string for the OpenTok session your app connects to. This can be generated from
the [Developer Dashboard](https://dashboard.tokbox.com/projects) or from a
[server-side library](http://www.tokbox.com/opentok/docs/concepts/server_side_libraries.html).

The `doConnect` method sends the [OTSession connectWithApiKey:token:] to the the `_session` object:

```objc
- (void)doConnect 
{
  [_session connectWithApiKey:kApiKey token:kToken];
}
```

The `kToken` constant is the token constant for the client connecting to the session. See [Connection Token Creation](http://www.tokbox.com/opentok/docs/concepts/token_creation.html) for details.

When the app connects to the OpenTok session, the OTSessionDelegate (which is the ViewController instance) is sent the
`sessionDidConnect:` message:

```objc
- (void)sessionDidConnect:(OTSession*)session
{
  NSLog(@"sessionDidConnect (%@)", session.sessionId);
  [self doPublish];
}
```

### Publishing an audio-video stream to a session

The `doPublish` method allocates and initializes an OTPublisher object, and then sends the `publish:` message to
the `_session` object:

```objc
- (void)doPublish
{
  _publisher = [[OTPublisher alloc] initWithDelegate:self];
  [_publisher setName:[[UIDevice currentDevice] name]];
  [_session publish:_publisher];
  [self.view addSubview:_publisher.view];
  [_publisher.view setFrame:CGRectMake(0, 0, widgetWidth, widgetHeight)];
}
```

The name of a stream is an optional string that appears at the bottom of the stream's view when the user taps the stream
(or clicks it in a browser). You set the name for the stream by setting the `name` property of the OTPublisher object
before you pass it to the session with the `publish:` message.

The view of the OTPublisher object is added as a subview of the ViewController's view.

### Subscribing to streams

When a stream is added to a session, the OTSessionDelegate is sent the `session:didReceiveStream:` message. It then
allocates and initializes an OTSubscriber object for the stream.

```objc
- (void)session:(OTSession*)mySession didReceiveStream:(OTStream*)stream
{
  NSLog(@"session didReceiveStream (%@)", stream.streamId);

  if ( ( subscribeToSelf &&  [stream.connection.connectionId isEqualToString: _session.connection.connectionId]) ||
       (!subscribeToSelf && ![stream.connection.connectionId isEqualToString: _session.connection.connectionId]) ) {
    if (!_subscriber) {
      _subscriber = [[OTSubscriber alloc] initWithStream:stream delegate:self];
    }
  }
}
```

This app subscribes to one stream, at most. It either subscribes to the stream you publish, or it subscribes to one
of the other streams in the session (if there is one), based on the `subscribeToSelf` property, which is set at the
top of the file.

The Connection ID for the stream you publish with match the Connection ID for your Session. (***Note:*** in a real
application, a client would not normally subscribe to its own published stream. However, for this test app, it is
convenient for the client to subscribe to its own stream.)

If the session does not yet have the `_subscriber` property set to an OTSubscriber object, it does so in this method.

The OTSubscriberDelegate is sent the `subscriberDidConnectToStream` message when the subscriber connects to the
stream. At this point, the code adds the OTSubscriber's view to as a subview of the ViewController:

```objc
- (void)subscriberDidConnectToStream:(OTSubscriber*)subscriber
{
  NSLog(@"subscriberDidConnectToStream (%@)", subscriber.stream.connection.connectionId);
  [subscriber.view setFrame:CGRectMake(widgetWidth * subscriberCount++, widgetHeight, widgetWidth, widgetHeight)];
  [self.view addSubview:subscriber.view];
}
```

The OpenTok iOS SDK supports a limited number of simultaneous audio-video streams in an app:

- On iPad 2 / 3 / 4, the limit is four streams. An app can have up to four simultaneous subscribers, or one publisher
  and up to three subscribers.
- On all other supported iOS devices, the limit is two streams. An app can have up to two subscribers, or one publisher
  and one subscriber.

### Removing dropped streams

As streams leave the session (when clients disconnect or stop publishing), the OTSessionDelegate is sent the 
`session:didDropStream:` message. 

Subscriber objects are automatically removed from their superview when their stream is dropped.

The code checks to see if you are subscribing to streams other than your own. If so, it checks to see if the
dropped stream matches the subscriber's stream. It does this by comparing the `streamId` property of the dropped
OTStream object with the `stream.streamId` property of the OTSubscriber:

```objc
- (void)session:(OTSession*)session didDropStream:(OTStream*)stream
{
  NSLog(@"session didDropStream (%@)", stream.streamId);

  if ( !subscribeToSelf
       && _subscriber
       && [_subscriber.stream.streamId isEqualToString: stream.streamId] ) {
    _subscriber = nil;
    [self updateSubscriber];
  }
}
```

The `updateSubscriber` method subscribes to another stream in the session (other than the one you publish), if there is one. The
`OTSession.streams` property is a dictionary of all streams in a session:

```objc
- (void)updateSubscriber
{
  for (NSString* streamId in _session.streams) {
    OTStream* stream = [_session.streams valueForKey:streamId];
    if (stream.connection.connectionId != _session.connection.connectionId) {
      _subscriber = [[OTSubscriber alloc] initWithStream:stream delegate:self];
      break;
    }
  }
}
```

Knowing when you have disconnected from the session
---------------------------------------------------

Finally, when the app disconnects from the session, the OTSubscriberDelegate is sent the `sessionDidDisconnect:` message:

```objc
- (void)sessionDidDisconnect:(OTSession*)session
{
  NSString* alertMessage = [NSString stringWithFormat:@"Session disconnected: (%@)", session.sessionId];
  NSLog(@"sessionDidDisconnect (%@)", alertMessage);
}
```

If an app cannot connect to the session (perhaps because of no network connection), the OTSubscriberDelegate is sent
the `session:didFailWithError:` message:

```objc
- (void)session:(OTSession*)session didFailWithError:(OTError*)error
{
  NSLog(@"sessionDidFail");
  [self showAlert:[NSString stringWithFormat:@"There was an error connecting to session %@", session.sessionId]];
}
```

## Next steps

The OpenTokFullTutorial sample app uses more features of the OpenTok iOS SDK than the OpenTokHello app does.

For details on the full OpenTok on WebRTC iOS API, see the [reference documentation](http://www.tokbox.com/opentok/webrtc/docs/ios/reference/index.html).

