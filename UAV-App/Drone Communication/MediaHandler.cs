using DJI.WindowsSDK;
using DJI.WindowsSDK.Components;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace UAV_App.Drone_Communication
{
    internal class MediaHandler
    {
        //Counter to keep track with how many images have to be downloaded
        //When 0 is reached the camera is set to SHOOT_PHOTO
        private int filesRemaining = 0;

        public ObservableCollection<MediaFile> files = new ObservableCollection<MediaFile>();
        private readonly CameraHandler cameraHandler = DJISDKManager.Instance.ComponentManager.GetCameraHandler(0, 0);
        private readonly MediaTaskManager taskManager = new MediaTaskManager(0, 0);

        private Dictionary<string, LocationCoordinate2D> locationDictionary = new Dictionary<string, LocationCoordinate2D>();
        private Queue<LocationCoordinate2D> locationQueue = new Queue<LocationCoordinate2D>();

        //Boolean to see if all files are being downloaded, or just one
        private bool isDownloadAll = false;

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

        //Request list of downloadable items from drone.
        //Wait for response, then download most recent files
        public async void DownloadMostRecentPhoto(LocationCoordinate2D location)
        {
            locationQueue.Enqueue(location);
            isDownloadAll = false;
            await LoadFiles(MediaFileListLocation.SD_CARD);
        }

        //Get the last item in the list and return the location
        public LocationCoordinate2D GetMostRecentPhotoLocation()
        {
            MediaFile file = files.Last();

            LocationCoordinate2D location;
            locationDictionary.TryGetValue(file.fileName, out location);
            return location;
        }

        //Get location from filename
        public LocationCoordinate2D GetLocationFromFileName(string fileName)
        {
            LocationCoordinate2D location;
            locationDictionary.TryGetValue(fileName, out location);
            return location;
        }

        //Request list of downloadable items from drone.
        //Wait for response, then download all files
        public async void DownloadAllPhotos()
        {
            isDownloadAll = true;
            await LoadFiles(MediaFileListLocation.SD_CARD);
        }

        //Clear all images to limit disk usage
        //Needs to be called a the start of every missions
        public async void ClearImageCache()
        {
            StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Pictures", CreationCollisionOption.OpenIfExists);
            var files = await storageFolder.GetFilesAsync();
            foreach (var file in files)
            {
                _ = file.DeleteAsync();
            };
        }

        //Download all files from the list
        private async Task DownloadAllFilesAsync()
        {
            isDownloadAll = true;
            foreach (var file in files)
            {
                //Check file type so no videos will be downloaded
                //This will make the downloading of all files a lot faster
                if (file.fileType == MediaFileType.JPEG)
                {
                    filesRemaining++;
                    await DownloadSingle(file);
                }
            }
        }

        //Download most recent file
        private async Task DownloadRecentFileAsync()
        {
            //We are not downloading all files, just one
            isDownloadAll = false;

            //Add 1 to the files remaining
            filesRemaining++;

            //Get the last item in the list and download it
            MediaFile file = files.Last();
            await DownloadSingle(file);
        }

        //Request files from drone
        private async Task<bool> LoadFiles(MediaFileListLocation fileLocation)
        {
            var result = await cameraHandler.GetCameraWorkModeAsync();

            //Check if the drone is connected
            //If the drone is not connected or if there is an error it will return null
            if (result.value == null)
            {
                Debug.WriteLine("No result from workmode");
                return false;
            }

            //Check if the camera is in the correct workmode
            var mode = result.value?.value;
            if (mode != CameraWorkMode.TRANSCODE && mode != CameraWorkMode.PLAYBACK)
            {
                Debug.WriteLine("Set camera workmode");
                await SetCameraWorkMode(CameraWorkMode.TRANSCODE);
            }

            //Clear all files from last load request
            this.files.Clear();

            var fileListTask = MediaTask.FromRequest(new MediaFileListRequest
            {
                count = -1,
                index = 1,
                subType = MediaRequestType.ORIGIN,
                isAllList = true,
                location = fileLocation,
            });

            //Gets called when there is a response
            fileListTask.OnListReqResponse += (fileSender, files) =>
            {
                //Add all files to the file list
                files.ForEach(obj => {
                    locationDictionary.Add(obj.fileName, locationQueue.Dequeue());
                    this.files.Add(obj);
                });

                //If the list is not empty download the files
                //Go to the correct method based on the download mode
                if (files.Count != 0)
                {
                    if (isDownloadAll)
                    {
                        DownloadAllFilesAsync();
                    }
                    else
                    {
                        DownloadRecentFileAsync();
                    }
                }
            };

            //Gets called when the task is done
            fileListTask.OnRequestTearDown += (fileSender, retCode, response) =>
            {
                if (retCode == SDKError.NO_ERROR)
                {
                    return;
                }
                Debug.WriteLine(String.Format("LaunchFileDataTask get files : {0}. Switch Mode or try again", retCode));
            };

            taskManager.PushBack(fileListTask);

            return true;
        }

        //Download a single file
        private async Task DownloadSingle(MediaFile file)
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

            //Create folder and file for downloading
            StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Pictures", CreationCollisionOption.OpenIfExists);
            var storageFile = await storageFolder.CreateFileAsync(file.fileName, CreationCollisionOption.GenerateUniqueName);
            var stream = await storageFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
            var outputStream = stream.GetOutputStreamAt(0);
            var fileWriter = new DataWriter(outputStream);
            task.OnDataReqResponse += async (sender, req, data, speed) =>
            {
                //Write to file and close writer when done
                fileWriter.WriteBytes(data);
                await fileWriter.StoreAsync();
                await outputStream.FlushAsync();
            };

            //Gets called when the file is done downloading
            task.OnRequestTearDown += (sender, retCode, res) =>
            {
                filesRemaining--;
                //Set camera work mode to SHOOT_PHOTO when finished with downloading
                if (filesRemaining == 0)
                {
                    //Debug.WriteLine("Remaining: FINISHED");
                    _ = SetCameraWorkMode(CameraWorkMode.SHOOT_PHOTO);
                }
                else
                {
                    //Debug.WriteLine("Remaining:" + filesRemaining);
                }
            };

            taskManager.PushBack(task);
        }

        //Take a screenshot of the live feed
        //Faster method of getting photos, resolution is lower than downloading
        public async void TakeScreenshot(SwapChainPanel swapChainPanel)
        {
            var _bitmap = new RenderTargetBitmap();
            await _bitmap.RenderAsync(swapChainPanel);

            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.Downloads
            };
            savePicker.FileTypeChoices.Add("Image", new List<string>() { ".jpg" });
            savePicker.SuggestedFileName = "Card" + System.DateTime.Now.ToString("yyyyMMddhhmmss");
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
