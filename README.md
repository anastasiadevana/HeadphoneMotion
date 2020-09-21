# Headphone Motion Unity Plugin

HeadphoneMotion is a plugin for Unity3d that exposes Apple's Headphone Motion API (<a href="https://developer.apple.com/documentation/coremotion/cmheadphonemotionmanager">CMHeadphoneManager</a>) in Unity.

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

Apple released the new Headphone Motion API in iOS 14, which provides head tracking data from compatible headphones. 
Currently (as of September 2020) the only compatible device is AirPods Pro.

Please note that Apple's Head Motion API only provides rotational data (3dof), but no positional data.

---

## <a name="installation"></a>Installation

**Option 1:** Clone this repository into the `Assets` folder in your Unity project.

**Option 2:** Donwload this repository as a zip, rename the extracted folder to `HeadphoneMotion`, and place it anywhere in your Unity project `Assets` folder.

---

## <a name="example-scene"></a> Running the example scene

In order to verify that the plugin works, it's best to test with the provided example scene.

> Here is what you will need in order to build and run the example scene:
> - Unity3D (tested with version `2019.4.1f1`)
> - Xcode version 12 or higher
> - AirPods Pro running the latest firmware
> - Apple mobile device running iOS 14 or greater
> - Basic knowledge of how to sign and install iOS builds

* Make sure that your project is set to build for iOS. Go to `File > Build Settings...`
    * If `iOS` is not already selected, do select it and click the `Switch Platform` button
* Add the example scene `HeadphoneMotionExample` to the build, and click `Build and Run`
* Name your build folder where the `Xcode` project will be saved, and click the `Save` button. This will start the build process.
* Make sure that your `iOS` device is connected to the computer and unlocked

> If at this point you get a `Code Signing Error`, take care of the signing steps. For the sake of brevity, app signing is outside the scope of these instructions.

On the first build attempt, you will likely get a crash with the following error:

```
[access] This app has crashed because it attempted to access privacy-sensitive data 
without a usage description. The app's Info.plist must contain an NSMotionUsageDescription 
key with a string value explaining to the user how the app uses this data.
```

What this means is that head tracking data requires a special permission from the user, and you, the developer, must provide a reason to the user as to why they will need to grant this permission.

In order to do that, go to the `Info.plist` **in the root folder of the Xcode project**, add a new key titled `NSMotionUsageDescription`, and in the `Value` field enter the permission text.

![Add NSMotionUsageDescription Value Gif](https://i.imgur.com/vZVl0Oe.gif)

* Hit the `Play` button in Xcode to build again, and the project should successfully build and install on your device.
* If you don't already have your AirPods Pro in, put them on now, and you should see the cube rotating to match your headpose.

![Headphone Motion Example App Screenshot](https://i.imgur.com/Z8bRaRZ.png)

### Example App Features

- `Disable Tracking` / `Enable Tracking` - this button toggles the overall Headphone Motion functionality. Note that if the tracking is disabled, you will not receive headphone connection events.
- If your device supports the Headphone Motion API, you will see `Headphone motion is available` (otherwise you will see `not available`)
- `Headphones are connected (or "not connected")` - shows the headphone connection status

#### Dealing With Rotation Offset

You may notice that the "default" rotation of the cube is slightly offset when it starts tracking headpose. This is due to the position of one of the AirPods in your ear. **The best way to fix this is to adjust the AirPod position in your ear.**

You can also try to use the `Calibrate starting rotation` button (click it when you feel that you're looking perfectly straight forward). The cube rotation will be adjusted based on this calibration, but it **will not work well with large offsets**. 

Click the `Reset calibration` button will reset the calibration value.

---

## <a name="how-to-use"></a>How to use

Here is the minimum amount of code you will need to start using this functionality.

Include the `HearXR` namespace by adding the following line of code to the top of your script:

```C#
using HearXR;
```

Place the following code in some instantiation function in your script (for example into Unity's `Start()` call).

```c#

// This call initializes the native plugin.
HeadphoneMotion.Init();

// Check if headphone motion is available on this device.
if (HeadphoneMotion.IsHeadphoneMotionAvailable())
{
    // Subscribe to the rotation callback.
    // Alternatively, you can subscribe to OnHeadRotationRaw event to get the 
    // x, y, z, w values as they come from the API.
    HeadphoneMotion.OnHeadRotationQuaternion += HandleHeadRotationQuaternion;
    
    // Start tracking headphone motion.
    HeadphoneMotion.StartTracking();
}

```

In the same script, place the function below, which is the handler function that receives the headpose rotation data.
In this exaple, the received data will be applied to the `GameObject` that this script is attached to.

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
   - Apple's Headphone Motion API only provides 3dof information
    
- **Why does the rotation seem "crooked" when my head is perfectly straight?**
   - This is likely due to the position of one of the AirPod Pros in your ear. Only one headphone is being used for the rotation data. Just wiggle each one to figure out which one is currently being used, and then adjust it until your target object appears straight when your head is straight.
    
---

## License

[![License](http://img.shields.io/:license-mit-blue.svg?style=flat-square)](http://badges.mit-license.org)


- **[MIT license](http://opensource.org/licenses/mit-license.php)**
- Copyright 2020 © Anastasia Devana
