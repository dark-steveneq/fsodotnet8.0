﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FSO.Client.UI.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using FSO.Client.UI.Model;
using FSO.Client.Utils;
using FSO.Client.UI.Framework.Parser;
using FSO.Common.Rendering.Framework.Model;
using FSO.Common.Rendering.Framework.IO;
using FSO.Common.Utils;
using FSO.Common.Rendering.Framework;
using FSO.Common;

namespace FSO.Client.UI.Controls
{
    /// <summary>
    /// Big complex text edit control, used in many places
    /// </summary>
    public class UITextEdit : UIElement, IFocusableUI, ITextControl
    {
        /**
         * Standard modes
         */
        public static UITextEdit CreateTextBox()
        {
            return new UITextEdit {
                MaxLines = 1,
                BackgroundTextureReference = UITextBox.StandardBackground,
                TextMargin = new Rectangle(8, 2, 8, 3)
            };
        }

        /**
         * Background texture & resize info
         */
        private Texture2D m_BackgroundTex;
        private ITextureRef m_BackgroundTexRef;
        private NineSliceMargins NineSliceMargins;
        private float m_Width;
        private float m_Height;

        /**
         * Text box vars
         */
        protected StringBuilder m_SBuilder = new StringBuilder();

        /**
         * Interaction
         */
        private UIMouseEventRef m_MouseEvent;

        protected int SelectionStart = -1;
        protected int SelectionEnd = -1;

        public event ChangeDelegate OnChange;
        public event ChangeDelegate OnFocusOut;
        public event KeyPressDelegate OnEnterPress;
        public event KeyPressDelegate OnTabPress;
        public event KeyPressDelegate OnShiftTabPress;
        public event UIMouseEvent NoFocusPassthrough;
        public bool EventSuppressed;

        private UITextEditMode m_Mode = UITextEditMode.Editor;
        private bool m_IsReadOnly = false;

        public UITextEditMode Mode
        {
            set
            {
                m_Mode = value;
                m_IsReadOnly = value == UITextEditMode.ReadOnly;
            }
            get
            {
                return m_Mode;
            }
        }

        [UIAttribute("mode")]
        public string UIScriptMode
        {
            set
            {
                if (value.Equals("kReadOnly", StringComparison.InvariantCultureIgnoreCase))
                {
                    Mode = UITextEditMode.ReadOnly;
                }
            }
        }

        public UITextEdit()
        {
            UIUtils.GiveTooltip(this);
            TextStyle = TextStyle.DefaultLabel;

            m_MouseEvent = ListenForMouse(new Rectangle(0, 0, 10, 10), new UIMouseEvent(OnMouseEvent));
        }

        /**
         * Functionality
         */

        /// <summary>
        /// Returns the current text (input) in this textbox.
        /// </summary>
        public string CurrentText
        {
            get { return m_SBuilder.ToString(); }
            set
            {
                if (value == null) value = "";
                CodeParser = null;
                m_SBuilder = new StringBuilder(value);
                SelectionStart = Math.Max(0, Math.Min(SelectionStart, value.Length - 1));
                SelectionEnd = -1; //todo: move along maybe?
                m_DrawDirty = true;
                Invalidate();
            }
        }

        public void ForceDirty()
        {
            m_DrawDirty = true;
        }

        //formatted mode abstractions

        public bool BBCodeEnabled;
        private BBCodeParser CodeParser;
        public string CurrentTextParsed
        {
            get
            {
                if (BBCodeEnabled)
                {
                    CodeParser = new BBCodeParser(CurrentText);
                    return CodeParser.Stripped;
                }
                else return CurrentText;
            }
        }

        private bool m_Password = false;
        public bool Password
        {
            get { return m_Password; }
            set
            {
                m_Password = value;
                m_DrawDirty = true;
                Invalidate();
            }
        }

        [UIAttribute("font", typeof(TextStyle))]
        public TextStyle TextStyle { get; set; }
        public Rectangle TextMargin = Rectangle.Empty;

        /** Only horizontal alignments work in the text edit control **/
        public TextAlignment Alignment { get; set; }

        [UIAttribute("alignment")]
        public int InternalAlignment
        {
            set
            {
                switch (value)
                {
                    case 3:
                        Alignment = TextAlignment.Center;
                        break;
                }
            }
        }

        [UIAttribute("flashOnEmpty")]
        public bool FlashOnEmpty { get; set; }

        private Color m_FrameColor;
        private Texture2D m_FrameTexture = TextureUtils.TextureFromColor(GameFacade.GraphicsDevice, new Color(255, 249, 157));

        [UIAttribute("frameColor")]
        public Color FrameColor
        {
            get
            {
                return m_FrameColor;
            }
            set
            {
                m_FrameColor = value;
                m_FrameTexture = TextureGenerator.GetPxWhite(GameFacade.GraphicsDevice);
            }
        }

        /**
         * Properties
         */
        
        /// <summary>
        /// Background texture
        /// </summary>
        public Texture2D BackgroundTexture
        {
            get { return m_BackgroundTex; }
        }

        public ITextureRef BackgroundTextureReference
        {
            get { return m_BackgroundTexRef; }
            set
            {
                m_BackgroundTexRef = value;
            }
        }

