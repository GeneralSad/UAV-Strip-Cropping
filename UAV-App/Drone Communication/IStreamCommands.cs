using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAV_App.Drone_Communication
{
    public interface IStreamCommands
    {
        void getLastFrame(); //TODO: change return type to an image, don't know the data type atm

        //TODO: Move camera methods from OverlayPage to here
    }
}
