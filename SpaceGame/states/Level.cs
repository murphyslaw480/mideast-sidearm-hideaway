using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

using SpaceGame.graphics;
using SpaceGame.graphics.hud;
using SpaceGame.utility;
using SpaceGame.units;
using SpaceGame.equipment;
using Microsoft.Xna.Framework.Content;

namespace SpaceGame.states
{
    class LevelData
    {
        public string Name;
        public BurstWaveData[] BurstWaves;
        public TrickleWaveData[] TrickleWaves;
        public UnicornData[] Unicorns;
        public BlackHoleData BlackHole;
        public int PlayerX, PlayerY;
        public int Width, Height;
    }

    //each gadget is associated with an action that affects the game
    public delegate void GadgetAction(bool active);

    class Level : Gamestate
    {
		public static Texture2D s_CursorTexture;
        const string c_levelNameFormat = "Level{0}";

        #region classes
        #endregion

        #region constants
        const float c_timeSlowFactor = 0.5f;
        #endregion

        #region fields
        Spaceman _player;
        InventoryManager _inventoryManager;
        BlackHole _blackHole;
        Gadget _primaryGadget, _secondaryGadget;
        Wave[] _waves;         //collection of all trickle waves and active burst wave
        int _burstWaveIdx;

        Unicorn[] _unicorns;
        Rectangle _levelBounds;
        Vector2 _absMousePos, _relMousePos;

        Hud userInterface;
        Rectangle _cameraLock;
        Camera2D _camera;

        bool _timeSlowed;

		Vector2 _cursorTextureCenter;

        TimeSpan _gameOverTimer = TimeSpan.FromSeconds(3.0);
        #endregion

        #region constructor
        public Level (ContentManager content, int levelNumber, InventoryManager im)
            : base(content, false, "Level")
        {
            LevelData data = DataManager.GetData<LevelData>(String.Format(c_levelNameFormat, levelNumber));
            _levelBounds = new Rectangle(0, 0, data.Width, data.Height);
            _player = new Spaceman(new Vector2(data.PlayerX, data.PlayerY));
            _blackHole = new BlackHole(data.BlackHole);
            //store active burst wave as last tricklewave
            _waves = new Wave[data.TrickleWaves.Length + data.BurstWaves.Length];
            _camera = new Camera2D(_player.Position, _levelBounds.Width, _levelBounds.Height);
            //construct waves
            for (int i = 0; i < data.TrickleWaves.Length; i++)
            { 
                _waves[i] = new TrickleWave(data.TrickleWaves[i], _levelBounds);
            }
            for (int i = 0; i < data.BurstWaves.Length; i++)
            {
                BurstWave prevWave = (i == 0) ? null : (BurstWave)_waves[i + data.TrickleWaves.Length - 1];
                _waves[i + data.TrickleWaves.Length] = new BurstWave(data.BurstWaves[i], _levelBounds, prevWave);
            }
            //Test code to set weapons 1-6 to created weapons
            im.setPrimaryGadget(new Gadget("Teleporter", this));
            im.setSecondaryGadget(new Gadget("Stopwatch", this));
            im.setSlot(1, new ThrowableWeapon("Cryonade", _player));

            if (data.Unicorns == null)
            {
                _unicorns = new Unicorn[0];
            }
            else
            {
                _unicorns = new Unicorn[data.Unicorns.Length];
                for (int j = 0; j < data.Unicorns.Length; j++)
                {
                    _unicorns[j] = new Unicorn(data.Unicorns[j]);
                }
            }

            _primaryGadget = im.getPrimaryGadget();
            _secondaryGadget = im.getSecondaryGadget();
            _inventoryManager = im;
            
            userInterface = new Hud(_player, _blackHole, _waves);

			_cursorTextureCenter = new Vector2(s_CursorTexture.Width / 2 , s_CursorTexture.Height / 2);
            selectRandomWeapons();
        }

        void selectRandomWeapons()
        {
            Random rand = new Random();
            int rand1 = rand.Next(0, 4);
            int rand2;
            do
            {
                rand2 = rand.Next(0, 4);
            } while (rand2 == rand1);
            ProjectileWeapon[] weapons = new ProjectileWeapon[]
            {
                new ProjectileWeapon("Shotgun", _player),
                new ProjectileWeapon("Gatling", _player),
                new ProjectileWeapon("Flamethrower", _player),
                new ProjectileWeapon("RocketLauncher", _player),
            };
            _player.PrimaryWeapon = weapons[rand1];
            _player.SecondaryWeapon = weapons[rand2];
        }
        #endregion

