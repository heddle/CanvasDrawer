﻿using System;
using CanvasDrawer.Pages;
using CanvasDrawer.State;

namespace CanvasDrawer.Graphics.Rubberband {
    public sealed class RubberbandManager {

        //colors used in rubberbanding
        private static readonly string[] TRANSCOLOR = { "rgba(95%,85%,85%,0.25)", "rgba(75%, 75%, 95%, 0.25)" };
        private static readonly string[] BORDCOLOR = { "#555555", "#4444EE" };

        public int ClickCount { get; set; }

        public PageManager PageManager { get; set; }

        //use thread safe singleton pattern
        private static RubberbandManager _instance;
        private static readonly object _padlock = new object();

        private Rect rbRect;

        //am i currently rubber banding?
        //the start of the rubber band
        private UserEvent _startEvent;

        //the current mode
        private ERubberbandMode _mode;

        //picks the colors in RECTANGLE mode
        private int _option;

        RubberbandManager() : base() {
        }

        public static RubberbandManager Instance {
            get {
                lock (_padlock) {
                    if (_instance == null) {
                        _instance = new RubberbandManager();
                    }
                    return _instance;
                }
            }
        }

        /// <summary>
        /// Initialize the rubber banding
        /// </summary>
        /// <param name="mode">The mode, e.g. RECTANGLE, LINE, ELLIPSE, CLOUD</param>
        /// <param name="ue">The mouse event.</param>
        /// <param name="option"></param>
        public void InitRubberbanding(ERubberbandMode mode, UserEvent ue, int option) {
            _option = Math.Max(0, Math.Min(1, option));
            _startEvent = new UserEvent(ue);
            _mode = mode;
            PageManager.SaveCanvasInBackgoundImage();
            rbRect = null;
            ClickCount = 0;
        }

        /// <summary>
        /// Update the rubberbanding.
        /// </summary>
        /// <param name="ue">The mouse event.</param>
        public void UpdateRubberbanding(UserEvent ue) {

            if (StateMachine.Instance.CurrentState != EState.Banding) {
                return;
            }

             switch (_mode) {
                case ERubberbandMode.RECTANGLEDRAG:

                    double width = ue.X - _startEvent.X;
                    double height = ue.Y - _startEvent.Y;

                    if (rbRect == null) {
                        rbRect = new Rect();
                    }
                    else {
                        PageManager.RestoreRectangularAreaFromBackgroundImage(rbRect.X, rbRect.Y, rbRect.Width, rbRect.Height);
                    }
                    rbRect.Set(_startEvent.X - 2, _startEvent.Y - 2, width + 4, height + 4);
                    PageManager.DrawRect(_startEvent.X, _startEvent.Y, width, height,
                        TRANSCOLOR[_option], BORDCOLOR[_option], 1, 0);
                    break;

                case ERubberbandMode.ELLIPSE:
                    width = ue.X - _startEvent.X;
                    height = ue.Y - _startEvent.Y;

                    if (rbRect == null) {
                        rbRect = new Rect();
                    }
                    else {
                        PageManager.RestoreRectangularAreaFromBackgroundImage(rbRect.X, rbRect.Y, rbRect.Width, rbRect.Height);
                    }
                    rbRect.Set(_startEvent.X - 2, _startEvent.Y - 2, width + 4, height + 4);

                    PageManager.DrawEllipse(_startEvent.X + width / 2, _startEvent.Y + height / 2, width / 2, height / 2, TRANSCOLOR[_option], BORDCOLOR[_option], 1);
                    break;

                case ERubberbandMode.CLOUD:

                    width = ue.X - _startEvent.X;
                    height = ue.Y - _startEvent.Y;

                    if (rbRect == null) {
                        rbRect = new Rect();
                    }
                    else {
                        PageManager.RestoreRectangularAreaFromBackgroundImage(rbRect.X, rbRect.Y, rbRect.Width, rbRect.Height);
                    }
                    rbRect.Set(_startEvent.X - 2, _startEvent.Y - 2, width + 4, height + 4);
                    PageManager.DrawImage(_startEvent.X, _startEvent.Y, width, height, "cloud_Net");
                    break;

                case ERubberbandMode.LINE:
                    break;

                default:
                    break;
            }
        }

        public UserEvent EndRubberbanding() {
            PageManager.RestoreCanvasFromBackgroundImage();
            return _startEvent;
        }


    }
}
