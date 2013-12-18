using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceGame.graphics
{
    class Sprite
    {
        #region constant
        protected const string c_spritePath = "spritesheets/";
        protected const string c_unitSpritePath = c_spritePath + "units/";
        protected const string c_bodySpritePath = c_unitSpritePath + "bodies/";
        protected const string c_projectileSpritePath = c_spritePath + "projectiles/";
        protected const string c_weaponSpritePath = c_spritePath + "weapons/";
        protected const string c_obstacleSpritePath = c_spritePath + "obstacles/";

        const float ICE_OPACITY_FACTOR = 0.5f;
        //time it takes to complete a teleport
        const float c_teleportTime = 0.25f;
        //visual stretching of sprite during teleport
        const float c_teleportDialation = 2.0f;
        #endregion
        
        #region static
        public enum SpriteType
        {
            None,
            Unit,
            Projectile,
            Weapon,
            Obstacle
        }
        public static ContentManager Content;
        static Rectangle tempRect;   //temporary rectangle for cross instance use
        public static Texture2D IceCubeTexture
        {
            get {return iceCubeTexture;}
            set 
            { 
                iceCubeTexture = value;
                iceCubeTextureCenter = new Vector2(iceCubeTexture.Width / 2, iceCubeTexture.Height / 2);
            }
        }
        static Texture2D iceCubeTexture;
        static Vector2 iceCubeTextureCenter;
        #endregion

        #region fields
        //length of each animation, in # of frames
        protected int _framesPerAnimation;
        //number of different animations, e.g. for facing different directions
        protected int _numStates;
        //time between each animation frame, and countdown to next frame
        TimeSpan _animationInterval, _timeTillNext;
        //used if PlayAnimation is called
        public bool Animating { get; private set;}
        public bool AnimationOver { get; private set;}
        //current frame of animation
        protected int _currentFrame = 0;
        //current animation state
        protected int _currentState = 0;
        //width and height of sprite
        int _frameWidth, _frameHeight;
        //spritesheet of animation frames
        //(_framesPerAnimation) # of sprites high, (_numStates) # of sprites wide
        //height(pixels) = _framesPerAnimation * _frameHeight, width(pixels) = _numStates * _frameWidth
        Texture2D _spriteSheet;
        //scale and rotation
        float _defaultScale;
        float _scale;
        float _angle = 0.0f;
        //layer on which to draw sprite
        private float _zLayer;
        //size of sprite
        Rectangle _size;
        //shade do draw the sprite with
        private Color _shade;
        //2Darray of rects to select from sprite sheet
        Rectangle[,] _rects;
        //origin used for rotation
        Vector2 _origin;
		//whether to loop animation
        bool _loopAnimation;

        //for Flash(Color, Time)
        Color _flashColor;
        int _flashCounter = 0;
        TimeSpan _currentFlashTime, _halfFlashTime;
        float _teleportTimer;
        Vector2 _teleportStartPos;
        #endregion

        #region properties
        public Color Shade
        {
            get { return _shade; }
            set
            {
                _flashCounter = 0;  //stop flashing if shade manually set
                _shade = value;
            }
        }

        public float Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                //Recalculate the Size of the Sprite with the new scale
                _size = new Rectangle(0, 0, (int)(_frameWidth * Scale), (int)(_frameHeight * Scale));
            }
        }

        public Vector2 Origin
        {
            get { return _origin; }
            protected set { _origin = value; }
        }

        //scale relative to default scale
        public float ScaleFactor
        {
            get {return _scale / _defaultScale;}
            set { Scale = value * _defaultScale; }
        }

        public float Angle
        {
            get { return _angle; }
            set { _angle = value; }
        }

        public virtual int AnimationState
        {
            get { return _currentState; }
            set { _currentState = value % _numStates; }
        }

        public Vector2 Center {get {return new Vector2(_size.Center.X, _size.Center.Y);}}
        public float Height { get { return _size.Bottom; } }
        public float Width { get { return _size.Right; } }
        public float FrameHeight { get { return _frameHeight; } }
        public float FrameWidth { get { return _frameWidth; } }
        public TimeSpan FullAnimationTime
        {
            get 
            {
                return TimeSpan.FromSeconds(
                (float)_animationInterval.TotalSeconds * _framesPerAnimation);
            }
        }
        public bool FlipH { get; set; }
        public bool FlipV { get; set; }
        #endregion properties

        #region methods
        public Sprite(string spriteName, SpriteType type = SpriteType.None)
            : this(DataManager.GetData<SpriteData>(spriteName), type)
        { }

        protected Sprite(SpriteData spriteData, SpriteType type = SpriteType.None)
        {
            _frameWidth = spriteData.FrameWidth; 
            _frameHeight = spriteData.FrameHeight;
            _origin = new Vector2(_frameWidth / 2.0f, _frameHeight / 2.0f);
            _framesPerAnimation = spriteData.NumFrames;
            _numStates = spriteData.NumStates;
            _defaultScale = spriteData.DefaultScale;
            ScaleFactor = 1.0f;      //use property to set rect
            _animationInterval = TimeSpan.FromSeconds(spriteData.SecondsPerAnimation);
            _timeTillNext = _animationInterval;
            initRects();
            Shade = Color.White;
            _zLayer = spriteData.ZLayer;
            switch (type)
            {
                case SpriteType.Projectile:
                    _spriteSheet = Content.Load<Texture2D>(c_projectileSpritePath + spriteData.AssetName);
                    break;
                case SpriteType.Unit:
                    _spriteSheet = Content.Load<Texture2D>(c_unitSpritePath + spriteData.AssetName);
                    break;
                case SpriteType.Weapon:
                    _spriteSheet = Content.Load<Texture2D>(c_weaponSpritePath + spriteData.AssetName);
                    break;
                case SpriteType.Obstacle:
                    _spriteSheet = Content.Load<Texture2D>(c_obstacleSpritePath + spriteData.AssetName);
                    break;
                default:
                    _spriteSheet = Content.Load<Texture2D>(c_spritePath + spriteData.AssetName);
                    break;
            }
        }

        private void initRects()
        {
            _rects = new Rectangle[_numStates, _framesPerAnimation];

            for (int y = 0; y < _framesPerAnimation; y++)
            {
                for (int x = 0; x < _numStates; x++)
                {
                    _rects[x, y] = new Rectangle(x * _frameWidth, y * _frameHeight, _frameWidth, _frameHeight);
                }
            }
        }

        public void Reset()
        {
            _timeTillNext = _animationInterval;
            _currentState = 0; _currentFrame = 0;
            Shade = Color.White;
            _angle = 0.0f;
            Scale = _defaultScale;
        }

        public virtual void Update(GameTime theGameTime)
        {
            animate(theGameTime);

            handleFlash(theGameTime);

            if (_teleportTimer > 0)
            {
                _teleportTimer -= (float)theGameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        protected void animate(GameTime theGameTime)
        {
            _timeTillNext -= theGameTime.ElapsedGameTime;

            if (Animating && _timeTillNext <= TimeSpan.Zero && _currentFrame == _framesPerAnimation - 1)
            {
				if (!_loopAnimation)
                {
                    AnimationOver = true;
                    Animating = false;
                    _currentFrame = 0;
                }
            }

            if (_timeTillNext < TimeSpan.Zero)
            {
                _timeTillNext = _animationInterval;
                _currentFrame = (_currentFrame + 1) % _framesPerAnimation;
            }
        }

        void handleFlash(GameTime theGameTime)
        {
            if (_flashCounter > 0)
            {
                _currentFlashTime += theGameTime.ElapsedGameTime;

                if (_currentFlashTime < _halfFlashTime)
                {
                    _shade = Color.Lerp(Color.White, _flashColor, (float)_currentFlashTime.TotalSeconds / (float)_halfFlashTime.TotalSeconds);
                    _currentFlashTime += theGameTime.ElapsedGameTime;
                }
                else if (_currentFlashTime >= (_halfFlashTime + _halfFlashTime))
                {
                    _currentFlashTime = TimeSpan.Zero;
                    _flashCounter -= 1;
                    if (_flashCounter == 0)
                    {
                        _shade = Color.White;
                    }
                }
                else
                {
                    _shade = Color.Lerp(_flashColor, Color.White, (float)(_currentFlashTime - _halfFlashTime).TotalSeconds / (float)_halfFlashTime.TotalSeconds);
                }
            }
        }

        public void PlayAnimation(int animationNumber, bool loop)
        {
            Animating = true;
            AnimationOver = false;
            _loopAnimation = loop;
            AnimationState = animationNumber % _numStates;
            _currentFrame = 0;
        }

        public void PlayTeleportEffect(Vector2 startPos)
        {
            _teleportStartPos = startPos;
            _teleportTimer = c_teleportTime;
        }

        public void Flash(Color color, TimeSpan timePerFlash, int numFlashes)
        {
            _flashColor = color;
            _flashCounter = numFlashes;
            _currentFlashTime = TimeSpan.Zero;
            _halfFlashTime = TimeSpan.FromSeconds(timePerFlash.TotalSeconds / 2);
        }

        public virtual void Draw(SpriteBatch batch, Vector2 position)
        {
            Draw(batch, position, _angle, _origin);
        }

        public virtual void Draw(SpriteBatch batch, Vector2 position, float rotation)
        {
            Draw(batch, position, rotation, _origin);
        }

        public virtual void Draw(SpriteBatch batch, Vector2 position, float rotation, Vector2 origin)
        {
            SpriteEffects effects = (FlipH ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (FlipV ? SpriteEffects.FlipVertically : SpriteEffects.None);
            if (_teleportTimer > 0)
            {
                //draw previous location
                tempRect.Width = (int)(Width * _teleportTimer / c_teleportTime);   //width gets smaller
                tempRect.Height = (int)(Height * (1 + c_teleportDialation * (1 - _teleportTimer / c_teleportTime)));
                tempRect.X = (int)(_teleportStartPos.X);
                tempRect.Y = (int)(_teleportStartPos.Y);

                batch.Draw(_spriteSheet, tempRect, _rects[_currentState, _currentFrame], Shade, rotation, origin, effects, _zLayer);
                    
                //draw current location
                tempRect.Width = (int)(Width * (1 - _teleportTimer / c_teleportTime));   //width grows back to normal
                tempRect.Height = (int)(Height * (1 + c_teleportDialation * _teleportTimer / c_teleportTime));
                tempRect.X = (int)(position.X);
                tempRect.Y = (int)(position.Y);

                batch.Draw(_spriteSheet, tempRect, _rects[_currentState, _currentFrame], Shade, rotation, origin, effects, _zLayer);
            }
            else
            {
                batch.Draw(_spriteSheet, position, _rects[_currentState, _currentFrame], Shade, rotation, origin,
                    Scale, effects, _zLayer);
            }
        }

        public void DrawFragment(SpriteBatch batch, int row, int col, int numDivisions, Rectangle drawRect, float angle, float opacity)
        {
            tempRect = _rects[_currentState, _currentFrame];
            tempRect.X += tempRect.Width * col / numDivisions;
            tempRect.Y += tempRect.Height * row / numDivisions;
            tempRect.Width /= numDivisions;
            tempRect.Height /= numDivisions;
            batch.Draw(_spriteSheet, drawRect, tempRect, Color.Lerp(Color.Transparent, Color.White, opacity),
                angle, _origin / numDivisions, SpriteEffects.None, _zLayer);
        }

        public void DrawIce(SpriteBatch batch, Rectangle rect, float angle, float opacity)
        {
            batch.Draw(IceCubeTexture, rect, null,
                Color.Lerp(Color.Transparent, Color.White, opacity * ICE_OPACITY_FACTOR),
                angle, iceCubeTextureCenter, SpriteEffects.None, 0);
        }
        #endregion
    }
}