        public void SetBackgroundTexture(Texture2D texture, int marginLeft, int marginRight, int marginTop, int marginBottom)
        {
            BackgroundTextureReference = null;
            m_BackgroundTex = texture;
            if (texture != null)
            {
                NineSliceMargins = new NineSliceMargins
                {
                    Left = marginLeft,
                    Right = marginRight,
                    Top = marginTop,
                    Bottom = marginBottom
                };
                NineSliceMargins.CalculateOrigins(texture);
            }
            else
            {
                NineSliceMargins = null;
            }
        }

        /// <summary>
        /// Component width
        /// </summary>
        public float Width
        {
            get { return m_Width; }
        }
        
        /// <summary>
        /// Component height
        /// </summary>
        public float Height
        {
            get { return m_Height; }
        }

        [UIAttribute("size")]
        public override Vector2 Size
        {
            get
            {
                return new Vector2(m_Width, m_Height);
            }
            set
            {
                SetSize(value.X, value.Y);
            }
        }

        public void SetSize(float width, float height)
        {
            m_Width = width;
            m_Height = height;

            if (NineSliceMargins != null)
            {
                NineSliceMargins.CalculateScales(m_Width, m_Height);
            }
            m_Bounds = new Rectangle(0, 0, (int)m_Width, (int)m_Height);
            
            if (m_MouseEvent != null)
            {
                m_MouseEvent.Region = new Rectangle(0, 0, (int)m_Width, (int)m_Height);
            }
        }

        public void RemoveMouseEvent()
        {
            RemoveMouseListener(m_MouseEvent);
        }

        private Rectangle m_Bounds;
        public override Rectangle GetBounds()
        {
            return m_Bounds;
        }

        private bool m_IsDraggingSelection = false;

        /**
         * Interaction Functionality
         */
         
        public void OnMouseEvent(UIMouseEventType evt, UpdateState state)
        {
            if (m_IsReadOnly) { return; }

            if (NoFocusPassthrough != null && state.InputManager.GetFocus() != this)
            {
                NoFocusPassthrough?.Invoke(evt, state);
                return;
            }
            switch (evt)
            {
                case UIMouseEventType.MouseDown:
                    /**
                     * Hit test, work out where selection should begin
                     */
                    var position = this.GetMousePosition(state.MouseState);
                    var index = this.HitTestText(position);

                    Control_SetSelectionStart(
                        Control_GetSelectableIndex(index, -1)
                    );
                    SelectionEnd = -1;
                    m_IsDraggingSelection = true;

                    state.InputManager.SetFocus(this);
                    break;

                case UIMouseEventType.MouseOver:
                    GameFacade.Cursor.SetCursor(CursorType.IBeam);
                    break;

                case UIMouseEventType.MouseOut:
                    GameFacade.Cursor.SetCursor(CursorType.Normal);
                    break;

                case UIMouseEventType.MouseUp:
                    m_IsDraggingSelection = false;
                    break;
            }
        }

        public void SelectionToEnd()
        {
            SelectionEnd = -1;
            SelectionStart = m_SBuilder.Length;
        }

        public int GetSelectedInd()
        {
            return Control_GetSelectionStart();
        }

        #region IFocusableUI Members

        private bool IsFocused;
        private string QueuedChange;
        public void OnFocusChanged(FocusEvent newFocus)
        {
            IsFocused = newFocus == FocusEvent.FocusIn;
            if (IsFocused)
            {
                m_cursorBlink = true;
                m_cursorBlinkLastTime = GameFacade.LastUpdateState.Time.TotalGameTime.Ticks;
                /*
                if (FSOEnvironment.SoftwareKeyboard && FSOEnvironment.SoftwareDepth)
                {
                    try
                    {
                        Guide.BeginShowKeyboardInput(PlayerIndex.One, "", "", CurrentText, (ar) =>
                        {
                            var str = Guide.EndShowKeyboardInput(ar);
                            lock (this)
                            {
                                QueuedChange = str;
                            }
                        }, null);
                    }
                    catch (Exception e) { }
                }
                */
            }
            else
            {
                OnFocusOut?.Invoke(this);
                m_cursorBlink = false;
                SelectionEnd = -1;
                SelectionStart = -1;
                m_DrawDirty = true;
                Invalidate();
            }
        }

        #endregion

        private bool m_cursorBlink = false;
        private long m_cursorBlinkLastTime;

        private bool m_frameBlinkOn = false;
        private bool m_frameBlink = false;
        private long m_frameBlinkLastTime;

