﻿// <copyright file="DisplayRequest.Android.cs" company="James Croft">
// Copyright (c) James Croft. All rights reserved.
// </copyright>

#if __ANDROID__
namespace XPlat.Device.Display
{
    using System;

    using Android.Views;

    /// <summary>Represents a display request.</summary>
    public class DisplayRequest : IDisplayRequest
    {
        private readonly Window currentWindow;

        /// <summary>Initializes a new instance of the <see cref="DisplayRequest"/> class.</summary>
        public DisplayRequest(Window currentWindow)
        {
            this.currentWindow = currentWindow;
        }

        /// <summary>Activates a display request.</summary>
        public void RequestActive()
        {
            try
            {
                this.currentWindow?.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("DisplayRequest requires the 'android.permission.WAKE_LOCK' to be enabled to request active.", ex);
            }
        }

        /// <summary>Deactivates a display request.</summary>
        public void RequestRelease()
        {
            try
            {
                this.currentWindow?.ClearFlags(WindowManagerFlags.KeepScreenOn);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("DisplayRequest requires the 'android.permission.WAKE_LOCK' to be enabled to request release.", ex);
            }
        }
    }
}
#endif