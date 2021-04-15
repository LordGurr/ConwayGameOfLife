using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Data;
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
        private Button clear;
        private Camera camera;
        private Vector2 muspositionTillWorldPåKlick = new Vector2();
        public static Vector2 screenSize { get; private set; }
        private int tilesPåverkadeSenast = 0;
        private Vector2 position;

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
            clear.setPos((int)((_graphics.PreferredBackBufferWidth / 2) + clear.rectangle.Width * 0.6f), _graphics.PreferredBackBufferHeight - next.rectangle.Height * 2);
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
            if (next.rectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y) || fullscreen.rectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y) || clear.rectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y))
            {
                buttonClicked = true;
                if (next.Clicked())
                {
                    try
                    {
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
                                if (tilesPåverkade.Contains(dödaTiles[a]))
                                {
                                    dödaTiles.RemoveAt(a);
                                }
                            }
                            tilesPåverkade.AddRange(dödaTiles);
                            for (int a = 0; a < dödaTiles.Count; a++)
                            {
                                List<Tile> rutorBredvidDöda = tilesBredvid(dödaTiles[a]);
                                tilesPåverkade.AddRange(rutorBredvidDöda);
                                dödaTiles[a].SetAlive(rutorBredvidDöda.FindAll(x => x.alive).Count);
                            }
                        }
                        for (int i = 0; i < tilesPåverkade.Count; i++)
                        {
                            tilesPåverkade[i].UpdateAlive();
                        }
                        tilesPåverkadeSenast = tilesPåverkade.Count;
                    }
                    catch (System.Exception e)
                    {
                        string temp = e.Message;
                    }
                }
                if (clear.Clicked())
                {
                    List<Tile> levandeTiles = knapparna.FindAll(x => x.alive);
                    for (int i = 0; i < levandeTiles.Count; i++)
                    {
                        levandeTiles[i].SetAlive(false);
                        levandeTiles[i].UpdateAlive();
                    }
                }

                // TODO: Add your update logic here
                if (fullscreen.Clicked())
                {
                    //fullscreen.setPos(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - buttonStart.rectangle.Width, 0);
                    //_graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                    //_graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                    _graphics.IsFullScreen = !_graphics.IsFullScreen;
                    _graphics.ApplyChanges();
                    screenSize = new Vector2(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
                }
            }
            //camera.UpdateCamera(position);
            if (position + camera.ScreenToWorldSpace(new Vector2(Mouse.GetState().X, Mouse.GetState().Y)) == muspositionTillWorldPåKlick)
            {
            }
            camera.UpdateCamera(position + (Input.GetMouseButton(2) ? Vector2.Subtract(camera.ScreenToWorldSpace(new Vector2(Mouse.GetState().X, Mouse.GetState().Y)), muspositionTillWorldPåKlick) : new Vector2()));
            Vector3 cameraPos = camera.transform.Translation;
            if (Input.GetButton(Keys.PageDown))
            {
                camera.Zoom -= 0.01f;
            }
            else if (Input.GetButton(Keys.PageUp))
            {
                camera.Zoom += 0.01f;
            }
            camera.Zoom = (float)(Input.clampedScrollWheelValue * 0.001) + 1;
            if (Input.GetMouseButtonDown(2))
            {
                muspositionTillWorldPåKlick = camera.ScreenToWorldSpace(new Vector2(Mouse.GetState().X, Mouse.GetState().Y));
            }
            if (Input.GetMouseButton(2))
            {
            }
            if (!buttonClicked || Input.GetMouseButton(0))
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
                catch (System.Exception e)
                {
                    string temp = e.Message;
                }
            }
            base.Update(gameTime);
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
                    _spriteBatch.DrawString(font, "i: " + i, new Vector2(knapparna[i].rectangle.Left + 5, knapparna[i].rectangle.Top + 3), Color.Black);
                }
            }
            Vector3 cameraPos = camera.transform.Translation;
            // TODO: Add your drawing code here
            _spriteBatch.End();
            _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
            fullscreen.Draw(_spriteBatch, font);
            next.Draw(_spriteBatch, font);
            clear.Draw(_spriteBatch, font);
            if (debugging)
            {
                _spriteBatch.DrawString(font, "position: " + position.ToString(), new Vector2(5, 5), Color.Black);
                _spriteBatch.DrawString(font, "musklickpos: " + muspositionTillWorldPåKlick.ToString(), new Vector2(5, 25), Color.Black);
                _spriteBatch.DrawString(font, "musworldpos: " + camera.ScreenToWorldSpace(new Vector2(Mouse.GetState().X, Mouse.GetState().Y)).ToString(), new Vector2(5, 45), Color.Black);
            }
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}