        public override void Update(UpdateState state)
        {
            if (!Visible) { return; }

            base.Update(state);
            lock (this)
            {
                if (QueuedChange != null)
                {
                    CurrentText = QueuedChange;
                    QueuedChange = null;
                    if (OnChange != null) OnChange(this);
                }
            }
            if (FSOEnvironment.SoftwareKeyboard && FSOEnvironment.SoftwareDepth && state.InputManager.GetFocus() == this) state.InputManager.SetFocus(null);
            if (m_IsReadOnly) { return; }

            if (FlashOnEmpty)
            {
                if (m_SBuilder.Length == 0)
                {
                    /** This field may need to flash **/
                    if (!state.SharedData.ContainsKey("UIText_Flash"))
                    {
                        /** No other field is flashing yet :) **/
                        m_frameBlinkOn = true;
                        state.SharedData.Add("UIText_Flash", this);

                        var now = state.Time.TotalGameTime.Ticks;
                        if (now - m_frameBlinkLastTime > 5000000)
                        {
                            m_frameBlinkLastTime = now;
                            m_frameBlink = !m_frameBlink;
                        }
                    }
                    else
                    {
                        m_frameBlinkOn = false;
                    }
                }
                else
                {
                    m_frameBlinkOn = false;
                }
            }
            else
            {
                m_frameBlinkOn = false;
            }

            if (IsFocused)
            {
                var now = state.Time.TotalGameTime.Ticks;
                if (now - m_cursorBlinkLastTime > 5000000)
                {
                    m_cursorBlinkLastTime = now;
                    m_cursorBlink = !m_cursorBlink;
                    Invalidate();
                }

                var allowInput = m_SBuilder.Length < MaxChars && m_SBuilder.Length.ToString().Split('\n').Count() <= MaxLines;

                var inputResult = state.InputManager.ApplyKeyboardInput(m_SBuilder, state, SelectionStart, SelectionEnd, allowInput);
                if (inputResult != null)
                {
                    SelectionStart = inputResult.SelectionStart;
                    SelectionEnd = inputResult.SelectionEnd;

                    Control_ValidateText();

                    if (inputResult.ContentChanged)
                    {
                        if (OnChange != null)
                        {
                            OnChange(this);
                        }
                    }

                    if (inputResult.ContentChanged || inputResult.SelectionChanged)
                    {
                        m_cursorBlink = true;
                        m_cursorBlinkLastTime = now;

                        /** We need to recompute the drawing commands **/
                        m_DrawDirty = true;
                        Invalidate();
                        Control_ScrollTo(Control_GetSelectionStart());
                    }

                    /**
                     * Selection?
                     */
                    foreach (var key in inputResult.UnhandledKeys)
                    {
                        switch (key)
                        {
                            case Keys.Left:
                                if (inputResult.ShiftDown)
                                {
                                    Control_MoveSelection(-1, 0);
                                }
                                else
                                {
                                    Control_MoveCursor(-1, 0);
                                }
                                break;

                            case Keys.Right:
                                if (inputResult.ShiftDown)
                                {
                                    Control_MoveSelection(1, 0);
                                }
                                else
                                {
                                    Control_MoveCursor(1, 0);
                                }
                                break;

                            case Keys.Down:
                                if (inputResult.ShiftDown)
                                {
                                    Control_MoveSelection(0, 1);
                                }
                                else
                                {
                                    Control_MoveCursor(0, 1);
                                }
                                break;

                            case Keys.Up:
                                if (inputResult.ShiftDown)
                                {
                                    Control_MoveSelection(0, -1);
                                }
                                else
                                {
                                    Control_MoveCursor(0, -1);
                                }
                                break;
                        }
                    }

                    EventSuppressed = false;
                    if (inputResult.EnterPressed && OnEnterPress != null) OnEnterPress(this);
                    if (inputResult.TabPressed && OnTabPress != null) OnTabPress(this);
                    if (inputResult.ShiftDown && inputResult.TabPressed && OnShiftTabPress != null) OnShiftTabPress(this);
                }

                if (m_IsDraggingSelection)
                {
                    /** Dragging **/
                    var position = this.GetMousePosition(state.MouseState);
                    var index = this.HitTestText(position);
                    if (index == -1)
                    {
                        if (position.Y < TextMargin.Y)
                        {
                            VerticalScrollPosition = m_VScroll;
                            index = m_Lines[m_VScroll].StartIndex;
                        }
                        else
                        {
                            index = m_SBuilder.Length;
                        }
                    }
                    Control_SetSelectionEnd(
                        Control_GetSelectableIndex(index, -1)
                    );

                    m_DrawDirty = true;
                    Invalidate();
                }
            }
        }

        #region Text Control

        private int m_MaxLines = int.MaxValue;
        private int m_MaxChars = int.MaxValue;

        [UIAttribute("lines")]
        public int MaxLines
        {
            get{ return m_MaxLines; }
            set { m_MaxLines = (value<0)?int.MaxValue:value;  }
        }
        [UIAttribute("capacity")]
        public int MaxChars
        {
            get { return m_MaxChars; }
            set { m_MaxChars = value; }
        }

        /// <summary>
        /// Makes sure that the text does not overflow max lines
        /// or max chars
        /// </summary>
        private void Control_ValidateText()
        {
            if (m_SBuilder.Length > MaxChars)
            {
                m_SBuilder.Remove(MaxChars, m_SBuilder.Length - MaxChars);
            }

            var lines = m_SBuilder.ToString().Split(new string[] { "\n" }, StringSplitOptions.None).ToList();
            if (lines.Count > MaxLines)
            {
                //remove newlines on last line
                while (lines.Count > MaxLines && lines.Count >= 1)
                {
                    lines[MaxLines - 1] += lines[MaxLines].Replace("\r", "");
                    lines.RemoveAt(MaxLines);
                }
                m_SBuilder = new StringBuilder(String.Join("\n", lines));
            }

            SelectionStart = Math.Min(m_SBuilder.Length, SelectionStart);
            SelectionEnd = Math.Min(m_SBuilder.Length, SelectionEnd);
        }

