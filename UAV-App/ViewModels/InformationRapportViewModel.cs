using DJIUWPSample.Commands;
using DJIUWPSample.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UAV_App.Models;
using System.Text.Json;
using System.IO;
using DJI.WindowsSDK;
using Windows.Storage.Streams;
using Windows.Storage;
using System.Reflection.PortableExecutable;
using Windows.UI.Xaml.Shapes;
using Windows.System;

namespace UAV_App.ViewModels
{
    internal class InformationRapportViewModel : ViewModelBase
    {

        private string filename;
        private IRandomAccessStream stream;
        private IOutputStream outputStream;
        private IInputStream inputStream;
        private DataWriter fileWriter;
        private DataReader fileReader;
        private StorageFolder storageFolder;

        //Singleton to acces public viewmodel
        private static readonly InformationRapportViewModel _singleton = new InformationRapportViewModel();
        public static InformationRapportViewModel Instance
        {
            get
            {
                return _singleton;
            }
        }

        //Empty, can't add listeners when SDK hasn't started yet.
        private InformationRapportViewModel()
        {
        }

        //Create a new folder if it does not exist
        //Create new file and create streams to read/write the file
        public async Task NewInformationRapportAsync()
        {
            filename = System.DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss");
            storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Rapports", CreationCollisionOption.OpenIfExists);
            StorageFile storageFile = await storageFolder.CreateFileAsync(filename + ".json", CreationCollisionOption.GenerateUniqueName);
            stream = await storageFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
            await OpenFileAsync();
        }

        //Create streams to read/write file
        private async Task OpenFileAsync()
        {
            outputStream = stream.GetOutputStreamAt(0);
            fileWriter = new DataWriter(outputStream);
            inputStream = stream.GetInputStreamAt(0);
            fileReader = new DataReader(inputStream);
            await fileReader.LoadAsync((uint)stream.Size);
        }

        //Add entry to rapport
        //Deserialize file, add item and serialize
        private async Task AddRapportEntryAsync(InformationRapportModel informationRapport)
        {
            if (stream == null) {
                await NewInformationRapportAsync();
            }

            await OpenFileAsync();
            string received = fileReader.ReadString((uint)stream.Size);

            List<InformationRapportModel> rapport;

            if (!string.IsNullOrEmpty(received))
            {
                rapport = JsonSerializer.Deserialize<List<InformationRapportModel>>(received);
            } else
            {
                rapport = new List<InformationRapportModel>();
            }

            rapport.Add(informationRapport);

            JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(rapport, options);
            fileWriter.WriteString(jsonString);

            await fileWriter.StoreAsync();
            await outputStream.FlushAsync();
        }

        //Get all rapports in the rapports folder
        public async Task<IEnumerable<StorageFile>> GetRapportsAsync()
        {
            var files = await storageFolder.GetFilesAsync();
            var jsonfiles = files.Where( file => file.FileType == ".json");

            return jsonfiles;
        }

        //Get last rapport by looking at creation date
        public async Task<StorageFile> GetLastRapportAsync()
        {
            storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Rapports", CreationCollisionOption.OpenIfExists);
            var jsonFiles = await GetRapportsAsync();

            if (!jsonFiles.Any()) { return null; }

            StorageFile newestFile = jsonFiles.First();

            foreach (var file in jsonFiles)
            {
                if (file.DateCreated >= newestFile.DateCreated)
                {
                    newestFile = file;
                }
            }

            return newestFile;
        }

        //Return list of all rapport entries
        private async Task<List<InformationRapportModel>> ReadLastRapportAsync()
        {
            await OpenFileAsync();
            string received = fileReader.ReadString((uint) stream.Size);

            return JsonSerializer.Deserialize<List<InformationRapportModel>>(received);
        }

        //Open the last log and ask what application should open it
        public ICommand _openLog;
        public ICommand OpenLog
        {
            get
            {
                if (_openLog == null)
                {
                    _openLog = new RelayCommand(async delegate ()
                    {
                        StorageFile file = await GetLastRapportAsync();
                        if (file == null) return;
                        LauncherOptions options = new LauncherOptions
                        {
                            DisplayApplicationPicker = true
                        };
                        await Launcher.LaunchFileAsync(file, options);
                    }, delegate () { return true; });
                }
                return _openLog;
            }
        }

        //Open the location where logs are saved
        public ICommand _openLogLocation;
        public ICommand OpenLogLocation
        {
            get
            {
                if (_openLogLocation == null)
                {
                    _openLogLocation = new RelayCommand(async delegate ()
                    {
                        StorageFolder folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Rapports", CreationCollisionOption.OpenIfExists);
                        await Launcher.LaunchFolderAsync(folder);
                    }, delegate () { return true; });
                }
                return _openLogLocation;
            }
        }

    }
}
