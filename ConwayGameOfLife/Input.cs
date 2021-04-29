using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace ConwayGameOfLife
{
    internal static class Input
    {
        //enum mouseButtons { noll, ett};
        private static KeyboardState currentKeyState;

        public static bool mouseClickingToAlive = true;
        private static KeyboardState previousKeyState;

        private static GamePadState currentGamePadState;
        private static GamePadState previousGamePadState;

        private static MouseState currentMouseState;
        private static MouseState previousMouseState;

        private static int currentscrollWheelValue;
        private static int previousscrollWheelValue;
        public static int clampedScrollWheelValue { private set; get; }
        public static int differenceScrollWheelValue { private set; get; }
        public static Vector2 directional { private set; get; }
        public static Vector2 normalizedDirectional { private set; get; }

        public static KeyboardState GetState()
        {
            previousKeyState = currentKeyState;
            currentKeyState = Microsoft.Xna.Framework.Input.Keyboard.GetState();

            previousGamePadState = currentGamePadState;
            currentGamePadState = Microsoft.Xna.Framework.Input.GamePad.GetState(PlayerIndex.One);

            previousMouseState = currentMouseState;
            currentMouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();

            previousscrollWheelValue = currentscrollWheelValue;
            currentscrollWheelValue = Mouse.GetState().ScrollWheelValue;

            differenceScrollWheelValue = currentscrollWheelValue - previousscrollWheelValue;
            if (Math.Abs(differenceScrollWheelValue) > 10)
            {
            }

            clampedScrollWheelValue += differenceScrollWheelValue;
            clampedScrollWheelValue = MathHelper.Clamp(clampedScrollWheelValue, -720, 1680);
            //Vector2 input = new Vector2();
            directional = new Vector2();
            directional += new Vector2(0, GamePad.GetState(PlayerIndex.One).DPad.Down == ButtonState.Pressed ? 1 : 0);
            directional += new Vector2(0, GamePad.GetState(PlayerIndex.One).DPad.Up == ButtonState.Pressed ? -1 : 0);
            directional += new Vector2(GamePad.GetState(PlayerIndex.One).DPad.Left == ButtonState.Pressed ? -1 : 0, 0);
            directional += new Vector2(GamePad.GetState(PlayerIndex.One).DPad.Right == ButtonState.Pressed ? 1 : 0, 0);

            directional += new Vector2(GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X, -GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y);

            directional += new Vector2(0, GetButton(Keys.Down) || GetButton(Keys.S) ? 1 : 0);
            directional += new Vector2(0, GetButton(Keys.Up) || GetButton(Keys.W) ? -1 : 0);
            directional += new Vector2(GetButton(Keys.Left) || GetButton(Keys.A) ? -1 : 0, 0);
            directional += new Vector2(GetButton(Keys.Right) || GetButton(Keys.D) ? 1 : 0, 0);
            directional = new Vector2(Math.Clamp(directional.X, -1, 1), Math.Clamp(directional.Y, -1, 1));
            normalizedDirectional = AdvancedMath.ClampMagnitude(directional, 1);
            return currentKeyState;
        }

        public static bool GetButton(Keys key)
        {
            return currentKeyState.IsKeyDown(key);
        }

        public static bool GetButtonDown(Keys key)
        {
            return currentKeyState.IsKeyDown(key) && !previousKeyState.IsKeyDown(key);
        }

        public static bool GetButtonUp(Keys key)
        {
            return !currentKeyState.IsKeyDown(key) && previousKeyState.IsKeyDown(key);
        }

        public static bool GetButton(Buttons key)
        {
            return currentGamePadState.IsButtonDown(key);
        }

        public static bool GetButtonDown(Buttons key)
        {
            return currentGamePadState.IsButtonDown(key) && !previousGamePadState.IsButtonDown(key);
        }

        public static bool GetButtonUp(Buttons key)
        {
            return !currentGamePadState.IsButtonDown(key) && previousGamePadState.IsButtonDown(key);
        }

        public static bool GetMouseButton(int key)
        {
            if (key == 0)
            {
                return currentMouseState.LeftButton == ButtonState.Pressed;
            }
            if (key == 1)
            {
                return currentMouseState.RightButton == ButtonState.Pressed;
            }
            if (key == 2)
            {
                return currentMouseState.MiddleButton == ButtonState.Pressed;
            }
            return false;
        }

        public static bool GetMouseButtonDown(int key)
        {
            if (key == 0)
            {
                return currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released;
            }
            if (key == 1)
            {
                return currentMouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released;
            }
            if (key == 2)
            {
                return currentMouseState.MiddleButton == ButtonState.Pressed && previousMouseState.MiddleButton == ButtonState.Released;
            }
            return false;
        }

        public static bool GetMouseButtonUp(int key)
        {
            if (key == 0)
            {
                return currentMouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed;
            }
            if (key == 1)
            {
                return currentMouseState.RightButton == ButtonState.Released && previousMouseState.RightButton == ButtonState.Pressed;
            }
            if (key == 2)
            {
                return currentMouseState.MiddleButton == ButtonState.Released && previousMouseState.MiddleButton == ButtonState.Pressed;
            }
            return false;
        }

        public static Vector2 MousePos()
        {
            return new Vector2(currentMouseState.Position.X, currentMouseState.Position.Y);
        }

        public static void ResetScrollWheel()
        {
            clampedScrollWheelValue = 0;
        }
    }
}