        /// <summary>
        /// Handles using arrow keys to move the selection end
        /// </summary>
        /// <param name="deltaX"></param>
        private void Control_MoveSelection(int deltaX, int deltaY)
        {
            if (SelectionEnd == -1)
            {
                /** Currently have no selection range **/
                SelectionEnd = Control_GetSelectionStart();
            }

            var newIndex = SelectionEnd + deltaX;
            if (deltaY != 0)
            {
                /** Up / down a line **/
                newIndex = Control_GetIndexForLineAdjusment(newIndex, deltaY);
            }

            Control_SetSelectionEnd(
                Control_GetSelectableIndex(
                   newIndex, deltaX
                )
            );

            Control_ScrollTo(SelectionEnd);
            m_DrawDirty = true;
            Invalidate();
        }

        /// <summary>
        /// Handles using arrow keys to move the selection start
        /// </summary>
        /// <param name="deltaX"></param>
        private void Control_MoveCursor(int deltaX, int deltaY)
        {
            /** If we have a selection, deselect **/
            if (SelectionEnd != -1)
            {
                SelectionStart = SelectionEnd;
                SelectionEnd = -1;
            }

            var newIndex = Control_GetSelectionStart() + deltaX;
            if (deltaY != 0)
            {
                /** Up / down a line **/
                newIndex = Control_GetIndexForLineAdjusment(newIndex, deltaY);
            }

            Control_SetSelectionStart(
                Control_GetSelectableIndex(
                   newIndex, deltaX
                )
            );

            Control_ScrollTo(Control_GetSelectionStart());
            m_DrawDirty = true;
            Invalidate();
        }

        /// <summary>
        /// Takes in a string index and adjusts it
        /// if that index lies between non-breakable chars
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private int Control_GetSelectableIndex(int index, int delta)
        {
            if (index < m_SBuilder.Length && index > 1 && m_SBuilder[index - 1] == '\r' && m_SBuilder[index] == '\n')
            {
                return index + delta;
            }
            return index;
        }

        /// <summary>
        /// Gets the absolute selection start position
        /// </summary>
        /// <returns></returns>
        private int Control_GetSelectionStart()
        {
            if (SelectionStart == -1)
            {
                return m_SBuilder.Length;
            }
            return SelectionStart;
        }

        /// <summary>
        /// Sets the selection start index
        /// </summary>
        /// <param name="val"></param>
        private void Control_SetSelectionStart(int val)
        {
            if (val < 0) { val = 0; }
            if (val >= m_SBuilder.Length)
            {
                val = -1;
            }
            SelectionStart = val;
        }

        private void Control_SetSelectionEnd(int val)
        {
            if (val < 0) { val = 0; }
            if (val >= m_SBuilder.Length)
            {
                val = m_SBuilder.Length;
            }
            if (val == Control_GetSelectionStart())
            {
                /** Selection size = 0, act as if there is no selection **/
                val = -1;
            }
            SelectionEnd = val;
        }

        /// <summary>
        /// Calculate & return the index that should be selected
        /// if the cursor is moved up or down by the value deltaY
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="deltaY"></param>
        private int Control_GetIndexForLineAdjusment(int startIndex, int deltaY)
        {
            var myLine = GetLineForIndex(startIndex);
            if (myLine != null)
            {
                var newLineNum = myLine.LineNumber + deltaY;
                if (newLineNum >= 0 && newLineNum < m_Lines.Count)
                {
                    /** Its a valid line **/
                    var lineOffset = startIndex - myLine.StartIndex;
                    var newLine = m_Lines[newLineNum];

                    return newLine.StartIndex + Math.Min(newLine.Text.Length, lineOffset);
                }
            }
            return startIndex;
        }

        /// <summary>
        /// Sets the scroll position such that the given index
        /// is visible on screen
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private void Control_ScrollTo(int index)
        {
            var line = GetLineForIndex(index);
            if (line != null)
            {
                if (line.LineNumber < m_VScroll)
                {
                    /** Scroll up **/
                    VerticalScrollPosition -= (m_VScroll - line.LineNumber);
                }
                if (line.LineNumber >= m_VScroll + m_NumVisibleLines)
                {
                    VerticalScrollPosition += line.LineNumber - (m_VScroll + m_NumVisibleLines - 1);
                }
            }
        }

        #endregion

        #region Text Rendering Calculation

        private bool m_DrawDirty = false;
        private List<ITextDrawCmd> m_DrawCmds = new List<ITextDrawCmd>();
        private List<UITextEditLine> m_Lines = new List<UITextEditLine>();
        private Vector2 m_CursorPosition = Vector2.Zero;
        private float m_LineHeight;
        private int m_NumVisibleLines;

        public int TotalLines
        {
            get
            {
                return m_Lines.Count;
            }
        }

