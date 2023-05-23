using System;
using CanvasDrawer.State;

namespace CanvasDrawer.State
{
    public sealed class StateMachine
    {

        private static StateMachine? _instance; //? means its nullable

        public EState CurrentState { get; set; } = EState.Idle;

        private StateMachine() : base()
        {
        }

        //public access to singleton
        public static StateMachine Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new StateMachine();
                }
                return _instance;
            }
        }

    }


}
