using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ConwayGameOfLife
{
    internal class Button
    {
        public Rectangle rectangle { protected set; get; }
        public Texture2D Texture { protected set; get; }
        public string Text { protected set; get; }
        public bool pressed { protected set; get; }

        public Button(Rectangle _rectangle, Texture2D _texture, string _text)
        {
            rectangle = _rectangle;
            Texture = _texture;
            Text = _text;
            pressed = false;
        }

        public Button(Rectangle _rectangle, Texture2D _texture)
        {
            rectangle = _rectangle;
            Texture = _texture;
            Text = string.Empty;
            pressed = false;
        }

        public virtual bool Clicked()
        {
            if (pressed)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    if (rectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y))
                    {
                        pressed = false;
                        return true;
                    }
                }
            }
            if (Input.GetMouseButtonDown(0))
            {
                if (rectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y))
                {
                    pressed = true;
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                pressed = false;
            }
            return false;
        }

        public void setPos(int x, int y)
        {
            rectangle = new Rectangle(x, y, rectangle.Width, rectangle.Height);
            //rectangle.Y = y;
        }

        public void setPos(Vector2 pos)
        {
            rectangle = new Rectangle((int)pos.X, (int)pos.Y, rectangle.Width, rectangle.Height);
            //rectangle.Y = y;
        }

        public void setSize(int width, int height)
        {
            rectangle = new Rectangle(rectangle.X, rectangle.Y, width, height);
            //rectangle.Y = y;
        }

        public void setSize(Vector2 size)
        {
            rectangle = new Rectangle(rectangle.X, rectangle.Y, (int)size.X, (int)size.Y);
            //rectangle.Y = y;
        }

        public void setRectangle(Rectangle _rectangle)
        {
            rectangle = _rectangle;
            //rectangle.Y = y;
        }

        public void Draw(SpriteBatch _spriteBatch, SpriteFont font/*, Vector3 offset*/)
        {
            //setPos(rectangle.X - (int)offset.X, rectangle.Y - (int)offset.Y);
            //_spriteBatch.Draw(texture, new Rectangle(rectangle.X - (int)offset.X, rectangle.Y - (int)offset.Y, rectangle.Width, rectangle.Height), Color.White);
            _spriteBatch.Draw(Texture, rectangle, Color.White);
            Vector2 size = font.MeasureString(Text);
            _spriteBatch.DrawString(font, Text, new Vector2(rectangle.X + rectangle.Width / 2 /*- offset.X*/, rectangle.Y + rectangle.Height / 2 /*- offset.Y*/), Color.White, 0, new Vector2(size.X / 2, size.Y / 2), 2, SpriteEffects.None, 1);
        }
    }
}