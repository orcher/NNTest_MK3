using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NNTest_MK3
{
    static class NeuronSettings
    {
        public static bool ShowOutput { get; set; }
        public static bool ShowWeights { get; set; }
        public static bool ShowBias { get; set; }
        public static bool ShowError { get; set; }
        public static double LearningFactor { get; set; }
        public static double MobilityFactor { get; set; }
    }
}
