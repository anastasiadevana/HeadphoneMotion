//------------------------------------------------------------------------------
// HeadphoneMotion - Unity plugin that exposes the CMHeadphoneMotionManager API
// GitHub: https://github.com/anastasiadevana/HeadphoneMotion
//------------------------------------------------------------------------------
//
// MIT License
//
// Copyright (c) 2020 Anastasia Devana
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//------------------------------------------------------------------------------

#import <CoreMotion/CoreMotion.h>
#import "UnityAppController.h"

// Delegates
@protocol HeadphoneConnectionDelegate <NSObject>
-(void) connectionStatusChanged:(bool) connected;
@end

@protocol RotationDelegate <NSObject>
-(void) rotationUpdated:(double)x y:(double)y z:(double)z w:(double)w;
@end
// End delegates

// HeadphoneMotion interface
@interface HeadphoneMotion : NSObject<CMHeadphoneMotionManagerDelegate>

@property (strong, nonatomic) CMHeadphoneMotionManager *motionManager;

-(void) setRotationDelegate:(id<RotationDelegate>) rotationDelegate;
-(void) setHeadphoneConnectionDelegate:(id<HeadphoneConnectionDelegate>) headphoneConnectionDelegate;
-(void) startTracking;
-(void) stopTracking;

@end
// End HeadphoneMotion interface

// HeadphoneMotion implementation
@implementation HeadphoneMotion : NSObject

@synthesize motionManager;

bool _headphonesConnected;
bool _tracking;

id __headphoneConnectionDelegate = nil;
id __rotationDelegate = nil;

// Make this a singleton
static HeadphoneMotion *_instance;
+(HeadphoneMotion*) instance
{
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        _instance = [[HeadphoneMotion alloc] init];
    });
    return _instance;
}

-(id) init
{
    self = [super init];
    if (self)
    {
        // Instantiate the motion manager, and assign self as the delegate.
        self.motionManager = [[CMHeadphoneMotionManager alloc] init];
        motionManager.delegate = self;
        
        NSLog(@"HeadphoneMotion init complete");
    }
        
    return self;
}

-(void) startTracking
{
    if (_tracking == false)
    {
        if (motionManager.isDeviceMotionAvailable)
        {
            NSLog(@"Motion is available. Started tracking motion");
            [motionManager startDeviceMotionUpdatesToQueue:[NSOperationQueue mainQueue]
            withHandler:^(CMDeviceMotion *motion, NSError *error)
            {
                if (motion)
                {
                    if (__rotationDelegate)
                    {
                        [__rotationDelegate rotationUpdated:motion.attitude.quaternion.x
                                                     y:motion.attitude.quaternion.y
                                                     z:motion.attitude.quaternion.z
                                                     w:motion.attitude.quaternion.w];
                    }
                }
            }];

            _tracking = true;
        } else {
            NSLog(@"Motion is not available");
        }
    }
}

-(void) stopTracking
{
    if (_tracking)
    {
        NSLog(@"Stopped tracking motion");
        
        [motionManager stopDeviceMotionUpdates];
        
        _tracking = false;
    }
}

-(void) setHeadphoneConnectionDelegate:(id<HeadphoneConnectionDelegate>) headphoneConnectionDelegate {
    __headphoneConnectionDelegate = headphoneConnectionDelegate;
    NSLog(@"Set the headphone connection delegate");
}

-(void) setRotationDelegate:(id<RotationDelegate>) rotationDelegate {
    __rotationDelegate = rotationDelegate;
    NSLog(@"Set the rotation delegate");
}

-(void) headphoneMotionManagerDidConnect:(CMHeadphoneMotionManager *)manager
{
    NSLog(@"Headphones connected");
    
    _headphonesConnected = true;

    if (__headphoneConnectionDelegate)
    {
        [__headphoneConnectionDelegate connectionStatusChanged:true];
    }
}

-(void) headphoneMotionManagerDidDisconnect:(CMHeadphoneMotionManager *)manager
{
    NSLog(@"Headphones disconnected");
    
    _headphonesConnected = false;
    
    if (__headphoneConnectionDelegate)
    {
        [__headphoneConnectionDelegate connectionStatusChanged:false];
    }
}

-(bool) isHeadphoneMotionAvailable {
    if (motionManager.isDeviceMotionAvailable)
        return true;
    else
        return false;
}

-(bool) areHeadphonesConnected
{
    return _headphonesConnected;
}

@end
// End HeadphoneMotion Implementation

// Expose methods to Unity
extern "C"
{
    // Delegate wrappers for Unity
    typedef void (*RotationDelegateCallback)(double x, double y, double z, double w);
    RotationDelegateCallback rotationDelegate = NULL;

    @interface RotationDelegateUnityBridge : NSObject<RotationDelegate>
    @end
    static RotationDelegateUnityBridge *_rotationDelegate = nil;

    typedef void (*HeadphoneConnectionDelegateCallback)(bool connected);
    HeadphoneConnectionDelegateCallback headphoneConnectionDelegate = NULL;

    @interface HeadphoneConnectionDelegateUnityBridge : NSObject<HeadphoneConnectionDelegate>
    @end
    static HeadphoneConnectionDelegateUnityBridge *_headphoneConnectionDelegate = nil;
    // End delegate wrappers for Unity

    bool isHeadphoneMotionAvailable()
    {
        return [[HeadphoneMotion instance] isHeadphoneMotionAvailable];
    }

    bool areHeadphonesConnected()
    {
        return [[HeadphoneMotion instance] areHeadphonesConnected];
    }

    void startTracking()
    {
        [[HeadphoneMotion instance] startTracking];
    }

    void stopTracking()
    {
        [[HeadphoneMotion instance] stopTracking];
    }

    void setHeadphoneConnectionDelegate(HeadphoneConnectionDelegateCallback callback) {
        if (!_headphoneConnectionDelegate) {
            _headphoneConnectionDelegate = [[HeadphoneConnectionDelegateUnityBridge alloc] init];
        }
        
        [[HeadphoneMotion instance] setHeadphoneConnectionDelegate:_headphoneConnectionDelegate];

        headphoneConnectionDelegate = callback;
    }

    void setRotationDelegate(RotationDelegateCallback callback) {
        if (!_rotationDelegate) {
            _rotationDelegate = [[RotationDelegateUnityBridge alloc] init];
        }
        
        [[HeadphoneMotion instance] setRotationDelegate:_rotationDelegate];

        rotationDelegate = callback;
    }

    // Pass through delegate callbacks
    @implementation RotationDelegateUnityBridge
        -(void) rotationUpdated: (double)x y:(double)y z:(double)z w:(double)w {
            if (rotationDelegate != NULL) {
                rotationDelegate(x, y, z, w);
            }
        }
    @end

    @implementation HeadphoneConnectionDelegateUnityBridge
        -(void) connectionStatusChanged: (bool)connected {
            if (headphoneConnectionDelegate != NULL) {
                headphoneConnectionDelegate(connected);
            }
        }
    @end
    // End pass through delegate callbacks
}
// End expose methods to Unity

