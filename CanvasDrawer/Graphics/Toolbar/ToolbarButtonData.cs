using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CanvasDrawer.Graphics.Toolbar {
    public class ToolbarButtonData {

        //maps to html element id
        public string Id { get; set; }

        //is the button selected?
        public bool Selected { get; set; } = false;

        //is the button selected?
        public bool Enabled { get; set; } = true;

        public EToolbarButton Type { get; set; }

        //grouping for radio buton behavior
        public int Group { get; set; } = -1;

        //the image id see Toolbar.razor
        public string ImageId { get; set; }

        //is it a tool? if not it is a node
        public bool IsTool { get; set; }

    }
}