        /// <summary>
        /// When the text / scroll / highlight changes we need to
        /// re-compute how we are going to draw this text field
        /// </summary>
        public void ComputeDrawingCommands()
        {
            m_DrawCmds.Clear();
            m_DrawDirty = false;

            /**
             * Split the text into lines using manual lines
             * breaks and word wrap
             */
            string txt = null;

            if (m_Password){
                /** Use * instead **/
                txt = "";
                for(int i=0; i < m_SBuilder.Length; i++){
                    txt += "*";
                }
            }
            else
            {
                txt = CurrentTextParsed;
            }

            var lineWidth = m_Width - (TextMargin.Left + TextMargin.Height);
            m_LineHeight = TextStyle.MeasureString("W").Y + TextStyle.LineHeightModifier;

            var bbCommands = BBCodeEnabled ? CodeParser.Commands : new List<BBCodeCommand>();
            var bbIndex = 0;

            m_Lines.Clear();
            //txt = txt.Replace("\r", "");
            var words = txt.Split(' ').ToList();
	        var spaceWidth = TextStyle.MeasureString(" ").X;

            /**
             * Modify the array to make manual line breaks their own segment
             * in the array
             */
            var newWordsArray = TextRenderer.ExtractLineBreaks(words);
            TextRenderer.CalculateLines(m_Lines, newWordsArray, TextStyle, lineWidth, spaceWidth, new Vector2(), m_LineHeight);

            var topLeft = new Vector2(TextMargin.Left, TextMargin.Top);
            var position = topLeft;
            var txtScale = TextStyle.Scale * _Scale;

            m_NumVisibleLines = Math.Max(1, (int)Math.Floor(m_Height / m_LineHeight));
            /** Make sure the current vscroll is valid **/
            VerticalScrollPosition = m_VScroll;

            if (m_Slider != null)
            {
                m_Slider.MaxValue = Math.Max(0, m_Lines.Count - m_NumVisibleLines);
                m_Slider.Value = VerticalScrollPosition;
            }

            var bbColorStack = new Stack<Color>();
            var lastColor = TextStyle.Color;

            var yPosition = topLeft.Y;
            var numLinesAdded = 0;
            var shadowApplied = false;
            for (var i = 0; i < m_Lines.Count - m_VScroll; i++)
            {
                var line = m_Lines[m_VScroll + i];

                var segments = CalculateSegments(line, bbCommands, ref bbIndex);
                var xPosition = topLeft.X;
                segments.ForEach(x => x.Size = TextStyle.MeasureString(x.Text));
                var thisLineWidth = segments.Sum(x => x.Size.X);

                /** Alignment **/
                if (Alignment.HasFlag(TextAlignment.Center))
                {
                    xPosition += (int)Math.Round((lineWidth - thisLineWidth) / 2);
                } else if (Alignment.HasFlag(TextAlignment.Right))
                {
                    xPosition += (int)Math.Round((lineWidth - thisLineWidth));
                }
                line.LineStartX = (int)xPosition;

                foreach (var segment in segments)
                {
                    var segmentSize = segment.Size;
                    var segmentPosition = LocalPoint(new Vector2(xPosition, yPosition));

                    if (segment.Selected)
                    {
                        m_DrawCmds.Add(new TextDrawCmd_SelectionBox
                        {
                            BlendColor = TextStyle.SelectionBoxColor,
                            Texture = TextureGenerator.GetPxWhite(GameFacade.GraphicsDevice),
                            Position = segmentPosition,
                            Scale = new Vector2(segmentSize.X, m_LineHeight) * _Scale
                        });
                    }

                    if (segment.Text.Length > 0)
                    {
                        m_DrawCmds.Add(new TextDrawCmd_Text
                        {
                            Selected = segment.Selected,
                            Text = segment.Text,
                            Style = TextStyle,
                            Position = segmentPosition,
                            Scale = txtScale
                        });
                        xPosition += segmentSize.X;
                    }

                    if (segment.StartCommand != null)
                    {
                        var cmd = segment.StartCommand;
                        switch (cmd.Type)
                        {
                            case BBCodeCommandType.color:
                                if (cmd.Close)
                                {
                                    //pop a color off our stack
                                    if (bbColorStack.Count > 0)
                                    {
                                        lastColor = bbColorStack.Pop();
                                        m_DrawCmds.Add(new TextDrawCmd_Color(TextStyle, lastColor));
                                    }
                                }
                                else
                                {
                                    bbColorStack.Push(lastColor);
                                    lastColor = cmd.ParseColor();
                                    m_DrawCmds.Add(new TextDrawCmd_Color(TextStyle, lastColor));
                                }
                                break;
                            case BBCodeCommandType.s:
                                if (cmd.Close)
                                {
                                    m_DrawCmds.Add(new TextDrawCmd_Shadow(TextStyle, false));
                                    shadowApplied = false;
                                }
                                else
                                {
                                    m_DrawCmds.Add(new TextDrawCmd_Shadow(TextStyle, true));
                                    shadowApplied = true;
                                }
                                break;
                            case BBCodeCommandType.emoji:
                                if (segment.BBCatchup) break;
                                m_DrawCmds.Add(new TextDrawCmd_Emoji(TextStyle, cmd.Parameter, LocalPoint(new Vector2(xPosition, yPosition)), _Scale));
                                break;
                        }
                    }

                }



                yPosition += m_LineHeight;
                position.Y += m_LineHeight;

                numLinesAdded++;
                if (numLinesAdded >= m_NumVisibleLines)
                {
                    break;
                }
            }
            if (shadowApplied) m_DrawCmds.Add(new TextDrawCmd_Shadow(TextStyle, false));

            while (bbColorStack.Count > 0)
            {
                lastColor = bbColorStack.Pop();
                m_DrawCmds.Add(new TextDrawCmd_Color(TextStyle, lastColor));
            }

            /** No cursor in read only mode **/
            if (m_IsReadOnly) {
                m_DrawCmds.ForEach(x => x.Init()); 
                return;
            }

            var start = Control_GetSelectionStart();
            var cursorLine = GetLineForIndex(start);
            if (cursorLine != null && cursorLine.LineNumber >= m_VScroll && cursorLine.LineNumber < m_VScroll + m_NumVisibleLines)
            {
                var prefix = start - cursorLine.StartIndex;
                var cursorPosition = new Vector2(cursorLine.LineStartX, topLeft.Y + ((cursorLine.LineNumber - m_VScroll) * m_LineHeight));

                if (prefix > 0)
                {
                    if (prefix > cursorLine.Text.Length - 1)
                    {
                        cursorPosition.X += cursorLine.LineWidth;
                    }
                    else
                    {
                        cursorPosition.X += TextStyle.MeasureString(cursorLine.Text.Substring(0, prefix)).X;
                    }
                }


                m_DrawCmds.Add(new TextDrawCmd_Cursor
                {
                    Scale = new Vector2(_Scale.X, m_LineHeight * _Scale.Y),
                    Position = LocalPoint(cursorPosition),
                    Texture = TextureGenerator.GetPxWhite(GameFacade.GraphicsDevice),
                    Color = TextStyle.CursorColor
                });
            }

            m_DrawCmds.ForEach(x => x.Init());
        }

