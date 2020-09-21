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

using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace HearXR
{
    /// <summary>
    /// C# wrapper for the HeadphoneMotion native class.
    /// </summary>
    public class HeadphoneMotion : MonoBehaviour
    {
        #region Delegates
        /// <summary>
        /// Head rotation data from the headphone motion manager API.
        /// X, Y, Z, and W correspond to the CMQuaternion values.
        /// See https://developer.apple.com/documentation/coremotion/cmquaternion
        /// </summary>
        public delegate void HeadRotationAction(double x, double y, double z, double w);
        
        /// <summary>
        /// Headphone connection status change.
        /// </summary>
        /// <param name="connected">TRUE if headphones were connected, FALSE otherwise.</param>
        public delegate void HeadphoneConnectionAction(bool connected);
        #endregion
        
        #region Events
        /// <summary>
        /// Head rotation data is available. Passes in raw CMQuaternion x, y, z, w values.
        /// </summary>
        public static HeadRotationAction OnHeadRotationRaw;
        
        /// <summary>
        /// Head rotation data is available. Passes in a Quaternion ready to use for rotation.
        /// </summary>
        public static event Action<Quaternion> OnHeadRotationQuaternion;
        
        /// <summary>
        /// Headphones connection status was changed.
        /// </summary>
        public static HeadphoneConnectionAction OnHeadphoneConnectionChanged;
        #endregion

        #region Public Methods
        /// <summary>
        /// Initialize the Headphone Motion API.
        /// This method must be called before any others.
        /// </summary>
        public static void Init()
        {
    #if UNITY_IOS && !UNITY_EDITOR
            setHeadphoneConnectionDelegate(HeadphoneConnectionChanged);
            setRotationDelegate(RotationUpdated);
    #else
            DisplayPlatformError();
    #endif
        }
        
        /// <summary>
        /// Start listening for headphone connection events and tracking headphone motion.
        /// You must call Init first, and it is advisable to check IsHeadphoneMotionAvailable() before calling this method.
        /// </summary>
        public static void StartTracking()
        {
    #if UNITY_IOS && !UNITY_EDITOR
            startTracking();
    #else
            DisplayPlatformError();
    #endif
        }
        
        /// <summary>
        /// Stop listening for headphone connection events and tracking headphone motion.
        /// </summary>
        public static void StopTracking()
        {
    #if UNITY_IOS && !UNITY_EDITOR
            stopTracking();
    #else
            DisplayPlatformError();
    #endif
        }
        
        /// <summary>
        /// Check if headphone motion API is available.
        /// </summary>
        /// <returns>TRUE is headphone motion API is available, FALSE otherwise.</returns>
        public static bool IsHeadphoneMotionAvailable()
        {
    #if UNITY_IOS && !UNITY_EDITOR
            return isHeadphoneMotionAvailable();
    #else
            DisplayPlatformError();
            return false;
    #endif
        }
        
        /// <summary>
        /// Check if headphones are connected.
        /// Alternatively, you can subscribe to the OnHeadphoneConnectionChanged() event to be notified when the headphones are
        /// connected or disconnected. 
        /// </summary>
        /// <returns>TRUE if headphones are connected, FALSE otherwise.</returns>
        public static bool AreHeadphonesConnected()
        {
    #if UNITY_IOS && !UNITY_EDITOR
            return areHeadphonesConnected();
    #else
            DisplayPlatformError();
            return false;
    #endif
        }
        #endregion

        #region Private Methods
        private static void DisplayPlatformError()
        {
            Debug.LogWarning("HeadphoneMotion is only available on iOS platform and only at runtime.");
        }
        #endregion
        
        #region Import native class methods
        [DllImport ("__Internal")]
        private static extern bool isHeadphoneMotionAvailable();
        
        [DllImport ("__Internal")]
        private static extern bool areHeadphonesConnected();
        
        [DllImport("__Internal")]
        private static extern bool startTracking();
        
        [DllImport("__Internal")]
        private static extern bool stopTracking();
        
        [DllImport("__Internal")]
        private static extern void setHeadphoneConnectionDelegate(HeadphoneConnectionAction callback);
        
        [DllImport("__Internal")]
        private static extern void setRotationDelegate(HeadRotationAction callback);
        #endregion
        
        #region Handle native class callbacks
        [MonoPInvokeCallback(typeof(HeadphoneConnectionAction))] 
        private static void HeadphoneConnectionChanged(bool connected) {
            OnHeadphoneConnectionChanged?.Invoke(connected);
        }

        [MonoPInvokeCallback(typeof(HeadRotationAction))] 
        private static void RotationUpdated(double x, double y, double z, double w) {
            OnHeadRotationRaw?.Invoke(x, y, z, w);
            OnHeadRotationQuaternion?.Invoke(new Quaternion((float) x, (float) z, (float) y, (float) -w));
        }
        #endregion
    }
}
