using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace ConwayGameOfLife
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private List<Tile> knapparna = new List<Tile>();
        private int widthOfSingleCollisionSquare = 60;
        private int lengthofCollisionSquareX = 0;
        private int lengthofCollisionSquareY = 0;
        private Texture2D debug;
        private Texture2D aliveBox;
        private SpriteFont font;
        private Button fullscreen;
        private bool debugging = false;
        private DataTable dt = new DataTable();
        private Button next;
        private Button play;
        private Button clear;
        private Button reset;
        private Camera camera;
        public static Vector2 screenSize { get; private set; }
        private int tilesPåverkadeSenast = 0;
        private Vector2 position;
        private Vector2 previousMousePos;
        private bool monitorSwitch = false;
        private bool iterating = false;
        private Stopwatch timeSinceIteration;
        private bool playing = false;
        private float timeForIterate = 0.5f;
        private Stopwatch timeTakenToIterate;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            _graphics.IsFullScreen = true;
            _graphics.ApplyChanges();
            camera = new Camera(new Viewport(new Rectangle(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight)));
            screenSize = new Vector2(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            position = new Vector2(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2);
            previousMousePos = camera.ScreenToWorldSpace(new Vector2(Mouse.GetState().X, Mouse.GetState().Y));
            timeSinceIteration = new Stopwatch();
            timeTakenToIterate = new Stopwatch();
            timeSinceIteration.Start();
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            dt.Columns.Add("Index", typeof(int));
            dt.Columns.Add("Xpos", typeof(int));
            dt.Columns.Add("Ypos", typeof(int));
            DataColumn[] keys = new DataColumn[1];
            keys[0] = dt.Columns[0];
            dt.PrimaryKey = keys;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            debug = Content.Load<Texture2D>("Box15");
            aliveBox = Content.Load<Texture2D>("Box15Alive");
            font = Content.Load<SpriteFont>("font");
            fullscreen = new Button(new Rectangle(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2, 160, 40), aliveBox, "Fullscreen");
            fullscreen.setPos(_graphics.PreferredBackBufferWidth - fullscreen.rectangle.Width, 0);
            next = new Button(new Rectangle(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2, 160, 40), aliveBox, "Next");
            next.setPos(_graphics.PreferredBackBufferWidth / 2 - next.rectangle.Width / 2, _graphics.PreferredBackBufferHeight - next.rectangle.Height * 2);
            clear = new Button(new Rectangle(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2, 160, 40), aliveBox, "Clear");
            clear.setPos((int)Math.Round((_graphics.PreferredBackBufferWidth / 2) + clear.rectangle.Width * 0.6f), _graphics.PreferredBackBufferHeight - next.rectangle.Height * 2);
            play = new Button(new Rectangle(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2, 160, 40), aliveBox, "Play");
            play.setPos((int)Math.Round(_graphics.PreferredBackBufferWidth / 2 - next.rectangle.Width * 1.6f), _graphics.PreferredBackBufferHeight - next.rectangle.Height * 2);
            reset = new Button(new Rectangle(0, 0, 160, 40), aliveBox, "Reset");
            reset.setPos(0, 0);
            bool addedX = false;
            int xpos = 4;
            int ypos = 10;
            for (int a = -_graphics.PreferredBackBufferHeight; a < _graphics.PreferredBackBufferHeight * 2; a += widthOfSingleCollisionSquare)
            {
                for (int i = -_graphics.PreferredBackBufferWidth; i < _graphics.PreferredBackBufferWidth * 2; i += widthOfSingleCollisionSquare)
                {
                    knapparna.Add(new Tile(new Rectangle(i, a, widthOfSingleCollisionSquare, widthOfSingleCollisionSquare), debug, aliveBox, (i + _graphics.PreferredBackBufferWidth) / widthOfSingleCollisionSquare, (a + _graphics.PreferredBackBufferHeight) / widthOfSingleCollisionSquare));
                    DataRow tempRow = dt.NewRow();
                    tempRow[0] = knapparna.Count - 1;
                    tempRow[1] = (i + _graphics.PreferredBackBufferWidth) / widthOfSingleCollisionSquare;
                    tempRow[2] = (a + _graphics.PreferredBackBufferHeight) / widthOfSingleCollisionSquare;
                    dt.Rows.Add(tempRow);
                    //dt.Rows.Add(knapparna.Count - 1, i, a);
                    if (!addedX)
                    {
                        lengthofCollisionSquareX++;
                    }
                }
                addedX = true;
                lengthofCollisionSquareY++;
            }
            _graphics.IsFullScreen = false;
            _graphics.ApplyChanges();
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (IsActive)
            {
                if (monitorSwitch)
                {
                    if (Window.ClientBounds.Width > 1)
                    {
                    }
                    screenSize = new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height);
                    fullscreen.setPos(Window.ClientBounds.Width - fullscreen.rectangle.Width, 0);
                    next.setPos(Window.ClientBounds.Width / 2 - next.rectangle.Width / 2, Window.ClientBounds.Height - next.rectangle.Height * 2);
                    clear.setPos((int)Math.Round((Window.ClientBounds.Width / 2) + clear.rectangle.Width * 0.6f), Window.ClientBounds.Height - next.rectangle.Height * 2);
                    play.setPos((int)Math.Round(Window.ClientBounds.Width / 2 - next.rectangle.Width * 1.6f), Window.ClientBounds.Height - next.rectangle.Height * 2);
                    reset.setPos(0, 0);

                    monitorSwitch = false;
                }
                Input.GetState();
                if (Input.GetButtonUp(Buttons.Back) || Input.GetButtonUp(Keys.Escape))
                    Exit();
                if (Input.GetButtonDown(Keys.PrintScreen) || Input.GetButtonDown(Buttons.X))
                {
                    debugging = !debugging;
                    if (debugging)
                    {
                        _graphics.IsFullScreen = false;
                        _graphics.ApplyChanges();
                        screenSize = new Vector2(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
                    }
                }
                bool buttonClicked = false;
                if (next.rectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y) || fullscreen.rectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y) || clear.rectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y) || play.rectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y) || reset.rectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y))
                {
                    buttonClicked = true;
                    if (next.Clicked() && !iterating && !playing)
                    {
                        Iterate();
                    }
                    if (clear.Clicked())
                    {
                        ClearAll();
                        playing = false;
                    }

                    if (reset.Clicked())
                    {
                        ClearAll();
                        position = new Vector2(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2);
                        camera.Zoom = 1;
                        Input.ResetScrollWheel();
                        playing = false;
                    }

                    // TODO: Add your update logic here
                    if (fullscreen.Clicked())
                    {
                        //fullscreen.setPos(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - buttonStart.rectangle.Width, 0);
                        //_graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                        //_graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                        _graphics.IsFullScreen = !_graphics.IsFullScreen;
                        _graphics.ApplyChanges();
                        monitorSwitch = true;
                    }
                    if (play.Clicked())
                    {
                        playing = !playing;
                    }
                }
                if (playing && timeSinceIteration.Elapsed.TotalSeconds > timeForIterate && !iterating)
                {
                    Iterate();
                }
                //camera.UpdateCamera(position);
                //if (Input.GetMouseButtonDown(2))
                //{
                //    muspositionTillWorldPåKlick = camera.ScreenToWorldSpace(new Vector2(Mouse.GetState().X, Mouse.GetState().Y));
                //}

                //Vector2 mousePos = camera.ScreenToWorldSpace(new Vector2(Mouse.GetState().X, Mouse.GetState().Y));
                //Vector3 cameraPos = camera.transform.Translation;
                //Vector2 musDiff = previousMousePos - camera.ScreenToWorldSpace(new Vector2(Mouse.GetState().X, Mouse.GetState().Y));
                //if (WithinRange(muspositionTillWorldPåKlick - mousePos, muspositionTillWorldPåKlick, 20))
                //{
                //}
                //if ((mousePos - muspositionTillWorldPåKlick) + camera.ScreenToWorldSpace(new Vector2(Mouse.GetState().X, Mouse.GetState().Y)) == muspositionTillWorldPåKlick)
                //{
                //}
                if (Input.GetButton(Keys.PageDown))
                {
                    camera.Zoom -= 0.01f;
                }
                else if (Input.GetButton(Keys.PageUp))
                {
                    camera.Zoom += 0.01f;
                }
                camera.Zoom = (float)(Input.clampedScrollWheelValue * 0.001) + 1;

                //if (Input.GetMouseButton(2))
                //{
                //    position += new Vector2((float)musDiff.X, (float)musDiff.Y);
                //}
                //camera.UpdateCamera((Input.GetMouseButton(2) ? position + musDiff : position));
                UpdateMouse(gameTime);
                camera.UpdateCamera((position));
                if (next.rectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y) && !buttonClicked)
                {
                }
                //previousMousePos = mousePos;
                if (!buttonClicked && Input.GetMouseButton(0) && !playing)
                {
                    try
                    {
                        for (int i = 0; i < knapparna.Count; i++)
                        {
                            bool temp = knapparna[i].Clicked(camera.ScreenToWorldSpace(new Vector2(Mouse.GetState().X, Mouse.GetState().Y)));
                            if (temp)
                            {
                                Input.mouseClickingToAlive = knapparna[i].alive;
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        string temp = e.Message;
                    }
                }
                base.Update(gameTime);
            }
        }

        private Vector2 lastMousePosition;
        private bool enableMouseDragging;

        private void UpdateMouse(GameTime gameTime)
        {
            if (Input.GetMouseButtonDown(2) && !enableMouseDragging)
                enableMouseDragging = true;
            else if (Input.GetMouseButtonUp(2) && enableMouseDragging)
                enableMouseDragging = false;

            if (enableMouseDragging)
            {
                Vector2 delta = lastMousePosition - Input.MousePos();

                if (delta != Vector2.Zero)
                {
                    position += delta / camera.Zoom;
                }
            }

            lastMousePosition = Input.MousePos();
        }

        private void ClearAll()
        {
            List<Tile> levandeTiles = knapparna.FindAll(x => x.alive);
            for (int i = 0; i < levandeTiles.Count; i++)
            {
                levandeTiles[i].SetAlive(false);
                levandeTiles[i].UpdateAlive();
            }
        }

        private void Iterate()
        {
            try
            {
                timeSinceIteration.Restart();
                timeTakenToIterate.Restart();
                iterating = true;
                List<Tile> tilesPåverkade = new List<Tile>();
                List<Tile> levandeTiles = knapparna.FindAll(x => x.alive);
                tilesPåverkade.AddRange(levandeTiles);
                for (int i = 0; i < levandeTiles.Count; i++)
                {
                    List<Tile> rutorBredvid = tilesBredvid(levandeTiles[i]);
                    tilesPåverkade.AddRange(rutorBredvid);
                    levandeTiles[i].SetAlive(rutorBredvid.FindAll(x => x.alive).Count);
                    List<Tile> dödaTiles = rutorBredvid.FindAll(x => !x.alive);
                    for (int a = 0; a < dödaTiles.Count; a++)
                    {
                        if (tilesPåverkade.FindAll(o => o == dödaTiles[a]).Count > 1)
                        {
                            dödaTiles.RemoveAt(a);
                        }
                    }
                    tilesPåverkade.AddRange(dödaTiles);
                    for (int a = 0; a < dödaTiles.Count; a++)
                    {
                        List<Tile> rutorBredvidDöda = tilesBredvid(dödaTiles[a]);
                        //tilesPåverkade.AddRange(rutorBredvidDöda);
                        dödaTiles[a].SetAlive(rutorBredvidDöda.FindAll(x => x.alive).Count);
                    }
                }
                for (int i = 0; i < tilesPåverkade.Count; i++)
                {
                    tilesPåverkade[i].UpdateAlive();
                }
                tilesPåverkadeSenast = tilesPåverkade.Count;
                iterating = false;
                timeTakenToIterate.Stop();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Next iteration error: " + e.Message);
                iterating = false;
            }
        }

        private bool WithinRange(Vector2 vector21, Vector2 vector22, float range)
        {
            if (Math.Abs(vector21.X - vector22.X) < range)
            {
                if (Math.Abs(vector21.Y - vector22.Y) < range)
                {
                    return true;
                }
            }
            return false;
        }

        private List<Tile> tilesBredvid(Tile tile)
        {
            List<Tile> returnList = new List<Tile>();
            //Tiles above
            if (tile.ypos > 0)
            {
                if (tile.xpos > 0)
                {
                    returnList.Add(GetTileFromPos(tile.xpos - 1, tile.ypos - 1));
                }
                returnList.Add(GetTileFromPos(tile.xpos, tile.ypos - 1));
                if (tile.xpos < lengthofCollisionSquareX - 1)
                {
                    returnList.Add(GetTileFromPos(tile.xpos + 1, tile.ypos - 1));
                }
            }

            //Tiles on same x row
            if (tile.xpos > 0)
            {
                returnList.Add(GetTileFromPos(tile.xpos - 1, tile.ypos));
            }
            if (tile.xpos < lengthofCollisionSquareX - 1)
            {
                returnList.Add(GetTileFromPos(tile.xpos + 1, tile.ypos));
            }

            //Tiles under
            if (tile.ypos < lengthofCollisionSquareY - 1)
            {
                if (tile.xpos > 0)
                {
                    returnList.Add(GetTileFromPos(tile.xpos - 1, tile.ypos + 1));
                }
                returnList.Add(GetTileFromPos(tile.xpos, tile.ypos + 1));
                if (tile.xpos < lengthofCollisionSquareX - 1)
                {
                    returnList.Add(GetTileFromPos(tile.xpos + 1, tile.ypos + 1));
                }
            }
            return returnList;
        }

        private Tile GetTileFromPos(int xPos, int yPos)
        {
            foreach (DataRow o in dt.Select("Xpos = " + xPos + " AND Ypos = " + yPos).Take(1))
            {
                int index = (int)(o["Index"]);
                return knapparna[index];
            }
            return null;
        }

        protected override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, transformMatrix: camera.transform);
            GraphicsDevice.Clear(Color.DarkGray);
            for (int i = 0; i < knapparna.Count; i++)
            {
                knapparna[i].Draw(_spriteBatch, font);
            }
            if (debugging)
            {
                for (int i = 0; i < knapparna.Count; i++)
                {
                    _spriteBatch.DrawString(font, "i:" + i, new Vector2(knapparna[i].rectangle.Left + 5, knapparna[i].rectangle.Top + 3), Color.Black);
                }
            }
            Vector3 cameraPos = camera.transform.Translation;
            // TODO: Add your drawing code here
            _spriteBatch.End();
            _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
            fullscreen.Draw(_spriteBatch, font);
            next.Draw(_spriteBatch, font);
            play.Draw(_spriteBatch, font);
            clear.Draw(_spriteBatch, font);
            reset.Draw(_spriteBatch, font);
            if (debugging)
            {
                float size = 1.6f;
                int lines = 6;
                Rectangle background = new Rectangle(0, 40, 450, 20 * (lines + 2));
                _spriteBatch.Draw(aliveBox, background, Color.White);
                _spriteBatch.DrawString(font, "position: " + position.ToString(), new Vector2(30, 50), Color.Red, 0, new Vector2(), size, SpriteEffects.None, 0);
                //_spriteBatch.DrawString(font, "musklickpos: " + muspositionTillWorldPåKlick.ToString(), new Vector2(5, 25), Color.Black);
                //_spriteBatch.DrawString(font, "musworldpos: " + camera.ScreenToWorldSpace(new Vector2(Mouse.GetState().X, Mouse.GetState().Y)).ToString(), new Vector2(5, 45), Color.Black);
                //_spriteBatch.DrawString(font, "musdiff: " + (muspositionTillWorldPåKlick - camera.ScreenToWorldSpace(new Vector2(Mouse.GetState().X, Mouse.GetState().Y))).ToString(), new Vector2(5, 65), Color.Black);
                //_spriteBatch.DrawString(font, "Actualmusdiff: " + (previousMousePos - camera.ScreenToWorldSpace(new Vector2(Mouse.GetState().X, Mouse.GetState().Y))).ToString(), new Vector2(5, 85), Color.Black);
                _spriteBatch.DrawString(font, "tilesUpdated: " + (tilesPåverkadeSenast).ToString(), new Vector2(30, 70), Color.Red, 0, new Vector2(), size, SpriteEffects.None, 0);
                _spriteBatch.DrawString(font, "timesinceIterate: " + (timeSinceIteration).Elapsed.ToString(), new Vector2(30, 90), Color.Red, 0, new Vector2(), size, SpriteEffects.None, 0);
                _spriteBatch.DrawString(font, "zoom: " + (camera.Zoom).ToString(), new Vector2(30, 110), Color.Red, 0, new Vector2(), size, SpriteEffects.None, 0);
                _spriteBatch.DrawString(font, "scrollWheel: " + (Input.clampedScrollWheelValue).ToString(), new Vector2(30, 130), Color.Red, 0, new Vector2(), size, SpriteEffects.None, 0);
                _spriteBatch.DrawString(font, "timetakentoiterate: " + (timeTakenToIterate.Elapsed).ToString(), new Vector2(30, 150), Color.Red, 0, new Vector2(), size, SpriteEffects.None, 0);
            }
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }

    internal class Vector2Dec
    {
        public decimal X;
        public decimal Y;

        public Vector2Dec()
        {
            X = 0;
            Y = 0;
        }

        public Vector2Dec(decimal _x)
        {
            X = _x;
            Y = 0;
        }

        public Vector2Dec(decimal _x, decimal _y)
        {
            X = _x;
            Y = _y;
        }

        public static void Transform(ref Vector2Dec position, ref Matrix matrix, out Vector2Dec result)
        {
            var x = (position.X * (decimal)matrix.M11) + (position.Y * (decimal)matrix.M21) + (decimal)matrix.M41;
            var y = (position.X * (decimal)matrix.M12) + (position.Y * (decimal)matrix.M22) + (decimal)matrix.M42;
            result = new Vector2Dec();
            result.X = x;
            result.Y = y;
        }

        public static Vector2Dec Transform(Vector2Dec position, Matrix matrix)
        {
            var x = (position.X * (decimal)matrix.M11) + (position.Y * (decimal)matrix.M21) + (decimal)matrix.M41;
            var y = (position.X * (decimal)matrix.M12) + (position.Y * (decimal)matrix.M22) + (decimal)matrix.M42;
            Vector2Dec result = new Vector2Dec();
            result.X = x;
            result.Y = y;
            return result;
        }

        public static Vector2Dec operator -(Vector2Dec value1, Vector2Dec value2)
        {
            Vector2Dec result = new Vector2Dec();
            result.X = value1.X - value2.X;
            result.Y = value1.Y - value2.Y;
            return value1;
        }

        public static Vector2Dec operator +(Vector2Dec value1, Vector2Dec value2)
        {
            Vector2Dec result = new Vector2Dec();
            result.X = value1.X + value2.X;
            result.Y = value1.Y + value2.Y;
            return value1;
        }

        public static Vector2Dec operator *(Vector2Dec value1, Vector2Dec value2)
        {
            Vector2Dec result = new Vector2Dec();

            result.X = value1.X * value2.X;
            result.Y = value1.Y * value2.Y;
            return value1;
        }

        public static Vector2Dec operator /(Vector2Dec value1, Vector2Dec value2)
        {
            Vector2Dec result = new Vector2Dec();
            result.X = value1.X / value2.X;
            result.Y = value1.Y / value2.Y;
            return value1;
        }
    }
}