        /// <summary>
        /// Creates a list of segments to split the line into
        /// in order to draw selection boxes
        /// </summary>
        /// <returns></returns>
        protected List<UITextEditLineSegment> CalculateSegments(UITextEditLine line, List<BBCodeCommand> bbcmds, ref int bbind)
        {
            var result = new List<UITextEditLineSegment>();

            var points = new List<Tuple<int, BBCodeCommand>>();

            var hasSelection = (SelectionEnd != -1);
            var start = SelectionStart == -1 ? m_SBuilder.Length : SelectionStart;
            var end = SelectionEnd == -1 ? m_SBuilder.Length : SelectionEnd;
            if (start > end)
            {
                var temp = start;
                start = end;
                end = temp;
            }
            var lineStart = line.StartIndex;
            var lineEnd = lineStart + line.Text.Length;

            var selected = hasSelection && (start < lineStart && end >= lineStart);
            var lastMod = 0;
            while (true)
            {
                //check for selection first
                var nextBB = (bbind == bbcmds.Count) ? lineEnd : bbcmds[bbind].Index;
                if (hasSelection)
                {
                    if (!selected)
                    {
                        //look for selection start. is it before our next bbcommand?
                        if (start >= lastMod+lineStart && start <= nextBB)
                        {
                            selected = true;
                            if (start != nextBB || nextBB == lineEnd)
                            {
                                //
                                result.Add(new UITextEditLineSegment
                                {
                                    Selected = false, //prefix before we select the text
                                    Text = line.Text.Substring(lastMod, (start - lineStart) - lastMod),
                                });
                            }
                            lastMod = start - lineStart;
                        }
                    }
                    if (selected)
                    {
                        //look for selection end. is it before our next bbcommand?
                        var selE = end - lineStart;
                        if (end >= lastMod+lineStart && end <= nextBB)
                        {
                            selected = false;
                            if (end != nextBB || nextBB == lineEnd)
                            {
                                result.Add(new UITextEditLineSegment
                                {
                                    Selected = true, //the remainder of the selected string
                                    Text = line.Text.Substring(lastMod, (end - lineStart) - lastMod),
                                });
                            }
                            lastMod = end - lineStart;
                        }
                    }
                }

                if (nextBB < lineEnd)
                {
                    //this bbcmd happens on this line (or before we even drew anything (scroll))
                    //
                    if (bbcmds[bbind].Index - lineStart < 0)
                    {
                        result.Add(new UITextEditLineSegment
                        {
                            Selected = selected,
                            Text = "",
                            BBCatchup = true,
                            StartCommand = bbcmds[bbind++]
                        });
                    }
                    else
                    {
                        result.Add(new UITextEditLineSegment
                        {
                            Selected = selected,
                            Text = line.Text.Substring(lastMod, (bbcmds[bbind].Index - lineStart) - lastMod),
                            StartCommand = bbcmds[bbind++]
                        });
                        lastMod = (bbcmds[bbind - 1].Index - lineStart);
                    }
                } else
                {
                    //remainder of the line
                    result.Add(new UITextEditLineSegment
                    {
                        Selected = selected,
                        Text = line.Text.Substring(lastMod, (lineEnd - lineStart) - lastMod),
                    });
                    break;
                }
            }
            return result;

            
        }

