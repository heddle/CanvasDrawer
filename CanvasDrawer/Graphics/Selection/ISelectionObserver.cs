using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CanvasDrawer.Graphics.Items;

namespace CanvasDrawer.Graphics.Selection {
    public interface ISelectionObserver {
        void SelectionChange(List<Item> selectedItems);
    }
}
