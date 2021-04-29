using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ConwayGameOfLife
{
    internal class Tile : Button
    {
        private Texture2D aliveTex;
        private Texture2D deadTex;
        public int xpos { private set; get; }
        public int ypos { private set; get; }
        public bool alive { private set; get; }

        private enum AliveNext
        {
            alive,
            dead,
            tbd
        }

        private AliveNext aliveToChangeToNextTurn = AliveNext.tbd;

        public Tile(Rectangle _rectangle, Texture2D _texture, Texture2D _aliveTex, int _xpos, int _ypos)
            : base(_rectangle, _texture)
        {
            alive = false;
            aliveTex = _aliveTex;
            deadTex = _texture;
            xpos = _xpos;
            ypos = _ypos;
        }

        public bool Clicked(Vector2 mousePos)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (rectangle.Contains(mousePos))
                {
                    alive = !alive;
                    if (alive)
                    {
                        Texture = aliveTex;
                    }
                    else
                    {
                        Texture = deadTex;
                    }
                    return true;
                }
            }
            else if (Input.GetMouseButton(0))
            {
                if (rectangle.Contains(mousePos))
                {
                    alive = Input.mouseClickingToAlive;
                    if (alive)
                    {
                        Texture = aliveTex;
                    }
                    else
                    {
                        Texture = deadTex;
                    }
                    return false;
                }
            }
            return false;
        }

        public void SetAlive(bool _alive)
        {
            if (_alive)
            {
                aliveToChangeToNextTurn = AliveNext.alive;
            }
            else
            {
                aliveToChangeToNextTurn = AliveNext.dead;
            }
        }

        public void SetAlive(int aliveNeighboors)
        {
            if (!alive && aliveNeighboors == 3)
            {
                SetAlive(true);
                return;
            }
            if (alive && (aliveNeighboors < 2) ^ (aliveNeighboors > 3))
            {
                SetAlive(false);
                return;
            }
        }

        public void UpdateAlive()
        {
            if (aliveToChangeToNextTurn == AliveNext.alive)
            {
                alive = true;
            }
            else if (aliveToChangeToNextTurn == AliveNext.dead)
            {
                alive = false;
            }
            if (alive)
            {
                Texture = aliveTex;
            }
            else
            {
                Texture = deadTex;
            }
            aliveToChangeToNextTurn = AliveNext.tbd;
        }
    }
}