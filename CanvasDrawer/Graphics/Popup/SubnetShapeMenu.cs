using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;


/// <summary>
/// The menu for selecting the subnet shape.
/// </summary>
namespace CanvasDrawer.Graphics.Popup {
    public sealed class SubnetShapeMenu : BaseButtonPopup {

        public static string[] ShapeImageFiles { get; set; } = { "images/sub_Rectangle.svg", "images/sub_Circle.svg", "images/sub_Cloud.svg" };

        //the types of shapes
        public static readonly int RECTANGLE = 0;
        public static readonly int ELLIPSE   = 1;
        public static readonly int CLOUD     = 2;

        //use thread safe singleton pattern
        private static SubnetShapeMenu _instance;
        private static readonly object _padlock = new object();

        public SubnetShapeMenu() : base(ShapeImageFiles, "subnet") {
            CurrentSelection = RECTANGLE;
        }

        /// <summary>
        /// Public access to the singleton.
        /// </summary>
        public static SubnetShapeMenu Instance {
            get {
                lock (_padlock) {
                    if (_instance == null) {
                        _instance = new SubnetShapeMenu();
                    }
                    return _instance;
                }
            }
        }

    }
}