        /// <summary>
        /// Render
        /// </summary>
        /// <param name="batch"></param>
        public override void Draw(UISpriteBatch batch)
        {
            if (m_Slider != null) m_Slider.Visible = Visible;
            if (!Visible) { return; }

            if (m_DrawDirty)
            {
                ComputeDrawingCommands();
            }

            /** Can have a text box without a background **/
            if (m_BackgroundTex != null && NineSliceMargins != null)
            {
                NineSliceMargins.DrawOnto(batch, this, m_BackgroundTex, m_Width, m_Height);
            }
            if (m_BackgroundTexRef != null)
            {
                m_BackgroundTexRef.Draw(batch, this, 0, 0, m_Width, m_Height);
            }

            /**
             * Draw border
             */
            if (m_frameBlinkOn && m_frameBlink)
            {
                DrawingUtils.DrawBorder(batch, LocalRect(0, 0, m_Width, m_Height), 1, m_FrameTexture, m_FrameColor);
            }
            
            /**
             * Draw text
             */
            foreach (var cmd in m_DrawCmds)
            {
                cmd.Draw(this, batch);
            }
        }

        #endregion

        #region Text Layout

        private int m_VScroll;
        public int VerticalScrollPosition
        {
            get
            {
                return m_VScroll;
            }
            set
            {
                var oldValue = m_VScroll;
                m_VScroll = value;
                if (m_VScroll < 0)
                {
                    m_VScroll = 0;
                }
                if (m_VScroll > VerticalScrollMax)
                {
                    m_VScroll = VerticalScrollMax;
                }
                if (oldValue != m_VScroll)
                {
                    m_DrawDirty = true;
                    Invalidate();
                }
            }
        }

        public int VerticalScrollMax
        {
            get
            {
                return Math.Max(0, m_Lines.Count - m_NumVisibleLines);
            }
        }

