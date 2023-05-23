using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CanvasDrawer.Graphics.Rubberband {
    public class RubberbandData { 
        public ERubberbandMode Mode { get; set; }

        //starting mouse location
        public double StartX { get; set; }
        public double StartY { get; set; }

        //curent mouse location
        public double CurrentX { get; set; }
        public double CurentY { get; set; }

        //ending mouse location
        public double EndX { get; set; }
        public double EndY { get; set; }

    }
}
