﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace UAV_App.AI
{
    public class AIDetection
    {
        private Process process;
        public AIDetection()
        {
            InitProcess();
        }

        private void DownscaleResolution_Click(object sender, EventArgs e)
        {
            DownscaleResolution();
        }

        private void tileImage_Click(object sender, EventArgs e)
        {
            ConvertImageToTiles();
        }

        public void DownscaleResolution()
        {
            Debug.WriteLine("Starting DownscaleResolution...");

            string imagePath = @"C:/School/Jaar_3/Project_UAV/AI/Image_Preprocessing/Test4k.JPG";
            string savePath = @"C:/School/Jaar_3/Project_UAV/AI/Image_Preprocessing/TestDownscale.jpg";

            Bitmap b = new Bitmap(Image.FromFile(imagePath));
            Size size = new Size(2720, 1530);
            int sourceWidth = b.Width;
            int sourceHeight = b.Height;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;
            //Calulate  width with new desired size  
            nPercentW = ((float)size.Width / (float)sourceWidth);
            //Calculate height with new desired size  
            nPercentH = ((float)size.Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;
            //New Width  
            int destWidth = (int)(sourceWidth * nPercent);
            //New Height  
            int destHeight = (int)(sourceHeight * nPercent);
            Bitmap newImage = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage((Image)newImage);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            // Draw image with new width and height  
            g.DrawImage((Image)b, 0, 0, destWidth, destHeight);
            g.Dispose();

            newImage.Save(savePath, ImageFormat.Jpeg);

            Debug.WriteLine("Ending DownscaleResolution...");
        }

        public void ConvertImageToTiles()
        {
            Debug.WriteLine("Starting ConvertImageToTiles...");

            string imagePath = @"C:/School/Jaar_3/Project_UAV/AI/Image_Preprocessing/TestDownscale.JPG";
            string savePath = @"C:/School/Jaar_3/Project_UAV/AI/Image_Preprocessing/TileImages/TestTileImage";

            Image[] imgarray = new Image[16];
            Bitmap img = new Bitmap(Image.FromFile(imagePath));

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    var index = i * 4 + j;
                    imgarray[index] = new Bitmap(672, 382);
                    var graphics = Graphics.FromImage(imgarray[index]);
                    graphics.DrawImage(img, new Rectangle(0, 0, 672, 382), new Rectangle(i * 672, j * 382, 672, 382), GraphicsUnit.Pixel);
                    graphics.Dispose();
                }
            }

            for (int k = 0; k < imgarray.Length; k++)
            {
                imgarray[k].Save(savePath + k + ".jpg", ImageFormat.Jpeg);
            }

            Debug.WriteLine("Ending ConvertImageToTiles...");
        }

        public void InitProcess()
        {
            this.process = new Process();

            // Set the Python script path
            string scriptPath = @"C:/School/Jaar_3/Project_UAV/AI/AI_Training_Code/yolov5/detect.py";

            // Set the command-line arguments for the script
            double confidenceThreshold = 0.5;
            string name = "prediction"; //TODO: change name to varying directory name so it doesnt overwrite
            int imageSize = 672;
            string weightsPath = @"C:/School/Jaar_3/Project_UAV/AI/AI_Training_Code/yolov5/runs/train/exp11/weights/best.pt";
            string sourcePath = @"C:/School/Jaar_3/Project_UAV/AI/Image_Preprocessing/TileImages";

            string arguments = $"--weights {weightsPath} --img {imageSize} --conf {confidenceThreshold} --source {sourcePath} --name {name} --save-txt --hide-conf --hide-labels --save-conf --nosave";

            // Configure the process start info
            this.process.StartInfo.FileName = "python";
            this.process.StartInfo.Arguments = $"{scriptPath} {arguments}"; // Include arguments in the script path
            this.process.StartInfo.UseShellExecute = false;
            this.process.StartInfo.CreateNoWindow = true;
            this.process.StartInfo.RedirectStandardOutput = true;
            this.process.StartInfo.RedirectStandardError = true;
        }

        public static void RunDetectionScript(Process process)
        {
            Debug.WriteLine("Starting RunDetectionScript...");

            try
            {
                // Start the process
                process.Start();

                // Read the output and error streams
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                // Wait for the process to exit
                //process.WaitForExit();

                // Display the output and error messages
                Debug.WriteLine("Output:");
                Debug.WriteLine(output);
                Debug.WriteLine("Error:");
                Debug.WriteLine(error);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                // Close the process
                process.Close();
            }

            Debug.WriteLine("Ending RunDetectionScript...");
        }

        /*public static void RunDetectionScript(Process process) 
        {
            Debug.WriteLine("Starting RunDetectionScript...");

            // Set the Python script path
            string scriptPath = @"C:/School/Jaar_3/Project_UAV/AI/AI_Training_Code/yolov5/detect.py";

            // Set the command-line arguments for the script
            double confidenceThreshold = 0.5;
            string name = "prediction";
            int imageSize = 672;
            string weightsPath = @"C:/School/Jaar_3/Project_UAV/AI/AI_Training_Code/yolov5/runs/train/exp11/weights/best.pt";
            string sourcePath = @"C:/School/Jaar_3/Project_UAV/AI/Image_Preprocessing/TileImages";

            string arguments = $"--weights {weightsPath} --img {imageSize} --conf {confidenceThreshold} --source {sourcePath} --name {name} --save-txt --hide-conf --hide-labels --save-conf --nosave";


            using (Py.GIL())  // Acquire the Python Global Interpreter Lock (GIL)
            {
                *//*dynamic sys = Py.Import("sys");
                dynamic scriptScope = Py.CreateScope();

                // Import the script as a module
                dynamic scriptModule = Py.Import(scriptPath);

                // Set the command-line arguments
                sys.argv = new[] { "", "--weights", weightsPath, "--img", imageSize.ToString(), "--conf", confidenceThreshold.ToString(), "--source", sourcePath, "--name", name, "--save-txt", "--hide-conf", "--hide-labels", "--save-conf", "--nosave" };

                // Execute the script within the script scope
                scriptModule.Execute(scriptScope);

                // Access the variables or functions from the script if needed
                dynamic result = scriptScope.Get("result");*//*


                dynamic detectModule = Py.Import("detect");  // Import the detect module
                dynamic mainFunc = detectModule.main;  // Get the reference to the main function

                // Execute the detect module's main function
                mainFunc(weightsPath, imageSize, confidenceThreshold, sourcePath, name);

            }

            Debug.WriteLine("Ending RunDetectionScript...");
        }*/

        public List<Bird> SplitObjectsFromData(string data)
        {
            string[] values = data.Trim().Split("\r\n");

            List<Bird> birds = new List<Bird>();
            for (int i = 0; i < values.Length; i++)
            {
                int classId;
                double xCenter, yCenter, width, height, conf;
                string[] birdAttributes = values[i].Split(' ');

                if (int.TryParse(birdAttributes[0], out classId) &&
                   double.TryParse(birdAttributes[1], out xCenter) &&
                   double.TryParse(birdAttributes[2], out yCenter) &&
                   double.TryParse(birdAttributes[3], out width) &&
                   double.TryParse(birdAttributes[4], out height) &&
                   double.TryParse(birdAttributes[5], out conf))
                {
                    birds.Add(new Bird("Gull", classId, xCenter, yCenter, width, height, conf));
                }
            }
            return birds;
        }

        public void ProcessPrediction()
        {
            Debug.WriteLine("Starting ProcessPrediction...");

            string directoryPath = @"C:\School\Jaar_3\Project_UAV\AI\AI_Training_Code\yolov5\runs\detect\prediction\labels\";
            try
            {
                string[] filePaths = Directory.GetFiles(directoryPath, "*.txt");

                List<Bird> allBirds = new List<Bird>();
                int birdsOnTheTopLeftCounter = 0;
                int birdsOnTheTopRightCounter = 0;
                int birdsOnTheBottomLeftCounter = 0;
                int birdsOnTheBottomRightCounter = 0;

                foreach (string filePath in filePaths)
                {
                    string fileContents = File.ReadAllText(filePath);
                    string fileName = Path.GetFileName(filePath);
                    int fileNumber;
                    int.TryParse(string.Concat(fileName.Where(char.IsDigit)), out fileNumber);

                    List<Bird> currentBirds = SplitObjectsFromData(fileContents);
                    allBirds.AddRange(currentBirds);

                    if (currentBirds.Count == 0)
                        break;
                    else if (fileNumber <= 4)
                        birdsOnTheTopLeftCounter++;
                    else if (fileNumber <= 8)
                        birdsOnTheTopRightCounter++;
                    else if (fileNumber <= 12)
                        birdsOnTheBottomLeftCounter++;
                    else
                        birdsOnTheBottomRightCounter++;
                }

                Debug.WriteLine("Top Left detected: " + birdsOnTheTopLeftCounter + "\n" +
                                                   "Top Right detected: " + birdsOnTheTopRightCounter + "\n" +
                                                   "Bottom Left detected: " + birdsOnTheBottomLeftCounter + "\n" +
                                                   "Bottom Right detected: " + birdsOnTheBottomRightCounter + "\n" +
                                                   "Total birds detected: " + allBirds.Count);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            Debug.WriteLine("Ending ProcessPrediction...");
        }

        private void fullTestButton_Click(object sender, EventArgs e)
        {
            /*if (this.process != null && !this.process.HasExited)
            {
                Debug.WriteLine("Process is already running");
                return;
            }*/
            Stopwatch stopwatch = new Stopwatch();


            Debug.WriteLine("Starting AI...");
            stopwatch.Start();
            DownscaleResolution();
            stopwatch.Stop();
            TimeSpan elapsedTimeDownscale = stopwatch.Elapsed;
            stopwatch.Reset();

            stopwatch.Start();
            ConvertImageToTiles();
            stopwatch.Stop();
            TimeSpan elapsedTimeTiles = stopwatch.Elapsed;
            stopwatch.Reset();

            stopwatch.Start();
            RunDetectionScript(this.process);
            stopwatch.Stop();
            TimeSpan elapsedTimeDetection = stopwatch.Elapsed;
            stopwatch.Reset();

            stopwatch.Start();
            ProcessPrediction();
            stopwatch.Stop();
            TimeSpan elapsedTimePredictions = stopwatch.Elapsed;
            stopwatch.Reset();
            Debug.WriteLine("Ending AI...");

            Debug.WriteLine($"Elapsed time Dowscale: {elapsedTimeDownscale.TotalMilliseconds} milliseconds");
            Debug.WriteLine($"Elapsed time Tiles: {elapsedTimeTiles.TotalMilliseconds} milliseconds");
            Debug.WriteLine($"Elapsed time Detections: {elapsedTimeDetection.TotalMilliseconds} milliseconds");
            Debug.WriteLine($"Elapsed time Predictions: {elapsedTimePredictions.TotalMilliseconds} milliseconds");
        }
    }
}
