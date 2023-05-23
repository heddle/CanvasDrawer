using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CanvasDrawer.Graphics.Items {
    public interface IItemObserver {
        void ItemChangeEvent(ItemEvent e); 
    }
}
