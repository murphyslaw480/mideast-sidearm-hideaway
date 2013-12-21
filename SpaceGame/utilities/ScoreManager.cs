using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
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

        static ScoreManager _instance;

        ScorePopup[] _scorePopups;
        public int CurrentScore { get; private set; }

        public ScoreManager()
        {
            _instance = this;
            _scorePopups = new ScorePopup[c_maxScorePops];
            CurrentScore = 0;
        }

        /// <summary>
        /// Update movement of score popups
        /// </summary>
        /// <param name="time">elapsed time</param>
        public void Update(TimeSpan time)
        {
        }

        /// <summary>
        /// Draw score popups to screen
        /// </summary>
        /// <param name="sb">SpriteBatch with which to draw</param>
        public void Draw(SpriteBatch sb)
        {
        }

        /// <summary>
        /// Notify ScoreManager of a score event
        /// </summary>
        /// <param name="position">Location where score event occured</param>
        /// <param name="type">type of score event</param>
        public static void RegisterScore(Vector2 position, ScoreType type)
        {
        }

        struct ScorePopup
        {
            public TimeSpan LifeTime;
            public Vector2 Position;
            public string Text;
        }
    }

}
