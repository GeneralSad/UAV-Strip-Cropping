using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace UAV_App.AI
{
    //Datatype to hold information about the AI prediction.
    public struct AI_Prediction {
        public int amountOfBirdsDetected;
    }

    public class AIDetection
    {
        private Process process;
        public AIDetection()
        {
            InitProcess();
        }

        // Initializes the process so that all the arguments of the command are only set once. 
        public void InitProcess()
        {
            this.process = new Process();

            // Set the Python script path
            string scriptPath = @"..\..\Assets\AI_Assets\yolov5\detect.py";

            // Set the command-line arguments for the script
            double confidenceThreshold = 0.5;
            string name = "prediction"; //TODO: change name to varying directory name so it doesnt overwrite
            int imageSize = 672;
            string weightsPath = @"..\..\Assets\AI_Assets\yolov5\runs\train\best\best.pt";
            string sourcePath = @"..\..\Assets\AI_Assets\Image_Preprocessing\TileImages";

            string arguments = $"--weights {weightsPath} --img {imageSize} --conf {confidenceThreshold} --source {sourcePath} --name {name} --save-txt --hide-conf --hide-labels --save-conf --nosave";

            // Configure the process start info
            this.process.StartInfo.FileName = "python";
            this.process.StartInfo.Arguments = $"{scriptPath} {arguments}"; // Include arguments in the script path
            this.process.StartInfo.UseShellExecute = false;
            this.process.StartInfo.CreateNoWindow = true;
            this.process.StartInfo.RedirectStandardOutput = true;
            this.process.StartInfo.RedirectStandardError = true;
        }

        // Takes the image taken by the drone and scales it down to the correst resolution used by the AI model.
        public void DownscaleResolution()
        {
            Debug.WriteLine("Starting DownscaleResolution...");

            string imagePath = @"..\..\Assets\AI_Assets\Image_Preprocessing\Image4K.JPG"; //TODO: change name of Image4K to correct imagename of Leon's image
            string savePath = @"..\..\Assets\AI_Assets\Image_Preprocessing\ImageDownscale.jpg";

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

        // Splits the downscaled image into 16 smaller image so they can be processed faster by the AI model.
        public void ConvertImageToTiles()
        {
            Debug.WriteLine("Starting ConvertImageToTiles...");

            string imagePath = @"..\..\Assets\AI_Assets\Image_Preprocessing\ImageDownscale.JPG";
            string savePath = @"..\..\Assets\AI_Assets\Image_Preprocessing\TileImages/TileImage";

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

        // Runs the detect.py script for all the split-images.
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

        // Reads the data and splits the data up into Bird objects. This way the data can be read and consumed easily.
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

        // Reads the raw data and applies logic to it. Inn this function the program counts the mount of birds detected in the images and groups them based on their location.
        public AI_Prediction ProcessPrediction()
        {
            Debug.WriteLine("Starting ProcessPrediction...");

            AI_Prediction prediction = new AI_Prediction();

            string directoryPath = @"..\..\Assets\AI_Assets\yolov5\runs\detect\prediction\labels\";
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
                prediction.amountOfBirdsDetected = allBirds.Count;
                return prediction;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return prediction;
            }
            Debug.WriteLine("Ending ProcessPrediction...");
        }

        // Runs the full detection process from beginning to end.
        public async Task<AI_Prediction> RunFullDetection()
        {
            Debug.WriteLine("Starting AI...");

            DownscaleResolution();
            ConvertImageToTiles();
            RunDetectionScript(this.process);

            Debug.WriteLine("Ending AI...");
            return ProcessPrediction();
        }
    }
}
