using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAV_App.Models
{
    //Class for storing infomation. Gets used to serialize data to json and back
    public class InformationRapportModel
    {

        public InformationRapportModel()
        {
        }

        public InformationRapportModel(DateTime time, double latitude, double longitude, int friendly, int harmful, int neutral) {
            this.Time = time;
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Friendly = friendly;
            this.Harmful = harmful;
            this.Neutral = neutral;
        }

        public DateTime Time { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Friendly { get; set; }
        public int Harmful { get; set; }
        public int Neutral { get; set; }
    }
}
