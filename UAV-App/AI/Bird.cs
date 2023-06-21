using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAV_App.AI
{
    public class Bird
    {
        public string className;
        public int classId;
        public double objectCenterX;
        public double objectCenterY;
        public double objectWidth;
        public double objectHeight;
        public double confidence;

        public Bird(string className, int classId, double objectCenterX, double objectCenterY, double objectWidth, double objectHeight, double confidence)
        {
            this.className = className;
            this.classId = classId;
            this.objectCenterX = objectCenterX;
            this.objectCenterY = objectCenterY;
            this.objectWidth = objectWidth;
            this.objectHeight = objectHeight;
            this.confidence = confidence;
        }
    }
}
