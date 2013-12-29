using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

using SpaceGame.utility;
using System.IO;

namespace SpaceGame.states
{
    abstract class Gamestate
    {
        protected const string c_bgMusicDir = "audio/music/";
        public static ContentManager Content;
        #region properties
        //request to exit state (pop off state stack)
        public bool PopState { get; protected set; }
        //request to push a new state onto the stack
        public Gamestate PushState { get; protected set; }
        //request to replace state with another state
        public Gamestate ReplaceState { get; protected set; }
        //if true, the state below on the stack should also be drawn
        public bool Transparent { get; protected set; }
        public SongCollection BgMusic { get; protected set; }
        protected ContentManager _content;
        static Random s_rand = new Random();
        public virtual Vector2 GamepadAimOrigin
        {   //origin from which to calculate gamepad aim angle from thumbstick
            get
            {   //default is center of screen
                return new Vector2(Game1.SCREENWIDTH / 2, Game1.SCREENHEIGHT / 2);
            }
        }
        #endregion

        #region constructor
        public Gamestate(ContentManager content, bool transparent, string bgMusicFolder = null)
        {
            Transparent = transparent;
            _content = content;
            if (!String.IsNullOrEmpty(bgMusicFolder))
            {
                BgMusic = new SongCollection();
                string folder = "Content/" + c_bgMusicDir + bgMusicFolder + "/";
                foreach (string filename in Directory.GetFiles(folder))
                {
                    string name = filename.Remove(0, "Content/".Length);
                    BgMusic.Add(Content.Load<Song>(name));
                }
            }
            if (BgMusic != null) 
            {
                MediaPlayer.Play(BgMusic, s_rand.Next(BgMusic.Count));
            }
        }
        #endregion

        #region methods
        public abstract void Update(GameTime gameTime, InputManager input, InventoryManager im);
        public abstract void Draw(SpriteBatch spriteBatch);
        
        #endregion
    }
}
