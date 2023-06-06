using DJI.WindowsSDK;
using DJI.WindowsSDK.Components;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using DateTime = System.DateTime;

namespace UAV_App.Drone_Patrol
{
    public class CameraCommandHandler
    {

        private const double defaultPitch = -90;
        private const double defaultSpeed = 1;

        private const double downloadTimeout = 1;
        private const double gimbalTimeout = 1.5;

        private const double gimbalAccuracy = 2.5;


        public ObservableCollection<MediaFile> files = new ObservableCollection<MediaFile>();
        private readonly CameraHandler cameraHandler = DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0);
        private readonly MediaTaskManager taskManager = new MediaTaskManager(0, 0);

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

        public async Task<double> GetGimbal()
        {
            GimbalHandler gimbalHandler = DJISDKManager.Instance.ComponentManager.GetGimbalHandler(0, 0);
            var attitude = await gimbalHandler.GetGimbalAttitudeAsync();
            return attitude.value.Value.pitch;
        }

        public void ResetGimbal()
        {
            SetGimbal(defaultPitch);
        }

        public async void TakePhoto()
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

            double gimbalPitch = await GetGimbal();

            DateTime time = DateTime.Now;
            while (gimbalPitch - defaultPitch > gimbalAccuracy)
            {
                if (DateTime.Now >= time.AddSeconds(gimbalTimeout))
                {
                    Debug.WriteLine("Gimbal move timeout");
                    return;
                }
                gimbalPitch = await GetGimbal();
                Debug.WriteLine(gimbalPitch - defaultPitch);
            }

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

        private async Task<bool> SetCameraWorkMode(CameraWorkMode mode)
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
                    return false;
                }
                return true;
            }
            else
            {
                Debug.WriteLine("SDK hasn't been activated yet.");
                return false;
            }
        }

        public async void GetMostRecentPhoto()
        {
            await LoadFiles(MediaFileListLocation.SD_CARD);

            DateTime time = DateTime.Now;
            while (files.Count == 0)
            {
                if (DateTime.Now >= time.AddSeconds(downloadTimeout))
                {
                    Debug.WriteLine("Photo load timeout");
                    return;
                }
            }

            DownloadRecentFile();
        }

        public async void GetPhotos()
        {
            await LoadFiles(MediaFileListLocation.SD_CARD);

            DateTime time = DateTime.Now;
            while (files.Count == 0)
            {
                if (DateTime.Now >= time.AddSeconds(downloadTimeout))
                {
                    Debug.WriteLine("Photo load timeout");
                    return;
                }
            }

            DownloadAllFiles();
        }

        private void DownloadAllFiles()
        {
            foreach (var file in files)
            {
                this.DownloadSingle(file);
            }
        }

        private void DownloadRecentFile()
        {
            MediaFile file = files.Last();
            this.DownloadSingle(file);
        }

        private async Task<bool> LoadFiles(MediaFileListLocation fileLocation)
        {
            var result = await cameraHandler.GetCameraWorkModeAsync();

            if (result.value == null)
            {
                Debug.WriteLine("No result from workmode");
                return false;
            }

            var mode = result.value?.value;
            if (mode != CameraWorkMode.TRANSCODE && mode != CameraWorkMode.PLAYBACK)
            {
                Debug.WriteLine("Set mode");
                await SetCameraWorkMode(CameraWorkMode.TRANSCODE);
            }

            this.files.Clear();

            var fileListTask = MediaTask.FromRequest(new MediaFileListRequest
            {
                count = -1,
                index = 1,
                subType = MediaRequestType.ORIGIN,
                isAllList = true,
                location = fileLocation,
            });

            fileListTask.OnListReqResponse += (fileSender, files) =>
            {
                files.ForEach(obj => this.files.Add(obj));
            };

            fileListTask.OnRequestTearDown += (fileSender, retCode, response) =>
            {
                if (retCode == SDKError.NO_ERROR)
                {
                    return;
                }
            };

            taskManager.PushBack(fileListTask);

            return true;
        }

        private async void DownloadSingle(MediaFile file)
        {
            var request = new MediaFileDownloadRequest
            {
                index = file.fileIndex,
                count = 1,
                dataSize = -1,
                offSet = 0,
                segSubIndex = 0,
                subIndex = 0,
                type = MediaRequestType.ORIGIN
            };

            var task = MediaTask.FromRequest(request);
            var storageFile = await DownloadsFolder.CreateFileAsync(file.fileName, CreationCollisionOption.GenerateUniqueName);
            var stream = await storageFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
            var outputStream = stream.GetOutputStreamAt(0);
            var fileWriter = new DataWriter(outputStream);
            task.OnDataReqResponse += async (sender, req, data, speed) =>
            {
                fileWriter.WriteBytes(data);
                await fileWriter.StoreAsync();
                await outputStream.FlushAsync();
            };

            task.OnRequestTearDown += (sender, retCode, res) =>
            {
            };

            taskManager.PushBack(task);
        }

        public async void TakeScreenshot()
        {
            var _bitmap = new RenderTargetBitmap();
            await _bitmap.RenderAsync(OverlayPage.Current?.GetFeed());

            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.Downloads
            };
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
    }
}
