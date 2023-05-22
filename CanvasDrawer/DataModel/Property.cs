using System;
using System.Text.Json;
using System.Text.Json.Serialization;


using CanvasDrawer.Util;

namespace CanvasDrawer.DataModel
{


    public class Property
    {


        private bool _displayOnCanvas;

        [JsonIgnore]
        public bool DisplayedOnCanvas
        {
            get { return IsDisplayedOnCanvas(); }
            set { _displayOnCanvas = value; SetDisplayedOnCanvas(_displayOnCanvas); }
        }

        //the control bits
        public int ControlBits { get; set; } = 0;

        //bitwise features of the property
        public static readonly int DISPLAYEDONCANVAS = 01;
        public static readonly int EDITABLE = 02;
        public static readonly int FEEDBACKABLE = 04;
        public static readonly int SHOWINEDITOR = 010;
        public static readonly int NOTDISPLAYABLE = 020;




        //common
        public static readonly int BASIC = 0xFF;
        public static readonly int NOTDISPLAYEDONCANVAS = BASIC ^ DISPLAYEDONCANVAS;
        public static readonly int NOTEDITABLE = FEEDBACKABLE | SHOWINEDITOR;

        //keep deprecated keys for backward compatibility
        public static readonly int DEPRECATED = 0;

        //hidden and deprecated behave the same wa, they don't show up anywhere
        public static readonly int HIDDEN = 0;

        //the property key or name
        public string? Key { get; set; }

        // the value of the property
        public string? Value { get; set; }

        //null constructor required for deserialization
        public Property()
        {
        }

        public Property(string key, string value, int bits) : base()
        {
            Key = key;
            Value = value;
            ControlBits = bits;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src">The property to copy.</param>
        public Property(Property src)
        {
            Key = (string)src.Key.Clone();
            Value = (string)src.Value.Clone();
            ControlBits = src.ControlBits;
        }

        //check the control bits
        public bool CheckBit(int bit)
        {
            return Bits.CheckBit(ControlBits, bit);
        }

        //set a bit in the control bits
        public void SetBit(int bit)
        {
            ControlBits = Bits.SetBit(ControlBits, bit);
        }

        //clear a bit in the control bits
        public void ClearBit(int bit)
        {
            ControlBits = Bits.ClearBit(ControlBits, bit);
        }

        /// <summary>
        /// Is this property displayable on the map?
        /// </summary>
        /// <returns>true if the property is displayable</returns>
        public bool IsDisplayable()
        {
            return !IsNotDisplayable();
        }

        /// <summary>
        /// Is this property displayable on the canvas?
        /// </summary>
        /// <returns>true if the property is not displayable</returns>
        public bool IsNotDisplayable()
        {
            return CheckBit(NOTDISPLAYABLE);
        }

        /// <summary>
        /// Is this property displayed on the canvas?
        /// </summary>
        /// <returns>true if the property is displayed on the map.</returns>
        private bool IsDisplayedOnCanvas()
        {
            return IsDisplayable() && CheckBit(DISPLAYEDONCANVAS);
        }

        /// <summary>
        /// Set whether this property is displayed on the canvas.
        /// </summary>
        /// <param name="disp">The value of the setting.</param>
        private void SetDisplayedOnCanvas(bool disp)
        {
            if (disp)
            {
                SetBit(DISPLAYEDONCANVAS);
            }
            else
            {
                ClearBit(DISPLAYEDONCANVAS);
            }
        }

        /// <summary>
        /// Toggle whether the property is displayable on the canvas.
        /// </summary>
        public void ToggleDisplayedOnCanvas()
        {
            SetDisplayedOnCanvas(!IsDisplayedOnCanvas());
        }

        //is this property editable?
        public bool Editable()
        {
            return CheckBit(EDITABLE);
        }

        //is this property feedbackable?
        public bool Feedbackable()
        {
            return CheckBit(FEEDBACKABLE);
        }

        //is this property shown in the editor?
        public bool ShowInEditor()
        {
            return CheckBit(SHOWINEDITOR);
        }


        //for displaying on canvas
        public string? DisplayName()
        {
            return Key;
        }

        //is this the name property?
        public bool IsName()
        {
            return DefaultKeys.NAME_KEY.Equals(Key);
        }

        /// <summary>
        /// Is this the locked property?
        /// </summary>
        /// <returns>true if this is the locked property.</returns>
        public bool IsLocked()
        {
            return DefaultKeys.LOCKED_KEY.Equals(Key);
        }

        /// <summary>
        /// Is this the text property?
        /// </summary>
        /// <returns>true if this is the OS property.</returns>
        public bool IsText()
        {
            return DefaultKeys.TEXT_KEY.Equals(Key);
        }

        /// <summary>
        /// Is this the font size property?
        /// </summary>
        /// <returns>true if this is the font size property.</returns>
        public bool IsFontSize()
        {
            return DefaultKeys.FONTSIZE.Equals(Key);
        }

        /// <summary>
        /// Is this the foreground property?
        /// </summary>
        /// <returns>true if this is the foreground property.</returns>
        public bool IsForeground()
        {
            return DefaultKeys.FG_COLOR.Equals(Key);
        }


        //Convert to a string
        public override string ToString()
        {
            return Key + ": " + Value;
        }

        public string Serialize()
        {
            string jsonStr = JsonSerializer.Serialize<Property>(this);
            return jsonStr;
        }

    }
}

