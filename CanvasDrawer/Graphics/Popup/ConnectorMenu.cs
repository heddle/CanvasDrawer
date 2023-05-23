using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using CanvasDrawer.Graphics.Toolbar;

namespace CanvasDrawer.Graphics.Popup {
    public sealed class ConnectorMenu : BaseButtonPopup {

        public static string[] CnxImageFiles { get; set; } = {"images/connector.svg", "images/wan.svg" };

        //the types of connectors
        public static readonly int LINECNX = 0;
        public static readonly int WANCNX = 1;

        //use thread safe singleton pattern
        private static ConnectorMenu _instance;
        private static readonly object _padlock = new object();

        /// <summary>
        /// Create the ConnectorMenu singleton.
        /// </summary>
        public ConnectorMenu() : base(CnxImageFiles, "connector") {
            CurrentSelection = LINECNX;
        }


        /// <summary>
        /// Public access to the singleton.
        /// </summary>
        public static ConnectorMenu Instance {
            get {
                lock (_padlock) {
                    if (_instance == null) {
                        _instance = new ConnectorMenu();
                    }
                    return _instance;
                }
            }
        }
    }
}