        public UITextEditLine GetLineForIndex(int index)
        {
            if (index >= m_SBuilder.Length)
            {
                return m_Lines.LastOrDefault();
            }

            foreach (var line in m_Lines)
            {
                if (index >= line.StartIndex && index < line.StartIndex + line.Text.Length + line.WhitespaceSuffix)
                {
                    return line;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns which character index would be hit
        /// by the given mouse coordinates. The coords should be
        /// in local coords.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public int HitTestText(Vector2 point)
        {
            var yPosition = (float)TextMargin.Y;

            for (var i = 0; i < m_Lines.Count - m_VScroll; i++)
            {
                var line = m_Lines[m_VScroll + i];

                if (point.Y >= yPosition && point.Y <= yPosition + m_LineHeight)
                {
                    /** Its this line! **/
                    /** Now we need to work out what the X coordinate relates to **/
                    var roughEst =
                        Math.Round(
                            ((point.X - line.LineStartX) / line.LineWidth) * line.Text.Length
                        );
                    var index = Math.Max(0, roughEst);
                    index = Math.Min(index, line.Text.Length);

                    return (int)line.StartIndex + (int)index;
                }
                yPosition += m_LineHeight;
            }
            return -1;
        }


        #endregion

        public override void CalculateMatrix()
        {
            base.CalculateMatrix();
            m_DrawDirty = true;
            Invalidate();
        }

        #region ITextControl Members

        bool ITextControl.DrawCursor
        {
            get
            {
                return IsFocused && m_cursorBlink;
            }
        }

        #endregion












        #region Scrollbar

        [UIAttribute("scrollbarImage")]
        public Texture2D ScrollbarImage { get; set; }

        [UIAttribute("scrollbarGutter")]
        public int ScrollbarGutter { get; set; }

        public UISlider Slider
        {
            get { return m_Slider; }
        }

        private UISlider m_Slider;

        public void AttachSlider(UISlider slider)
        {
            m_Slider = slider;
            m_Slider.OnChange += new ChangeDelegate(m_Slider_OnChange);
        }

        public void InitDefaultSlider()
        {
            m_Slider = new UISlider();
            m_Slider.Texture = ScrollbarImage;
            AttachSlider(m_Slider);
            PositionChildSlider();
            Parent.Add(m_Slider);
        }

        public void PositionChildSlider()
        {
            m_Slider.Position = this.Position + new Vector2(this.Width + ScrollbarGutter, 0);
            m_Slider.SetSize(1, this.Height);
        }

        void m_Slider_OnChange(UIElement element)
        {
            VerticalScrollPosition = (int)((UISlider)element).Value;
        }

        #endregion
    }



    public class UITextEditLine
    {
        public int StartIndex;
        public int LineNumber;
        public string Text;
        public float LineWidth;
        public float LineHeight;
        public int LineStartX;

        public int WhitespaceSuffix;
    }

    public class UITextEditLineSegment
    {
        public string Text;
        public bool Selected;
        public bool BBCatchup;
        public BBCodeCommand StartCommand;
        public Vector2 Size;
    }


    public enum UITextEditMode
    {
        Editor,
        ReadOnly
    }

    public delegate void KeyPressDelegate(UIElement element);

    public interface ITextDrawCmd
    {
        void Draw(UIElement ui, SpriteBatch batch);
        void Init();
    }

    public class TextDrawCmd_Color : ITextDrawCmd
    {
        public TextStyle Style;
        public Color ReplColor;

        public TextDrawCmd_Color(TextStyle style, Color color)
        {
            Style = style;
            ReplColor = color;
        }

        #region ITextDrawCmd Members
        public virtual void Draw(UIElement ui, SpriteBatch batch)
        {
            Style.Color = ReplColor;
        }

        public void Init()
        {
        }
        #endregion
    }

    public class TextDrawCmd_Shadow : ITextDrawCmd
    {
        public TextStyle Style;
        public bool ReplShadow;

        public TextDrawCmd_Shadow(TextStyle style, bool shad)
        {
            Style = style;
            ReplShadow = shad;
        }

        #region ITextDrawCmd Members
        public virtual void Draw(UIElement ui, SpriteBatch batch)
        {
            Style.Shadow = ReplShadow;
        }

        public void Init()
        {
        }
        #endregion
    }



    public class TextDrawCmd_Emoji : ITextDrawCmd, INormalTextCmd
    {
        public TextStyle Style { get; set; }
        public Texture2D EmojiTarget;
        public Vector2 Position { get; set; }
        public Rectangle Slice;
        public Vector2 Scale;
        public bool Shadow;

        public TextDrawCmd_Emoji(TextStyle style, string emojiID, Vector2 position, Vector2 scale)
        {
            Position = position;
            Style = style;
            var emoj = GameFacade.Emojis.GetEmoji(emojiID);
            EmojiTarget = emoj.Item1;
            Slice = emoj.Item2;
            Scale = scale;
        }

        #region ITextDrawCmd Members
        public virtual void Draw(UIElement ui, SpriteBatch batch)
        {
            batch.Draw(
                EmojiTarget, 
                Position.ToPoint().ToVector2() + new Vector2(1, 0), 
                Slice, 
                (Shadow?Color.Black:Color.White) * (Style.Color.A / 255f), 
                0f, 
                Vector2.Zero, 
                Scale * (Style.Size / 12f), 
                SpriteEffects.None, 
                0f);
        }

        public void Init()
        {
        }
        #endregion
    }

    public interface INormalTextCmd
    {
        TextStyle Style { get; set; }
        Vector2 Position { get; set; }
    }

    public class TextDrawCmd_Text : ITextDrawCmd, INormalTextCmd
    {
        public bool Selected;
        public Vector2 Position { get; set; }
        public string Text;
        public TextStyle Style { get; set; }
        public Vector2 Scale;

        public void Init()
        {
            //Position.Y += Style.BaselineOffset;
        }

        #region ITextDrawCmd Members
        public virtual void Draw(UIElement ui, SpriteBatch batch)
        {
            if (Style.VFont != null)
            {
                batch.End();
                Matrix? mat = null;
                var uib = (batch as UISpriteBatch);
                if (uib != null && uib.BatchMatrixStack.Count > 0)
                    mat = uib.BatchMatrixStack.Peek();

                if (Selected)
                {
                    Style.VFont.Draw(batch.GraphicsDevice, Text, Position, Style.SelectedColor, Scale, mat);
                }
                else
                {
                    if (Style.Shadow)
                        Style.VFont.Draw(batch.GraphicsDevice, Text, Position + new Vector2(0, FSOEnvironment.DPIScaleFactor), Color.Black, Scale, mat);
                    Style.VFont.Draw(batch.GraphicsDevice, Text, Position, Style.Color, Scale, mat);
                }

                if (mat != null)
                    batch.Begin(transformMatrix: mat, rasterizerState: RasterizerState.CullNone);
                else
                    batch.Begin(rasterizerState: RasterizerState.CullNone);
            }
            else
            {
                if (Selected)
                {
                    batch.DrawString(Style.SpriteFont, Text, Position, Style.SelectedColor, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
                }
                else
                {
                    if (Style.Shadow)
                        batch.DrawString(Style.SpriteFont, Text, Position + new Vector2(0, 1), Color.Black, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
                    batch.DrawString(Style.SpriteFont, Text, Position, Style.Color, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
                }
            }
        }
        #endregion
    }

    public interface ITextControl
    {
        bool DrawCursor { get; }
    }

    public class TextDrawCmd_Cursor : ITextDrawCmd
    {
        public Vector2 Position;
        public Texture2D Texture;
        public Color Color;
        public Vector2 Scale;

        public void Init()
        {
        }

        public void Draw(UIElement ui, SpriteBatch batch)
        {
            if (((ITextControl)ui).DrawCursor)
            {
                batch.Draw(Texture, Position, null, Color, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
            }
        }
    }


    public class TextDrawCmd_SelectionBox : ITextDrawCmd
    {
        public Texture2D Texture;
        public Vector2 Position;
        public Vector2 Scale;
        public Color BlendColor;

        public void Init()
        {
        }

        public void Draw(UIElement ui, SpriteBatch batch)
        {
            batch.Draw(Texture, Position, null, BlendColor, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
        }
    }

}
