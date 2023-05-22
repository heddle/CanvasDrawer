using System;
namespace CanvasDrawer.State
{
	public enum EState
	{
        Idle, Drag, Feedback, Edit, Banding, Connect, Reconnect,
        Pan, Zoom, Reshape, Write, Read, PanPause, Placing
    }
}

