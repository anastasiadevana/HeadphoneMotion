# Headphone Motion

HeadphoneMotion is a plugin for Unity3d that exposes Apple's Headphone Motion API (<a href="https://developer.apple.com/documentation/coremotion/cmheadphonemotionmanager">CMHeadphoneManager</a>) to Unity.
You can use this to get head tracking data from Apple headphones like AirPods Pro into your Unity scenes.

![Headphone Motion Example Gif](https://i.imgur.com/dZaAM2G.gif)

## Table of Contents

- [What is this?](#what-is-this)
- [Installation](#installation)
- [Running the example scene](#example-scene)
- [How to use](#how-to-use)
- [FAQ](#faq)
- [License](#license)

---

## <a name="what-is-this"></a>What is this?

Apple released the new Headphone Motion API in iOS 14, which provides head tracking data from compatible headphones. At this point the only compatible device is AirPods Pro.
Please note that the data is only 3dof (degrees of freedom), meaning that it only provides the head rotation, but not position.

---

## <a name="installation"></a>Installation

1. Clone this repository to your desktop, or download it as a zip.
1. Put the downloaded `HeadphoneMotion` folder anywhere in your Unity `Assets` folder.

---

## <a name="example-scene"></a> Running the example scene

In order to verify that the plugin works, it's best to test with the provided example scene.

> What you'll need: 
> - Unity3D
> - Xcode version 12 or higher
> - AirPods Pro running the latest hardware
> - Apple mobile device running iOS 14
> - and the ability to sign and install iOS builds (I have confirmed that this functionality works without an Apple Developer account)

* Make sure that your project is set to build for iOS. Go to `File > Build Settings...`
    * If `iOS` is not already selected, do select it and click the `Switch Platform` button
* Add the example scene `HeadphoneMotionExample` to the build, and click `Build and Run`
* If you haven't already, name your build folder where the `Xcode` project will be saved
* Make sure your `iOS` device is connected to the computer and unlocked

> If at this point you get a `Code Signing Error`, take care of the signing steps. For the sake of brevity, app signing is outside the scope of these instructions.

On the first build attempt, you will likely get a crash with the following error:

```
[access] This app has crashed because it attempted to access privacy-sensitive data without a usage description. 
The app's Info.plist must contain an NSMotionUsageDescription key with a string value explaining to the user how the app uses this data.
```
In order to fix this, add the `NSMotionUsageDescription` key to the `Info.plist` **in the root folder of the Xcode project** (there may be other `Info.plist` files in the project, but this is the one you want).

![Add NSMotionUsageDescription Value Gif](https://i.imgur.com/vZVl0Oe.gif)

* Hit the `Play` button in Xcode to build again, and the project should successfully build and install on your device.
* If you don't already have your AirPods Pro in, put them on now, and you should see the cube rotating to match your headpose.

![Headphone Motion Example App Screenshot](https://i.imgur.com/Z8bRaRZ.png)

##### Example App Features

- `Disable Tracking` / `Enable Tracking` - this button toggles the overall Headphone Motion functionality. Note that if the tracking is disabled, you will not receive headphone connection events.
- If your device supports the Headphone Motion API, you will see `Headphone motion is available` (otherwise you will see `not available`)
- `Headphones are connected (or "not connected")` - shows the headphone connection status

**Rotation Calibration**
You may notice that the "default" rotation of the cube is slightly offset when it starts tracking headpose. This is due to the position of one of the AirPods in your ear. The best way to fix this is to adjust the AirPod position. 
You can also try to use the `Calibrate starting rotation` button (click it when you feel that you're looking perfectly straight forward). It will try to adjust the rotation based on this calibration, but it will not work well with large offsets. 
Click the `Reset calibration` button will reset the calibration value.

---

## <a name="how-to-use"></a>How to use

This is the minimum amount of code you need to start using this functionality.

Place this code in some Init function in your script (for example Unity's `Start()` call).

```c#

// This call initializes the native plugin.
HeadphoneMotion.Init();

// Check if headphone motion is available on this device.
if (HeadphoneMotion.IsHeadphoneMotionAvailable())
{
    // Subscribe to the rotation callback.
    // You can also subsribe to OnHeadRotationRaw event to get the x, y, z, w values as they come from the API.
    HeadphoneMotion.OnHeadRotationQuaternion += HandleHeadRotationQuaternion;
    
    // Start tracking headphone motion.
    HeadphoneMotion.StartTracking();
}

```

In the same file create the handler function to receive rotation data.

```c#
private void HandleHeadRotationQuaternion(Quaternion rotation)
{
    transform.rotation = rotation;
}
```

Also examine the example scene for some additional usage examples.

---

## <a name="faq"></a>FAQ

- **Is this 3dof or 6dof?**
    - The API only provides 3dof information
    
---

## License

[![License](http://img.shields.io/:license-mit-blue.svg?style=flat-square)](http://badges.mit-license.org)


- **[MIT license](http://opensource.org/licenses/mit-license.php)**
- Copyright 2020 © Anastasia Devana
