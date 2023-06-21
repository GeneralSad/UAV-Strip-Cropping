using DJI.WindowsSDK;
using DJI.WindowsSDK.Components;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DateTime = System.DateTime;

namespace UAV_App.Drone_Patrol
{
    public class CameraCommandHandler
    {

        //Default values for camera
        private const double defaultPitch = -90;
        private const double defaultSpeed = 1;

        //Amount of seconds to wait on the movement of the gimval
        private const double gimbalTimeout = 1.5;

        //Amount of degrees of accuracy for the gimbal to take a photo
        private const double gimbalAccuracy = 2.5;

        //Make the gimbal rotate a certain amount of degrees
        public async void SetGimbal(double pitch, double speed = defaultSpeed)
        {
            GimbalHandler gimbalHandler = DJISDKManager.Instance.ComponentManager.GetGimbalHandler(0, 0);

            GimbalAngleRotation gimbalAngleRotation = new GimbalAngleRotation
            {
                mode = GimbalAngleRotationMode.ABSOLUTE_ANGLE,
                pitch = pitch,
                pitchIgnored = false,
                duration = speed
            };

            await gimbalHandler.RotateByAngleAsync(gimbalAngleRotation);
        }

        //Get the pitch value of the gimbal
        public async Task<double> GetGimbalPitch()
        {
            GimbalHandler gimbalHandler = DJISDKManager.Instance.ComponentManager.GetGimbalHandler(0, 0);
            var attitude = await gimbalHandler.GetGimbalAttitudeAsync();
            if (attitude.value.HasValue) return attitude.value.Value.pitch;
            else return 0;
        }

        //Set gimbal to default gimbal postion
        public void ResetGimbal()
        {
            SetGimbal(defaultPitch);
        }

        //Reset camera settings and rotate gimbal to default position
        public async Task ResetCamera()
        {
            await SetCameraWorkMode(CameraWorkMode.SHOOT_PHOTO);

            PhotoRatioMsg photoRatioMsg = new PhotoRatioMsg
            {
                value = PhotoRatio.RATIO_16COLON9
            };

            CameraISOMsg cameraISOMsg = new CameraISOMsg
            {
                value = CameraISO.ISO_AUTO
            };

            PhotoStorageFormatMsg photoStorageFormatMsg = new PhotoStorageFormatMsg
            {
                value = PhotoStorageFormat.JPEG
            };

            CameraStorageLocationMsg cameraStorageLocationMsg = new CameraStorageLocationMsg
            {
                value = CameraStorageLocation.SDCARD
            };

            await DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0).ResetCameraSettingAsync();
            await DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0).SetPhotoRatioAsync(photoRatioMsg);
            await DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0).SetISOAsync(cameraISOMsg);
            await DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0).SetPhotoStorageFormatAsync(photoStorageFormatMsg);
            await DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0).SetCameraStorageLocationAsync(cameraStorageLocationMsg);

            ResetGimbal();

            double gimbalPitch = await GetGimbalPitch();

            DateTime time = DateTime.Now;
            while (Math.Abs(gimbalPitch - defaultPitch) > gimbalAccuracy)
            {
                if (DateTime.Now >= time.AddSeconds(gimbalTimeout))
                {
                    Debug.WriteLine("Gimbal move timeout");
                    return;
                }
                gimbalPitch = await GetGimbalPitch();
            }
        }

        //Make the drone take a photo
        public async void TakePhoto()
        {
            await ResetCamera();

            var retCode = await DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0).StartShootPhotoAsync();
            if (retCode != SDKError.NO_ERROR)
            {
                Debug.WriteLine("Failed to shoot photo, result code is " + retCode.ToString());
            }
            else
            {
                Debug.WriteLine("Shoot photo successfully");
            }

        }

        //Set the camera to the given work mode
        private async Task<bool> SetCameraWorkMode(CameraWorkMode mode)
        {
            CameraWorkModeMsg workMode = new CameraWorkModeMsg
            {
                value = mode,
            };
            var retCode = await DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0).SetCameraWorkModeAsync(workMode);
            if (retCode != SDKError.NO_ERROR)
            {
                Debug.WriteLine("Set camera work mode to " + mode.ToString() + "failed, result code is " + retCode.ToString());
                return false;
            }
            return true;
        }

    }
}
