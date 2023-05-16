using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAV_App.AI
{
    public class AnimalDetection : IAICallback
    {
        public enum AnimalType
        {
            HARMFUL,
            OTHER
        }

        public List<AnimalType> processPrediction()
        {
            return null;
        }

        //TODO: change return type to an image, don't know the data type atm
        public void prepareImage()
        {
            
        }

        public void notifyCallback()
        {

        }

        public void startPrediction()
        {

        }
    }
}
