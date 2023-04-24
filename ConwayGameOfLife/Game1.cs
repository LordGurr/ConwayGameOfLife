using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace ConwayGameOfLife
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private List<Tile> knapparna = new List<Tile>();
        private Tile[,] arrayTiles;
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
        private Button uncapped;

        private bool updateUncapped = false;

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
        private Texture2D box;
        private Stopwatch deltaTime;
        private Rectangle tilesOnScreen;

        private bool drawRectPos = true;

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
            box = Content.Load<Texture2D>("Box15White");
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

            uncapped = new Button(new Rectangle(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2, 160, 40), aliveBox, "Uncapped");
            uncapped.setPos((int)Math.Round((_graphics.PreferredBackBufferWidth / 2) + uncapped.rectangle.Width * 1.7f), _graphics.PreferredBackBufferHeight - next.rectangle.Height * 2);

            bool addedX = false;
            int xpos = 4;
            int ypos = 10;
            int size = 12;
            arrayTiles = new Tile[((size * 3) / 2) * (_graphics.PreferredBackBufferWidth / widthOfSingleCollisionSquare), ((size * 3) / 2) * (_graphics.PreferredBackBufferHeight / widthOfSingleCollisionSquare)];
            for (int a = -_graphics.PreferredBackBufferHeight * (size / 2); a < _graphics.PreferredBackBufferHeight * size; a += widthOfSingleCollisionSquare)
            {
                for (int i = -_graphics.PreferredBackBufferWidth * (size / 2); i < _graphics.PreferredBackBufferWidth * size; i += widthOfSingleCollisionSquare)
                {
                    knapparna.Add(new Tile(new Rectangle(i - (int)((0.25f * size - 0.5f) * _graphics.PreferredBackBufferWidth), a - (int)((0.25f * size - 0.5f) * _graphics.PreferredBackBufferHeight), widthOfSingleCollisionSquare, widthOfSingleCollisionSquare), debug, aliveBox, (i + _graphics.PreferredBackBufferWidth * (size / 2)) / widthOfSingleCollisionSquare, (a + _graphics.PreferredBackBufferHeight * (size / 2)) / widthOfSingleCollisionSquare));
                    DataRow tempRow = dt.NewRow();
                    tempRow[0] = knapparna.Count - 1;
                    tempRow[1] = (i + _graphics.PreferredBackBufferWidth * (size / 2)) / widthOfSingleCollisionSquare;
                    tempRow[2] = (a + _graphics.PreferredBackBufferHeight * (size / 2)) / widthOfSingleCollisionSquare;
                    dt.Rows.Add(tempRow);
                    arrayTiles[(i + _graphics.PreferredBackBufferWidth * (size / 2)) / widthOfSingleCollisionSquare, (a + _graphics.PreferredBackBufferHeight * (size / 2)) / widthOfSingleCollisionSquare] = knapparna[^1];
                    //dt.Rows.Add(knapparna.Count - 1, i, a);
                    if (!addedX)
                    {
                        lengthofCollisionSquareX++;
                    }
                }
                addedX = true;
                lengthofCollisionSquareY++;
            }
            Vector2Int leftTop = new Vector2Int(knapparna[0].rectangle.X, knapparna[0].rectangle.Y);
            Vector2Int rightBottom = new Vector2Int(knapparna[^1].rectangle.X, knapparna[^1].rectangle.Y);
            Vector2Int center = new Vector2Int(arrayTiles[arrayTiles.GetLength(0) / 2, arrayTiles.GetLength(1) / 2].rectangle.X, arrayTiles[arrayTiles.GetLength(0) / 2, arrayTiles.GetLength(1) / 2].rectangle.Y);
            _graphics.IsFullScreen = false;
            _graphics.ApplyChanges();
            Input.setCameraStuff(camera);
            deltaTime = new Stopwatch();
            deltaTime.Start();
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            Input.GetState();
            //deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            deltaTime.Restart();
            if (IsActive)
            {
                Vector2 topLeft = (camera.ScreenToWorldSpace(Vector2.Zero));
                Vector2 bottomRight = camera.ScreenToWorldSpace(new Vector2(camera.viewport.Width, camera.viewport.Height));
                Vector2Int topLeftScaled = new Vector2Int(AdvancedMath.GetNearestMultiple((int)topLeft.X, widthOfSingleCollisionSquare), AdvancedMath.GetNearestMultiple((int)topLeft.Y, widthOfSingleCollisionSquare));
                Vector2Int bottomRightScaled = new Vector2Int(AdvancedMath.GetNearestMultiple((int)bottomRight.X, widthOfSingleCollisionSquare), AdvancedMath.GetNearestMultiple((int)bottomRight.Y, widthOfSingleCollisionSquare));

                topLeftScaled -= new Vector2Int(arrayTiles[0, 0].rectangle.X, arrayTiles[0, 0].rectangle.Y);
                bottomRightScaled -= new Vector2Int(arrayTiles[0, 0].rectangle.X, arrayTiles[0, 0].rectangle.Y);

                topLeftScaled /= widthOfSingleCollisionSquare;
                bottomRightScaled /= widthOfSingleCollisionSquare;

                topLeftScaled = new Vector2Int(Math.Max(topLeftScaled.X, 0), Math.Max(topLeftScaled.Y, 0));

                tilesOnScreen = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)topLeft.X + (int)bottomRight.X, (int)topLeft.Y + (int)bottomRight.Y);
                tilesOnScreen = new Rectangle((int)topLeftScaled.X - 1, (int)topLeftScaled.Y - 1, /*(int)topLeftScaled.X +*/ (int)bottomRightScaled.X + 1, /*(int)topLeftScaled.Y +*/ (int)bottomRightScaled.Y + 1);

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
                    uncapped.setPos((int)Math.Round((_graphics.PreferredBackBufferWidth / 2) + uncapped.rectangle.Width * 1.7f), _graphics.PreferredBackBufferHeight - next.rectangle.Height * 2);

                    monitorSwitch = false;
                }
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
                if (debugging && Input.GetButtonDown(Keys.Home))
                {
                    drawRectPos = !drawRectPos;
                }
                bool buttonClicked = false;
                if (next.rectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y) || fullscreen.rectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y) || clear.rectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y) || play.rectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y) || reset.rectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y) || uncapped.rectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y))
                {
                    buttonClicked = true;
                    if (next.Clicked() && !iterating && !playing)
                    {
                        StartThreadIterate();
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
                        Input.SetScrollWheel(0);
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
                    if (uncapped.Clicked())
                    {
                        updateUncapped = !updateUncapped;
                    }
                }
                if (Input.GetButtonDown(Keys.F11) || Input.GetButtonDown(Keys.Space) || Input.GetButtonDown(Keys.Enter))
                {
                    buttonClicked = true;
                    if (Input.GetButtonDown(Keys.F11))
                    {
                        //fullscreen.setPos(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - buttonStart.rectangle.Width, 0);
                        //_graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                        //_graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                        _graphics.IsFullScreen = !_graphics.IsFullScreen;
                        _graphics.ApplyChanges();
                        monitorSwitch = true;
                    }
                    if (Input.GetButtonDown(Keys.Space))
                    {
                        playing = !playing;
                    }
                    if (Input.GetButtonDown(Keys.Enter) && !iterating && !playing)
                    {
                        StartThreadIterate();
                    }
                }
                if (playing && !iterating && (timeSinceIteration.Elapsed.TotalSeconds > timeForIterate || updateUncapped))
                {
                    StartThreadIterate();
                }
                if (Input.GetButton(Keys.Up))
                {
                    timeForIterate -= 1 * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    timeForIterate = (float)Math.Clamp(timeForIterate, 0.05, 10);
                }
                if (Input.GetButton(Keys.Down))
                {
                    timeForIterate += 1 * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    timeForIterate = (float)Math.Clamp(timeForIterate, 0.05, 2);
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
                    //camera.Zoom -= 0.01f;
                    Input.SetScrollWheel(Input.clampedScrollWheelValue - (int)(990 * gameTime.ElapsedGameTime.TotalSeconds));
                }
                if (Input.GetButton(Keys.PageUp))
                {
                    Input.SetScrollWheel(Input.clampedScrollWheelValue + (int)(990 * gameTime.ElapsedGameTime.TotalSeconds));
                    //camera.Zoom += 0.01f;
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
                if (!buttonClicked && !playing)
                {
                    if (Input.GetMouseButtonDown(0))
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
                    else if (Input.GetMouseButton(0))
                    {
                        try
                        {
                            /*List<Tile> tilesToCheck = knapparna.FindAll(a => a.alive != Input.mouseClickingToAlive);
                            for (int i = 0; i < tilesToCheck.Count; i++)
                            {
                                tilesToCheck[i].Clicked(camera.ScreenToWorldSpace(new Vector2(Mouse.GetState().X, Mouse.GetState().Y)));
                            }*/

                            for (int x = Math.Max(tilesOnScreen.X, 0); x < Math.Min(tilesOnScreen.Width, arrayTiles.GetLength(0)); x++)
                            {
                                for (int y = Math.Max(tilesOnScreen.Y, 0); y < Math.Min(tilesOnScreen.Height, arrayTiles.GetLength(1)); y++)
                                {
                                    if (arrayTiles[x, y].alive != Input.mouseClickingToAlive)
                                    {
                                        arrayTiles[x, y].Clicked(camera.ScreenToWorldSpace(new Vector2(Mouse.GetState().X, Mouse.GetState().Y)));
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e.Message);
                            string temp = e.Message;
                        }
                    }
                    if (Input.GetButtonDown(Keys.End))
                    {
                        for (int i = 0; i < knapparna.Count; i++)
                        {
                            //List<Tile> rutorBredvid = tilesBredvid(tilesPåverkade[i]);
                            knapparna[i].SetAlive(3);
                            knapparna[i].UpdateAlive();
                        }
                    }
                    if (Input.GetButtonDown(Keys.Insert))
                    {
                        Random rng = new Random();
                        for (int i = 0; i < knapparna.Count; i++)
                        {
                            //List<Tile> rutorBredvid = tilesBredvid(tilesPåverkade[i]);
                            knapparna[i].SetAlive(rng.Next(10) == 0 ? 3 : 0);
                            knapparna[i].UpdateAlive();
                        }
                    }
                }
                base.Update(gameTime);
            }
            else
            {
                Input.GetState();
                lastMousePosition = Input.MousePos();
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

        private void StartThreadIterate()
        {
            iterating = true;
            Thread t = new Thread(IterateNew);
            t.Name = "IterateThread";
            t.Start();
        }

        private void Iterate()
        {
            try
            {
                iterating = true;
                timeSinceIteration.Restart();
                timeTakenToIterate.Restart();
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
                timeTakenToIterate.Stop();
                iterating = false;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Next iteration error: " + e.Message);
                iterating = false;
                timeTakenToIterate.Stop();
            }
        }

        private void IterateNew()
        {
            //try
            {
                iterating = true;
                timeSinceIteration.Restart();
                timeTakenToIterate.Restart();
                List<Tile> tilesPåverkade = new List<Tile>();
                List<Tile> levandeTiles = knapparna.FindAll(x => x.alive);
                tilesPåverkade.AddRange(levandeTiles);
                for (int i = 0; i < levandeTiles.Count; i++)
                {
                    //List<Tile> rutorBredvid = tilesBredvid(levandeTiles[i]);
                    foreach (Tile tile in TilesBredvidNew(new Vector2Int(levandeTiles[i].xpos, levandeTiles[i].ypos)))
                    {
                        tilesPåverkade.Add(tile);
                    }
                    //tilesPåverkade.AddRange(rutorBredvid);
                    //levandeTiles[i].SetAlive(rutorBredvid.FindAll(x => x.alive).Count);
                    //List<Tile> dödaTiles = rutorBredvid.FindAll(x => !x.alive);
                    //for (int a = 0; a < dödaTiles.Count; a++)
                    //{
                    //    if (tilesPåverkade.FindAll(o => o == dödaTiles[a]).Count > 1)
                    //    {
                    //        dödaTiles.RemoveAt(a);
                    //    }
                    //}
                    //tilesPåverkade.AddRange(dödaTiles);
                    //for (int a = 0; a < dödaTiles.Count; a++)
                    //{
                    //    List<Tile> rutorBredvidDöda = tilesBredvid(dödaTiles[a]);
                    //    //tilesPåverkade.AddRange(rutorBredvidDöda);
                    //    dödaTiles[a].SetAlive(rutorBredvidDöda.FindAll(x => x.alive).Count);
                    //}
                }
                for (int i = 0; i < tilesPåverkade.Count; i++)
                {
                    //List<Tile> rutorBredvid = tilesBredvid(tilesPåverkade[i]);
                    tilesPåverkade[i].SetAlive(TilesBredvidCountNew(new Vector2Int(tilesPåverkade[i].xpos, tilesPåverkade[i].ypos)));
                }
                for (int i = 0; i < tilesPåverkade.Count; i++)
                {
                    tilesPåverkade[i].UpdateAlive();
                }
                tilesPåverkadeSenast = tilesPåverkade.Count;
                timeTakenToIterate.Stop();
                iterating = false;
            }
            //catch (Exception e)
            //{
            //    Debug.WriteLine("Next iteration error: " + e.Message);
            //    iterating = false;
            //    timeTakenToIterate.Stop();
            //}
        }

        private void IterateThread()
        {
            try
            {
                iterating = true;
                timeSinceIteration.Restart();
                timeTakenToIterate.Restart();
                List<Tile> tilesPåverkade = new List<Tile>();
                List<Tile> levandeTiles = knapparna.FindAll(x => x.alive);
                tilesPåverkade.AddRange(levandeTiles);
                List<Thread> threads = new List<Thread>();
                for (int i = 0; i < levandeTiles.Count; i++)
                {
                    Tile tile = levandeTiles[i];
                    threads.Add(new Thread(() => IterateTile(tile)));
                    threads[threads.Count - 1].Start();
                }
                for (int i = 0; i < threads.Count; i++)
                {
                    threads[i].Join();
                }
                for (int i = 0; i < knapparna.Count; i++)
                {
                    knapparna[i].UpdateAlive();
                }
                tilesPåverkadeSenast = tilesPåverkade.Count;
                timeTakenToIterate.Stop();
                iterating = false;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Next iteration error: " + e.Message);
                iterating = false;
                timeTakenToIterate.Stop();
            }
        }

        private void IterateTile(Tile tile)
        {
            List<Tile> rutorBredvid = tilesBredvid(tile);
            //tilesPåverkade.AddRange(rutorBredvid);
            tile.SetAlive(rutorBredvid.FindAll(x => x.alive).Count);
            List<Tile> dödaTiles = rutorBredvid.FindAll(x => !x.alive);
            //for (int a = 0; a < dödaTiles.Count; a++)
            //{
            //    if (tilesPåverkade.FindAll(o => o == dödaTiles[a]).Count > 1)
            //    {
            //        dödaTiles.RemoveAt(a);
            //    }
            //}
            //tilesPåverkade.AddRange(dödaTiles);
            for (int a = 0; a < dödaTiles.Count; a++)
            {
                List<Tile> rutorBredvidDöda = tilesBredvid(dödaTiles[a]);
                //tilesPåverkade.AddRange(rutorBredvidDöda);
                dödaTiles[a].SetAlive(rutorBredvidDöda.FindAll(x => x.alive).Count);
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

        public static readonly Vector2Int[] Directions = new[]
        {
            new Vector2Int(0, 1),     //  Up
            new Vector2Int(0, -1),    //  Down
            new Vector2Int(-1, 0),    //  Left
            new Vector2Int(1, 0),     //  Right

            //new Location(1,1),      //  Up Right
            //new Location(1,-1),     //  Down Right
            //new Location(-1,-1),    //  Down Left
            //new Location(-1,1),     //  Up Left
        };

        public static readonly Vector2Int[] DiagonalDirections = new[]
        {
            new Vector2Int(1,1),      //  Up Right
            new Vector2Int(1,-1),     //  Down Right
            new Vector2Int(-1,-1),    //  Down Left
            new Vector2Int(-1,1),     //  Up Left
        };

        public static readonly Vector2Int[] Lookup = new[]
        {
            new Vector2Int(0,3),
            new Vector2Int(1,3),
            new Vector2Int (1,2),
            new Vector2Int(0,2),
        };

        private IEnumerable<Tile> TilesBredvidNew(Vector2Int id)
        {
            bool[] passable = new bool[4];
            for (int i = 0; i < Directions.Length; i++)
            {
                Vector2Int next = new Vector2Int(id.X + Directions[i].X, id.Y + Directions[i].Y);

                if (InBounds(next))
                {
                    yield return GetTileFromPosNew(next);
                    passable[i] = true;
                }
            }
            for (int i = 0; i < DiagonalDirections.Length; i++)
            {
                Vector2Int next = new Vector2Int(id.X + DiagonalDirections[i].X, id.Y + DiagonalDirections[i].Y);

                if (passable[Lookup[i].X] && passable[Lookup[i].Y])
                {
                    yield return GetTileFromPosNew(next);
                }
            }
        }

        private int TilesBredvidCountNew(Vector2Int id)
        {
            bool[] passable = new bool[4];
            int amountAlive = 0;
            for (int i = 0; i < Directions.Length; i++)
            {
                Vector2Int next = new Vector2Int(id.X + Directions[i].X, id.Y + Directions[i].Y);

                if (InBounds(next))
                {
                    //yield return GetTileFromPos(next);
                    passable[i] = true;
                    if (GetTileFromPosNew(next).alive)
                    {
                        amountAlive++;
                    }
                }
            }
            for (int i = 0; i < DiagonalDirections.Length; i++)
            {
                Vector2Int next = new Vector2Int(id.X + DiagonalDirections[i].X, id.Y + DiagonalDirections[i].Y);

                if (passable[Lookup[i].X] && passable[Lookup[i].Y])
                {
                    if (GetTileFromPosNew(next).alive)
                    {
                        amountAlive++;
                    }
                }
            }
            return amountAlive;
        }

        public bool InBounds(Vector2Int id)
        {
            return 0 <= id.X && id.X < lengthofCollisionSquareX &&
                   0 <= id.Y && id.Y < lengthofCollisionSquareY;
        }

        private Tile GetTileFromPos(int xPos, int yPos)
        {
            foreach (DataRow o in dt.Select("Xpos = " + xPos + " AND Ypos = " + yPos).Take(1))
            {
                int index = (int)(o["Index"]);
                return knapparna[index];
            }
            Debug.WriteLine("Tile from pos retuns null");
            return null;
        }

        private Tile GetTileFromPos(Vector2Int vector2Int)
        {
            foreach (DataRow o in dt.Select("Xpos = " + vector2Int.X + " AND Ypos = " + vector2Int.Y).Take(1))
            {
                int index = (int)(o["Index"]);
                return knapparna[index];
            }
            Debug.WriteLine("Tile from pos retuns null");
            return null;
        }

        private Tile GetTileFromPosNew(Vector2Int vector2Int)
        {
            return arrayTiles[vector2Int.X, vector2Int.Y];
            Debug.WriteLine("Tile from pos retuns null");
            return null;
        }

        private void DrawButtons()
        {
            int drawnButtons = 0;
            for (int x = Math.Max(tilesOnScreen.X, 0); x < Math.Min(tilesOnScreen.Width, arrayTiles.GetLength(0)); x++)
            {
                for (int y = Math.Max(tilesOnScreen.Y, 0); y < Math.Min(tilesOnScreen.Height, arrayTiles.GetLength(1)); y++)
                {
                    arrayTiles[x, y].Draw(_spriteBatch, font);
                    drawnButtons++;
                }
            }
            drawnButtons = 0;
        }

        protected override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, transformMatrix: camera.transform);
            Color color = new Color(252, 204, 76);
            GraphicsDevice.Clear(Color.DarkGray);
            //for (int i = 0; i < knapparna.Count; i++)
            //{
            //    knapparna[i].Draw(_spriteBatch, font);
            //}
            DrawButtons();
            if (debugging)
            {
                //for (int i = 0; i < knapparna.Count; i++)
                //{
                //    _spriteBatch.DrawString(font, "i:" + i, new Vector2(knapparna[i].rectangle.Left + 5, knapparna[i].rectangle.Top + 3), Color.Black);
                //}
                if (drawRectPos)
                {
                    for (int x = Math.Max(tilesOnScreen.X, 0); x < Math.Min(tilesOnScreen.Width, arrayTiles.GetLength(0)); x++)
                    {
                        for (int y = Math.Max(tilesOnScreen.Y, 0); y < Math.Min(tilesOnScreen.Height, arrayTiles.GetLength(1)); y++)
                        {
                            _spriteBatch.DrawString(font, "pos:", new Vector2(arrayTiles[x, y].rectangle.Left + 5, arrayTiles[x, y].rectangle.Top + 13), Color.Black);
                            _spriteBatch.DrawString(font, arrayTiles[x, y].rectangle.X.ToString(), new Vector2(arrayTiles[x, y].rectangle.Left + 5, arrayTiles[x, y].rectangle.Top + 29), Color.Black);
                            _spriteBatch.DrawString(font, arrayTiles[x, y].rectangle.Y.ToString(), new Vector2(arrayTiles[x, y].rectangle.Left + 5, arrayTiles[x, y].rectangle.Top + 42), Color.Black);
                        }
                    }
                    //for (int i = 0; i < knapparna.Count; i++)
                    //{
                    //    _spriteBatch.DrawString(font, "pos:", new Vector2(knapparna[i].rectangle.Left + 5, knapparna[i].rectangle.Top + 13), Color.Black);
                    //    _spriteBatch.DrawString(font, knapparna[i].rectangle.X.ToString(), new Vector2(knapparna[i].rectangle.Left + 5, knapparna[i].rectangle.Top + 29), Color.Black);
                    //    _spriteBatch.DrawString(font, knapparna[i].rectangle.Y.ToString(), new Vector2(knapparna[i].rectangle.Left + 5, knapparna[i].rectangle.Top + 42), Color.Black);
                    //}
                }
                else
                {
                    for (int x = Math.Max(tilesOnScreen.X, 0); x < Math.Min(tilesOnScreen.Width, arrayTiles.GetLength(0)); x++)
                    {
                        for (int y = Math.Max(tilesOnScreen.Y, 0); y < Math.Min(tilesOnScreen.Height, arrayTiles.GetLength(1)); y++)
                        {
                            _spriteBatch.DrawString(font, "pos:", new Vector2(arrayTiles[x, y].rectangle.Left + 5, arrayTiles[x, y].rectangle.Top + 13), Color.Black);
                            _spriteBatch.DrawString(font, arrayTiles[x, y].xpos.ToString(), new Vector2(arrayTiles[x, y].rectangle.Left + 5, arrayTiles[x, y].rectangle.Top + 29), Color.Black);
                            _spriteBatch.DrawString(font, arrayTiles[x, y].ypos.ToString(), new Vector2(arrayTiles[x, y].rectangle.Left + 5, arrayTiles[x, y].rectangle.Top + 42), Color.Black);
                            //_spriteBatch.DrawString(font, "pos:" + knapparna[i].xpos.ToString() + "," + knapparna[i].ypos.ToString(), new Vector2(knapparna[i].rectangle.Left + 5, knapparna[i].rectangle.Top + 13), Color.Black);
                        }
                    }

                    //for (int i = 0; i < knapparna.Count; i++)
                    //{
                    //    _spriteBatch.DrawString(font, "pos:", new Vector2(knapparna[i].rectangle.Left + 5, knapparna[i].rectangle.Top + 13), Color.Black);
                    //    _spriteBatch.DrawString(font, knapparna[i].xpos.ToString(), new Vector2(knapparna[i].rectangle.Left + 5, knapparna[i].rectangle.Top + 29), Color.Black);
                    //    _spriteBatch.DrawString(font, knapparna[i].ypos.ToString(), new Vector2(knapparna[i].rectangle.Left + 5, knapparna[i].rectangle.Top + 42), Color.Black);
                    //    //_spriteBatch.DrawString(font, "pos:" + knapparna[i].xpos.ToString() + "," + knapparna[i].ypos.ToString(), new Vector2(knapparna[i].rectangle.Left + 5, knapparna[i].rectangle.Top + 13), Color.Black);
                    //}
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
            uncapped.Draw(_spriteBatch, font);
            if (debugging)
            {
                float size = 1.6f;
                int lines = 9;
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
                _spriteBatch.DrawString(font, "timeforiterate: " + (timeForIterate).ToString(), new Vector2(30, 170), Color.Red, 0, new Vector2(), size, SpriteEffects.None, 0);
                _spriteBatch.DrawString(font, "iterating: " + (iterating).ToString(), new Vector2(30, 190), Color.Red, 0, new Vector2(), size, SpriteEffects.None, 0);
                _spriteBatch.DrawString(font, "fps: " + (1 / deltaTime.Elapsed.TotalSeconds).ToString("F1"), new Vector2(30, 210), Color.Red, 0, new Vector2(), size, SpriteEffects.None, 0);
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

    public class Vector2Int
    {
        public int X { get; set; }

        public int Y { get; set; }

        public Vector2Int()
        {
            X = 0;
            Y = 0;
        }

        public Vector2Int(int _x)
        {
            X = _x;
            Y = 0;
        }

        public Vector2Int(int _x, int _y)
        {
            X = _x;
            Y = _y;
        }

        public static Vector2Int operator +(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2Int operator -(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.X - b.X, a.Y - b.Y);
        }

        public static Vector2Int operator *(Vector2Int a, int b)
        {
            return new Vector2Int(a.X * b, a.Y * b);
        }

        public static Vector2Int operator *(Vector2Int a, float b)
        {
            return new Vector2Int((int)Math.Round(a.X * b, MidpointRounding.AwayFromZero), (int)Math.Round(a.Y * b, MidpointRounding.AwayFromZero));
        }

        public static Vector2Int operator /(Vector2Int a, int b)
        {
            return new Vector2Int(a.X / b, a.Y / b);
        }

        public static Vector2Int operator /(Vector2Int a, float b)
        {
            return new Vector2Int((int)Math.Round(a.X / b, MidpointRounding.AwayFromZero), (int)Math.Round(a.Y / b, MidpointRounding.AwayFromZero));
        }

        public static Vector2Int operator %(Vector2Int a, int b)
        {
            return new Vector2Int(a.X % b, a.Y % b);
        }

        public static Vector2Int operator %(Vector2Int a, float b)
        {
            return new Vector2Int((int)Math.Round(a.X % b, MidpointRounding.AwayFromZero), (int)Math.Round(a.Y % b, MidpointRounding.AwayFromZero));
        }

        public static readonly Vector2Int One = new Vector2Int(1, 1);
        public static readonly Vector2Int Zero = new Vector2Int(0, 0);

        public float Distance(Vector2Int b)
        {
            return AdvancedMath.Vector2Distance(new Vector2(X, Y), new Vector2(b.X, b.Y));
        }

        //public Vector2Int Normalize
        //{
        //    get
        //    {
        //        Vector2 temp = AdvancedMath.Normalize(new Vector2(X, Y));
        //        return new Vector2Int((int)Math.Round(temp.X, MidpointRounding.AwayFromZero), (int)Math.Round(temp.Y, MidpointRounding.AwayFromZero));
        //    }
        //}

        public Vector2 ToVector2()
        {
            return new Vector2(X, Y);
        }
    }
}