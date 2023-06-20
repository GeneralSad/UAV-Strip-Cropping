﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAV_App.AI
{
    public interface IAIReceiverCallback
    {
        void onPrediction(List<int> predictions); //TODO: Type of List is YoloPrediction
    }
}