        #region methods
        public override void Update(GameTime gameTime, InputManager input, InventoryManager im)
        {
            if (_player.UnitLifeState == PhysicalUnit.LifeState.Destroyed || _player.UnitLifeState == PhysicalUnit.LifeState.Disabled
                || _blackHole.capacityUsed > _blackHole.totalCapacity)
            {
                _gameOverTimer -= gameTime.ElapsedGameTime;
            }
            if (_gameOverTimer < TimeSpan.Zero)
            {
                ReplaceState = new Gamemenu(_content);
            }
            _absMousePos = input.AbsoluteMousePos;
            _relMousePos = input.RelativeMousePos;
            input.SetCameraOffset(_camera.Position);
            handleInput(input);
            _camera.Update(gameTime, _player.Position);
            //if player is outside static area rectangle, call update on camera to update position of camera until
            //the player is in the static area rectangle or the camera reaches the _levelbounds, in which case,
            //the camera does not move in that direction (locks)

            /*
            if ((_player.HitRect.Bottom > _cameraLock.Bottom && _player.HitRect.Top < _cameraLock.Top &&
            _player.HitRect.Right < _cameraLock.Right && _player.HitRect.Left > _cameraLock.Left) && (player is in level bounds)
            {
             * _camera.Update(gameTime);
             * _cameraLock.X = (int)(_camera.position.X + (_camera.getViewportWidth() * 0.2));
             * _cameraLock.Y = (int)(_camera.position.Y + (_camera.getViewportHeight() * 0.2));
             * 
            }*/
           
            if (_timeSlowed)
                gameTime = new GameTime(gameTime.TotalGameTime, 
                    TimeSpan.FromSeconds((float)gameTime.ElapsedGameTime.TotalSeconds / 2));

            if (_blackHole.State == BlackHole.BlackHoleState.Pulling)
            {
                _blackHole.ApplyToUnit(_player, gameTime);
            }
            _player.Update(gameTime, _levelBounds);
            _primaryGadget.Update(gameTime);
            _secondaryGadget.Update(gameTime);
            _blackHole.Update(gameTime);


            if (_blackHole.State == BlackHole.BlackHoleState.Overdrive)
            {
                foreach (Wave w in _waves)
                {
                    w.SpawnEnable = false;
                }
                foreach (Unicorn u in _unicorns)
                {
                    u.SpawnEnable = false;
                }
            }
          
            for (int i = 0; i < _waves.Length; i++)
            {
                _waves[i].Update(gameTime, _player, _blackHole, _player.PrimaryWeapon, _player.SecondaryWeapon, _inventoryManager, _unicorns);
                //check cross-wave collisions
                if (_waves[i].Active)
                {
                    for (int j = i + 1; j < _waves.Length; j++)
                    {
                        _waves[i].CheckAndApplyCollisions(_waves[j]);
                    }
                }
            }

            for (int i = 0; i < _unicorns.Length; i++)
            {
                _unicorns[i].Update(gameTime, _levelBounds, _blackHole.Position, _player.Position, _player.HitRect);
                _unicorns[i].CheckAndApplyCollision(_player, gameTime);
                _blackHole.TryEatUnicorn(_unicorns[i], gameTime);
            }

            //Update Weapons 
            if (_player.CurrentWeapon != null)
            {
                _player.PrimaryWeapon.Update(gameTime);
                _player.SecondaryWeapon.Update(gameTime);
            }
            //update all items
            _inventoryManager.Update(gameTime, input);
        }

        private void handleInput(InputManager input)
        { 
            if (input.Exit)
                this.PopState = true;

            if (_blackHole.State == BlackHole.BlackHoleState.Exhausted)
                return;

            _player.MoveDirection = input.MoveDirection;
            _player.LookDirection = XnaHelper.DirectionBetween(_player.Position, input.AbsoluteMousePos);

            if (_player.UnitLifeState == PhysicalUnit.LifeState.Living)
            {
                if (input.FirePrimary)
                {
                    _player.TriggerWeapon(input.AbsoluteMousePos, 0);
                }
                else if (input.FireSecondary)
                {
                    _player.TriggerWeapon(input.AbsoluteMousePos, 1);
                }
                if (input.UseItem)
                {
                    _inventoryManager.CurrentItem.Use(input.AbsoluteMousePos);
                }
                if (input.TriggerGadget1)
                {
                    _primaryGadget.Trigger();
                }
                if (input.TriggerGadget2)
                {
                    _secondaryGadget.Trigger();
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, _camera.TransformMatrix());
            
            _blackHole.Draw(spriteBatch);
            _player.Draw(spriteBatch);
            if (_player.CurrentWeapon != null)
            {
                _player.PrimaryWeapon.Draw(spriteBatch);
                _player.SecondaryWeapon.Draw(spriteBatch);
            }
            if (_inventoryManager.CurrentItem != null)
            {
                _inventoryManager.CurrentItem.Draw(spriteBatch);
            }
			
            foreach (Wave wave in _waves)
            {
                wave.Draw(spriteBatch);
            }
			
            foreach (Unicorn unicorn in _unicorns)
            {
                unicorn.Draw(spriteBatch);
            }
            spriteBatch.End();

            spriteBatch.Begin();
            userInterface.draw(spriteBatch);
			spriteBatch.Draw(s_CursorTexture, _relMousePos - _cursorTextureCenter, Color.White);
            spriteBatch.End();

        }
        #endregion

        #region gadget actions
        public void TimeSlowAction(bool active)
        {
            _timeSlowed = active;
        }
        public void TeleportAction(bool active)
        {
            _player.Teleport(_absMousePos);
        }
        #endregion
    }
}
