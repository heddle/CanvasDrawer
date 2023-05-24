using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CanvasDrawer.Graphics.Selection;
using CanvasDrawer.Graphics.Items;
using CanvasDrawer.Pages;
using CanvasDrawer.Graphics.Toolbar;
using CanvasDrawer.State;

namespace CanvasDrawer.Graphics.Feedback {
    public class FeedbackManager {

        //use thread safe singleton pattern
        private static FeedbackManager _instance;
        private static readonly object _padlock = new object();

        //used to update the status of the corresponding razor component
        public delegate void Refresh();
        public Refresh Refresher { get; set; }


        public delegate void FeedbackUpdate(List<string> fbstrings);
        public FeedbackUpdate FeedbackUpdater { get; set; }

        //list for holding fedback
        private List<string> _feedbackStrings;

        FeedbackManager() : base() {
            _feedbackStrings = new List<string>();
        }

        public static FeedbackManager Instance {
            get {
                lock (_padlock) {
                    if (_instance == null) {
                        _instance = new FeedbackManager();
                    }
                    return _instance;
                }
            }
        }

        //update the feedback debugging display
        public void Update(UserEvent ue) {

            //don't bother if feedback hidden
            if (!DisplayManager.Instance.IsFeedbackVisible()) {
                return;
            }

   
          

            _feedbackStrings.Clear();
            _feedbackStrings.Add("Use CTRL-SHIFT-F to hide");
            _feedbackStrings.Add("State: " + StateMachine.Instance.CurrentState);
            _feedbackStrings.Add(String.Format("Map Mouse location: [{0:0.#}, {1:0.#}]", ue.X, ue.Y));
            _feedbackStrings.Add(String.Format("Page Mouse location: [{0:0.#}, {1:0.#}]", ue.Xpage, ue.Ypage));

            //is the canvas dirty
            _feedbackStrings.Add(DirtyManager.Instance.ToString());

			JSInteropManager? jsm = JSInteropManager.Instance;
			if (jsm == null) {
				return;
			}

			double cw = jsm.CanvasWidth;
            double ch = jsm.CanvasHeight;
            _feedbackStrings.Add(String.Format("Canvas size: [{0:0.#}, {1:0.#}]", cw, ch));


            double cssw = jsm.CssWidth;
            double cssh = jsm.CssHeight;
            _feedbackStrings.Add(String.Format("CSS size: [{0:0.#}, {1:0.#}]", cssw, cssh));

            double dpi = jsm.GetDPI();
            double ez = jsm.ZoomLevel;
            _feedbackStrings.Add("DPI: " + dpi + "  Zoom level: " + ez);

            double jOffLeft = jsm.GetCanvasOffsetLeft();
            double jOffTop = jsm.GetCanvasOffsetTop();
            _feedbackStrings.Add(String.Format("Canvas Offset: [{0:0.#}, {1:0.#}]", jOffLeft, jOffTop));

            _feedbackStrings.Add(jsm.ToString());

            Item item = SelectionManager.Instance.ItemAtEvent(ue);
            if (item != null) {
                item.AddFeedback(ue, _feedbackStrings);
            }

            FeedbackUpdater(_feedbackStrings);

        }
    }

}
