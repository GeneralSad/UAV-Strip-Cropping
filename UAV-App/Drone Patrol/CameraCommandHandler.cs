using DJI.WindowsSDK;
using DJI.WindowsSDK.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UAV_App.Pages;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using DateTime = System.DateTime;

namespace UAV_App.Drone_Patrol
{
    public class CameraCommandHandler
    {

        private const double defaultPitch = -90;
        private const double defaultSpeed = 1;


        public void cameraOn()
        {

        }
    
        public void cameraOff() 
        {
            
        }

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

        public async void ResetGimbal()
        {
            SetGimbal(defaultPitch);
        }

        public async void TakePhoto()
        {

            SetCameraWorkModeToShootPhoto();

            if (DJISDKManager.Instance.ComponentManager != null)
            {
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
            else
            {
                Debug.WriteLine("SDK hasn't been activated yet.");
            }
        }

        private async void SetCameraWorkModeToShootPhoto()
        {
            SetCameraWorkMode(CameraWorkMode.SHOOT_PHOTO);
        }

        private async void SetCameraWorkMode(CameraWorkMode mode)
        {
            if (DJISDKManager.Instance.ComponentManager != null)
            {
                CameraWorkModeMsg workMode = new CameraWorkModeMsg
                {
                    value = mode,
                };
                var retCode = await DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0).SetCameraWorkModeAsync(workMode);
                if (retCode != SDKError.NO_ERROR)
                {
                    Debug.WriteLine("Set camera work mode to " + mode.ToString() + "failed, result code is " + retCode.ToString());
                }
            }
            else
            {
                Debug.WriteLine("SDK hasn't been activated yet.");
            }
        }

        public void GetPhoto()
        {
            MediaFileListRequest mediaFileListRequest = new MediaFileListRequest
            {
                isAllList = true,
                location = MediaFileListLocation.INTERNAL_STORAGE
                
            };

            List<MediaFileListRequest> list = new List<MediaFileListRequest>
            {
                mediaFileListRequest
            };

            MediaTaskRequest mediaTaskRequest = new MediaTaskRequest()
            {
                type = MediaTaskType.FILE_LIST,
                listReq = list
            };

            MediaTask mediaTask = new MediaTask(mediaTaskRequest);
            mediaTask.OnListReqResponse += OnListResponse;
            mediaTask.OnListReqForward += OnListReqFwd;
            mediaTask.OnRequestTearDown += OnRqTdwn;

            MediaTaskManager mediaTaskManager = new MediaTaskManager(0, 0);
            Debug.WriteLine("Push to front");
            mediaTaskManager.PushFront(mediaTask);
        }

        private void OnRqTdwn(MediaTask sender, SDKError retCode, MediaTaskResponse? response)
        {
            Debug.WriteLine("code " + retCode.ToString() + " : response " + response.Value.fileList.files.Count + " : Sender " + sender.Request.listReq.Count);
        }

        private void OnListReqFwd(MediaTask sender, MediaFileListRequest? request, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public async void TakeScreenshot()
        {
            var _bitmap = new RenderTargetBitmap();
            await _bitmap.RenderAsync(OverlayPage.Current?.GetFeed());

            var savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            savePicker.FileTypeChoices.Add("Image", new List<string>() { ".jpg" });
            savePicker.SuggestedFileName = "Card" + DateTime.Now.ToString("yyyyMMddhhmmss");
            StorageFile savefile = await savePicker.PickSaveFileAsync();

            if (savefile == null)
            {
                return;
            }

            var pixels = await _bitmap.GetPixelsAsync();
            using (IRandomAccessStream stream = await savefile.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await
                BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
                byte[] bytes = pixels.ToArray();
                encoder.SetPixelData(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Ignore,
                    (uint)_bitmap.PixelWidth,
                    (uint)_bitmap.PixelHeight,
                    200,
                    200,
                    bytes
                );

                await encoder.FlushAsync();
            }
        }

        private void OnListResponse(MediaTask sender, List<MediaFile> files)
        {
            Debug.WriteLine($"Response: {files.Count}");
        }
    }
}
