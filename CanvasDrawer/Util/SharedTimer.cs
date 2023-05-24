using CanvasDrawer.Graphics;
using CanvasDrawer.Graphics.Theme;
using CanvasDrawer.Pages;
using CanvasDrawer.State;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Transactions;

namespace CanvasDrawer.Util {
    public class SharedTimer {

        //use thread safe singleton pattern
        private static SharedTimer _instance;
        private static readonly object _padlock = new object();

        //The global timer
        public System.Timers.Timer Timer { get; }

        public bool OffsetGrabPending { get; set; }

        public bool RedrawPending { get; set; }

        public bool PageManagerRefreshPending { get; set; }

        public SharedTimer() : base() {
            // A shared timer that fires every second
            Timer =  new System.Timers.Timer(1000);
            Timer.Elapsed += Counter;
            Timer.AutoReset = true;
            Timer.Enabled = true;
        }

        /// <summary>
        /// Increment the count of the number of times the timer fired.
        /// Use Count for things that happen in multiples of one second.
        /// </summary>
        private void Counter(Object source, ElapsedEventArgs e) {

            //see if we need to update because of scrolling, etc.
            if (OffsetGrabPending) {
                OffsetGrabPending = false;

                if (JSInteropManager.Instance != null) {
					JSInteropManager.Instance.SetOffsetsDirty();
                }
            }

            //see if any redraws pending
            if (RedrawPending && StateMachine.Instance.CurrentState == EState.Idle) {
                RedrawPending = false;
                GraphicsManager.Instance.ForceDraw();
            }

            //see if any page manager refreshes are pending
            if (PageManagerRefreshPending) {
                PageManagerRefreshPending = false;
                PageManager pm = GraphicsManager.Instance.PageManager;
                if (pm.Refresher != null) {
                    pm.Refresher();
                }
            }

        }

        //public access to the singleton
        public static SharedTimer Instance {
            get {
                lock (_padlock) {
                    if (_instance == null) {
                        _instance = new SharedTimer();
                    }
                    return _instance;
                }
            }
        }

    }
}
