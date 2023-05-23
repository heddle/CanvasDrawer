using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CanvasDrawer.Graphics.Items {

    //Item types
    public enum EItemType {
        //elbows are deprecated but kept here for a while for backwards compatibility
        Annotation, NodeBox, Node, LineConnector, ElbowConnector, Text, Unknown
    }
}


