using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SpaceGame.utilities
{
    enum ScoreType
    {
        RangeKill,
        MeleeKill,
        FlameKill
    }
    /// <summary>
    /// Singleton class for tracking and displaying score within a level
    /// </summary>
    class ScoreManager
    {
        const int c_maxScorePops = 20;  //Max # of on-screen score popups
        const string c_scoreFontName = "MenuFont";

        static ScoreManager s_instance;
        static SpriteFont s_scoreFont;
        static ScoreData[] s_scoreData;
        public static ContentManager Content;

        ScorePopup[] _scorePopups;
        public int CurrentScore { get; private set; }

        public static void LoadContent(ContentManager content)
        {
            Content = content;
            s_scoreFont = Content.Load<SpriteFont>(c_scoreFontName);
            s_scoreData = new ScoreData[Enum.GetValues(typeof(ScoreType)).Length];
            foreach (int val in Enum.GetValues(typeof(ScoreType)))
            {   //populate score data table from data manager for fast lookup (index by enum values)
                s_scoreData[val] = DataManager.GetData<ScoreData>(Enum.GetName(typeof(ScoreType), val));
            }
        }

        public ScoreManager()
        {
            s_instance = this;
            _scorePopups = new ScorePopup[c_maxScorePops];
            CurrentScore = 0;
        }

        /// <summary>
        /// Update movement of score popups
        /// </summary>
        /// <param name="time">elapsed time</param>
        public void Update(TimeSpan time)
        {
            for (int i = 0; i < _scorePopups.Length; i++)
            {
                _scorePopups[i].Update(time);
            }
        }

        /// <summary>
        /// Draw score popups to screen
        /// </summary>
        /// <param name="sb">SpriteBatch with which to draw</param>
        public void Draw(SpriteBatch sb)
        {
            for (int i = 0; i < _scorePopups.Length; i++)
            {
                _scorePopups[i].Draw(sb);
            }
        }

        /// <summary>
        /// Notify ScoreManager of a score event
        /// </summary>
        /// <param name="position">Location where score event occured</param>
        /// <param name="type">type of score event</param>
        public static void RegisterScore(Vector2 position, ScoreType type)
        {
            s_instance.registerScore(position, type);
        }

        void registerScore(Vector2 position, ScoreType type)
        {
            for (int i = 0; i < _scorePopups.Length; i++)
            {
                if (_scorePopups[i].LifeTime <= TimeSpan.Zero)
                {
                    _scorePopups[i] = new ScorePopup(position, s_scoreData[(int)type]);
                }
            }
        }

        struct ScorePopup
        {
            public TimeSpan LifeTime;
            public Vector2 Position;
            public ScoreData Data;
            public Color TextColor
            {
                get { return Data.Color; }
            }
            public int Score
            {
                get { return Data.Points; }
            }

            public ScorePopup(Vector2 position, ScoreData data)
            {
                Position = position;
                Data = data;
                LifeTime = TimeSpan.FromSeconds(data.LifeTime);
            }

            public void Update(TimeSpan time)
            {
                LifeTime -= time;
            }

            public void Draw(SpriteBatch sb)
            {
                if (LifeTime > TimeSpan.Zero)
                {
                    sb.DrawString(s_scoreFont, Data.Text, Position, Data.Color);
                }
            }
        }
    }

    class ScoreData
    {
        public string Name;
        public string Text;
        public int Points;
        public Color Color;
        public float LifeTime;
        public float Scale;
    }

}
