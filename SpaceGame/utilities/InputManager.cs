using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SpaceGame.utility
{
    public class InputManager
    {
        #region constants
        const int WHEEL_UNITS_PER_SCROLL = 10;
        #endregion
        #region fields
        #region constants
        const float c_thumbstickDeadZone = 0.1f;
        //how far to push thumstick to select something
        const float c_thumbstickSelectThreshold = 0.5f;
        #endregion
        #region members
        KeyboardState _previousKeyboardState;
        KeyboardState _currentKeyboardState;
        MouseState _previousMouseState;
        MouseState _currentMouseState;
        bool _gamePadConnected;
        bool _gamePadInverted;
        GamePadState _previousGamepadState;
        GamePadState _currentGamepadState;
        Vector2 _gamepadAimOrigin;
        //current scrolls toward next scroll event
        //- for scroll down, + for scroll up
        int _scrollCounter;
        bool _scrollUp, _scrollDown;
        Vector2 _cameraOffset;  //use to calculate absolute mouse position
        #endregion

        #region properties

        //Input Requests
        public bool MoveLeft
        {
            get { return _currentKeyboardState.IsKeyDown(Keys.A) || _currentGamepadState.ThumbSticks.Left.X < -c_thumbstickDeadZone; }
        }
        public bool MoveRight 
        { 
            get {return _currentKeyboardState.IsKeyDown(Keys.D) || _currentGamepadState.ThumbSticks.Left.X > c_thumbstickDeadZone;} 
        }
        public bool MoveDown 
        { 
            get {return _currentKeyboardState.IsKeyDown(Keys.S) || _currentGamepadState.ThumbSticks.Left.Y < -c_thumbstickDeadZone;}
        }
        public bool MoveUp 
        { 
            get {return _currentKeyboardState.IsKeyDown(Keys.W) || _currentGamepadState.ThumbSticks.Left.Y > c_thumbstickDeadZone;}
        }

        public bool ScrollUp { get { return _scrollUp; } }
        public bool ScrollDown { get { return _scrollDown; } }

        /// <summary>
        /// Request to move selector left (use for menus)
        /// </summary>
        public bool SelectLeft
        {
            get { return keyTapped(Keys.A) || keyTapped(Keys.Left)
                || buttonTapped(Buttons.DPadLeft); }
        }
        /// <summary>
        /// Request to move selector right (use for menus)
        /// </summary>
        public bool SelectRight 
        { 
            get { return keyTapped(Keys.D) || keyTapped(Keys.Right)
                || buttonTapped(Buttons.DPadRight); }
        }
        /// <summary>
        /// Request to move selector down (use for menus)
        /// </summary>
        public bool SelectDown 
        { 
            get { return keyTapped(Keys.S) || keyTapped(Keys.Down)
                || buttonTapped(Buttons.DPadDown); }
        }
        /// <summary>
        /// Request to move selector up (use for menus)
        /// </summary>
        public bool SelectUp 
        { 
            get { return keyTapped(Keys.W) || keyTapped(Keys.Up)
                || buttonTapped(Buttons.DPadUp); }
        }

        /// <summary>
        /// Confirmation button pressed (use for menus)
        /// </summary>
        public bool Confirm 
        {
            get { return keyTapped(Keys.Enter) || keyTapped(Keys.Space)
                || buttonTapped(Buttons.A) || buttonTapped(Buttons.Start); }
        }
        /// <summary>
        /// Cancellation/back button pressed (use for menus)
        /// </summary>
        public bool Cancel 
        {
            get { return keyTapped(Keys.Escape) || keyTapped(Keys.Back)
                || buttonTapped(Buttons.B) || buttonTapped(Buttons.Back); }
        }
        /// <summary>
        /// Get requested direction based on movement keys (normalized)
        /// </summary>
        public Vector2 MoveDirection
        {
            get 
            {
                Vector2 direction = Vector2.Zero;

                if (MoveDown)
                    direction.Y = 1;
                else if (MoveUp)
                    direction.Y = -1;
                if (MoveRight)
                    direction.X = 1;
                else if (MoveLeft)
                    direction.X = -1;

                if (direction.Length() > 0)
                    direction.Normalize();
                return direction;
            }
        }
        public bool FirePrimary 
        {
            get { return _currentMouseState.LeftButton == ButtonState.Pressed || _currentGamepadState.IsButtonDown(Buttons.RightTrigger); }
        }
        public bool FireSecondary
        {
            get { return _currentMouseState.RightButton == ButtonState.Pressed || _currentGamepadState.IsButtonDown(Buttons.LeftTrigger); }
        }
        public bool UseItem { get { return keyTapped(Keys.Q) || buttonTapped(Buttons.A); } }
        public bool TriggerGadget1
        { 
            get {return keyTapped(Keys.LeftShift) || buttonTapped(Buttons.LeftShoulder);}
        }
        public bool TriggerGadget2 
        { 
            get {return keyTapped(Keys.Space) || buttonTapped(Buttons.RightShoulder);}
        }
        public Vector2 AbsoluteMousePos
        {
            get 
            { 
                return RelativeMousePos + _cameraOffset; 
            }
        }
        public Vector2 RelativeMousePos
        {
            get 
            {
                if (_gamePadConnected)
                {
                    Vector2 screenVector = new Vector2(Game1.SCREENWIDTH, Game1.SCREENHEIGHT);
                    Vector2 aimVector = _currentGamepadState.ThumbSticks.Right;
                    if (!_gamePadInverted) { aimVector.Y *= -1; }
                    return aimVector * screenVector / 2 + _gamepadAimOrigin;
                }
                return new Vector2(_currentMouseState.X, _currentMouseState.Y); 
            }
        }
        public bool Exit
        {
            get { return _currentKeyboardState.IsKeyDown(Keys.Escape); }
        }
        //Change Item Request
        public int SelectItemNum
        {
            get
            {
                if (keyTapped(Keys.NumPad1) || keyTapped(Keys.D1))
                {
                    return 1;
                }
                else if (keyTapped(Keys.NumPad2) || keyTapped(Keys.D2))
                {
                    return 2;
                }
                else if (keyTapped(Keys.NumPad3) || keyTapped(Keys.D3))
                {
                    return 3;
                }
                else if (keyTapped(Keys.NumPad4) || keyTapped(Keys.D4))
                {
                    return 4;
                }
                else if (keyTapped(Keys.NumPad5) || keyTapped(Keys.D5))
                {
                    return 5;
                }
                else if (keyTapped(Keys.NumPad6) || keyTapped(Keys.D6))
                {
                    return 6;
                }
                else
                {
                    return -1;
                }
            }
        }
                         

        public bool fCycle
        {
            get { return keyTapped(Keys.Q); }
        }

        public bool bCycle
        {
            get { return keyTapped(Keys.E); }
        }
        /// <summary>
        /// return true if debug key (B) is pressed
        /// </summary>
        public bool DebugKey    
        {
            get { return _currentKeyboardState.IsKeyDown(Keys.B); }
        }
        #endregion
        #endregion

        #region methods
        public InputManager()
        {
            _gamePadConnected = GamePad.GetState(PlayerIndex.One).IsConnected;
        }

        public void SetCameraOffset(Vector2 offset)
        {
            _cameraOffset = offset;
        }

        /// <summary>
        /// Recieve and process input
        /// </summary>
        /// <param name="gamepadAimOrigin">Point from which to calculate thumstick aim position relative to screen view</param>
        public void Update(Vector2 gamepadAimOrigin)
        {
            _previousKeyboardState = _currentKeyboardState;
            _currentKeyboardState = Keyboard.GetState();
            _previousMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();
            _previousGamepadState = _currentGamepadState;
            _currentGamepadState = GamePad.GetState(PlayerIndex.One);
            _gamepadAimOrigin = gamepadAimOrigin;
            _scrollCounter += (_currentMouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue);
            _scrollDown = false;
            _scrollUp = false;
            if (_scrollCounter > WHEEL_UNITS_PER_SCROLL)
            {
                _scrollUp = true;
                _scrollCounter -= WHEEL_UNITS_PER_SCROLL;
            }
            else if (_scrollCounter < -WHEEL_UNITS_PER_SCROLL)
            {
                _scrollDown = true;
                _scrollCounter += WHEEL_UNITS_PER_SCROLL;
            }
        }

        private bool keyTapped(Keys key)
        {
            return _currentKeyboardState.IsKeyDown(key)
                && _previousKeyboardState.IsKeyUp(key);
        }

        private bool buttonTapped(Buttons button)
        {
            return _currentGamepadState.IsButtonDown(button)
                && _previousGamepadState.IsButtonUp(button);
        }

        /// <summary>
        /// Return the integer of the numkey between 1 and 6 pressed
        /// Return -1 if no numkey pressed
        /// </summary>
        /// <returns></returns>
        public int NumKey()
        {
            foreach (Keys key in _currentKeyboardState.GetPressedKeys())
            {
                if (keyTapped(key) && Keys.NumPad0 <= key && key <= Keys.NumPad6)
                    return (int)(key - Keys.NumPad0);
                if (keyTapped(key) && Keys.D0 <= key && key <= Keys.D6)
                    return (int)(key - Keys.D0);
            }
            return -1;
        }
        #endregion
    }
}
