using DJI.WindowsSDK;
using DJI.WindowsSDK.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UAV_App.Drone_Patrol
{
    public class CameraCommandHandler
    {
        public void cameraOn()
        {

        }
    
        public void cameraOff() 
        {
            
        }

        public async void setGimbal()
        {
            Debug.WriteLine("Gimbal");
            GimbalHandler gimbalHandler = DJISDKManager.Instance.ComponentManager.GetGimbalHandler(0, 0);

            //var hecc = await gimbalHandler.GetGimbalAttitudeRangeAsync();
            //Debug.WriteLine(hecc.value.Value.pitch.min + " : " + hecc.value.Value.pitch.max);

            GimbalAngleRotation gimbalAngleRotation = new GimbalAngleRotation();

            gimbalAngleRotation.mode = GimbalAngleRotationMode.ABSOLUTE_ANGLE;

            gimbalAngleRotation.pitch = -45;
            gimbalAngleRotation.roll = 0;
            gimbalAngleRotation.yaw = 0;

            gimbalAngleRotation.pitchIgnored = false;
            gimbalAngleRotation.rollIgnored = false;
            gimbalAngleRotation.yawIgnored = false;

            gimbalAngleRotation.duration = 1;

            await gimbalHandler.RotateByAngleAsync(gimbalAngleRotation);

            Thread.Sleep(5000);

            gimbalAngleRotation.pitch = 0;
            await gimbalHandler.RotateByAngleAsync(gimbalAngleRotation);


        }
    }
}
