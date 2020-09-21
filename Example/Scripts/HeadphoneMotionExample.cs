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
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace HearXR
{
    /// <summary>
    /// Example of using HeadphoneMotion.
    /// </summary>
    public class HeadphoneMotionExample : MonoBehaviour
    {
        #region Editor Fields
        [SerializeField] private Transform _rotateTarget = default;
        [SerializeField] private Text _motionAvailabilityText = default;
        [SerializeField] private Text _headphoneConnectionStatusText = default;
        [SerializeField] private Button _toggleTrackingButton = default;
        [SerializeField] private Button _calibrateStartingRotationButton = default;
        [SerializeField] private Button _resetCalibrationButton = default;
        #endregion
        
        #region Private Fields
        private bool _motionAvailable;
        private bool _tracking;
        private bool _headphonesConnected;
        private Text _trackingButtonText;
        private Quaternion _lastRotation = Quaternion.identity;
        private Quaternion _calibratedOffset = Quaternion.identity;
        #endregion

        #region Init
        private void Start()
        {
            // Add event listeners to buttons and hide buttons as needed.
            _toggleTrackingButton.onClick.AddListener(ToggleTracking);
            _trackingButtonText = _toggleTrackingButton.GetComponentInChildren<Text>();
            
            _calibrateStartingRotationButton.onClick.AddListener(CalibrateStartingRotation);
            _resetCalibrationButton.onClick.AddListener(ResetCalibration);
            UpdateRotationOffsetButtons();
            
            // Init HeadphoneMotion. Always call this first.
            HeadphoneMotion.Init();

            // Check if headphone motion is available on this device.
            _motionAvailable = HeadphoneMotion.IsHeadphoneMotionAvailable();
            _motionAvailabilityText.text =
                (_motionAvailable) ? "Headphone motion is available" : "Headphone motion is not available";
            
            if (_motionAvailable)
            {
                // Set headphones connected text to false to start with.
                HandleHeadphoneConnectionChange(false);
                
                // Subscribe to events before starting tracking, or will miss the initial headphones connected callback.
                // Subscribe to the headphones connected/disconnected event.
                HeadphoneMotion.OnHeadphoneConnectionChanged += HandleHeadphoneConnectionChange;
                
                // Subscribe to the rotation callback.
                HeadphoneMotion.OnHeadRotationQuaternion += HandleHeadRotationQuaternion;
                
                // Start tracking headphone motion.
                HeadphoneMotion.StartTracking();
                _tracking = true;
            }
            
            UpdateTrackingButton();
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Headphone connection status was changed (callback for OnHeadphoneConnectionChanged()).
        /// </summary>
        /// <param name="connected">TRUE if connected, FALSE otherwise.</param>
        private void HandleHeadphoneConnectionChange(bool connected)
        {
            _headphonesConnected = connected;
            _headphoneConnectionStatusText.text =
                (_headphonesConnected) ? "Headphones are connected" : "Headphones are not connected";
            
            UpdateRotationOffsetButtons();
        }
        
        /// <summary>
        /// Receive headphone as quaternion (callback for OnHeadRotationQuaternion()).
        /// </summary>
        /// <param name="rotation">Headphone rotation</param>
        private void HandleHeadRotationQuaternion(Quaternion rotation)
        {
            // Match the target object's rotation to the headphone rotation.
            if (_calibratedOffset == Quaternion.identity)
            {
                _rotateTarget.rotation = rotation;
            }
            else
            {
                _rotateTarget.rotation = rotation * Quaternion.Inverse(_calibratedOffset);
            }
            
            _lastRotation = rotation;
        }
        #endregion
        
        #region Private Methods
        private void ToggleTracking()
        {
            _tracking = !_tracking;

            if (_motionAvailable && _tracking)
            {
                HeadphoneMotion.StartTracking(); 
            }
            else
            {
                HeadphoneMotion.StopTracking();
            }

            UpdateTrackingButton();
        }
        
        private void CalibrateStartingRotation()
        {
            _calibratedOffset = _lastRotation;
            UpdateRotationOffsetButtons();
        }

        private void ResetCalibration()
        {
            _calibratedOffset = Quaternion.identity;
            UpdateRotationOffsetButtons();
        }
        
        private void UpdateTrackingButton()
        {
            if (!_motionAvailable)
            {
                _toggleTrackingButton.gameObject.SetActive(false);
                return;
            }
            
            _trackingButtonText.text = (_tracking) ? "Disable tracking" : "Enable tracking";
        }

        private void UpdateRotationOffsetButtons()
        {
            _calibrateStartingRotationButton.gameObject.SetActive(_tracking && _headphonesConnected);
            _resetCalibrationButton.gameObject.SetActive(_tracking && _headphonesConnected && _calibratedOffset != Quaternion.identity);
        }
        #endregion
    }
}
