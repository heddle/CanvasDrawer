using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CanvasDrawer.Graphics {

    public sealed class UserEvent {

        public EUserEventType Type { get; set; }

        //mouse locations in canvas relative coordinates
        public double X { get; set; }
        public double Y { get; set; }

        //mouse locations relative to page
        //needed for popup
        public double Xpage { get; set; }

        public double Ypage { get; set; }

        //keys pressed simultaneously
        public bool Alt { get; set; }
        public bool Control { get; set; }
        public bool Meta { get; set; }
        public bool Shift { get; set; }

        //left, middle, right = 0, 1, 2
        public int Button { get; set; }

        //was it a single or double click?
        public bool SingleClick { get; set; }
        public bool DoubleClick { get; set; }


        public int ResizeRectIndex { get; set; }

        //empty constructor
        public UserEvent() { }

        //copy constructor
        public UserEvent(UserEvent src) {
            Set(src);
        }

        public void Set(UserEvent src) {
            Type = src.Type;
            X = src.X;
            Y = src.Y;
            Xpage = src.Xpage;
            Ypage = src.Ypage;

            Alt = src.Alt;
            Control = src.Control;
            Meta = src.Meta;
            Shift = src.Meta;
            Button = src.Button;
            SingleClick = src.SingleClick;
            DoubleClick = src.DoubleClick;
            ResizeRectIndex = src.ResizeRectIndex;
        }

        //check if any modifier was pressed
        public bool AnyModifier() {
            return Alt || Control || Meta || Shift;
        }

        private string MouseString(string prefix, double x, double y) {
            return String.Format(prefix + " [{0:0.#}, {1:0.#}]", x, y);
        }

        //string representation
        public override string ToString() {
            String ms = MouseString("Loc: ", X, Y);
            return "UserEvent [" + Type.ToString() + "] " + ms;
        }
    }

 
}
