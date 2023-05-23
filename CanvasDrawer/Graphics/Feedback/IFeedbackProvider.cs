using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CanvasDrawer.Graphics.Feedback {
    public interface IFeedbackProvider {
        void AddFeedback(UserEvent ue, List<string> feedbackStrings);
    }
}
