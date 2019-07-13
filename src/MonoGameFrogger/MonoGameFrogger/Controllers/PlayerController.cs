﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameFrogger.Models;
using System;
using System.Linq;

namespace MonoGameFrogger.Controllers
{
    /// <summary>
    /// Player controller. Handles player input and updates the player model.
    /// </summary>
    class PlayerController : IController, IReset
    {
        private static float MoveCooldownPeriod = 0.2f;
        private static float DeathCooldownPeriod = 0.5f;
        private static float Distance = 16f;

        private readonly PlayerModel _model;
        private FrogAnimation _animation = null;
        private Cooldown _cooldown = null;
        private bool _inDeathAnimation = false;

        public event EventHandler MoveFinished;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="playerModel">Player model</param>
        public PlayerController(PlayerModel playerModel)
        {
            _model = playerModel;
        }

        /// <summary>
        /// Reset the player.
        /// </summary>
        /// <param name="resetMode">Reset mode</param>
        public void Reset(ResetMode resetMode)
        {
            if (resetMode == ResetMode.Death && !_inDeathAnimation)
            {
                // TODO: What happens when player gets to -1 lives??
                _model.Lives--;
                _inDeathAnimation = true;
                _model.Flip = SpriteEffects.None;

                _animation = new FrogAnimation(new int[] { 19, 20, 21, 22 },
                                               _model.Position,
                                               new Vector2(0, 0),
                                               DeathCooldownPeriod);
            } 
            else if (resetMode == ResetMode.Goal)
            {
                _animation = null;
                _model.Frame = 34;
                _model.Flip = SpriteEffects.None;
                _model.Position = new Vector2((16 * 7) - 8, 224);
                _cooldown = new Cooldown(0.5f);
                _inDeathAnimation = false;
            }
        }

        /// <summary>
        /// Update the player. Takes input from the keyboard and updates the position of the
        /// player's frog.
        /// </summary>
        /// <param name="deltaTime">Delta time</param>
        public void Update(float deltaTime)
        {
            if (_cooldown != null)
            {
                _cooldown.Update(deltaTime);
                if (!_cooldown.Complete)
                {
                    return;
                }
                _cooldown = null;
            }

            if (_animation != null && !_animation.Done)
            {
                _animation.Update(deltaTime);
                _model.Position = _animation.Position;
                _model.Frame = _animation.CurrentFrame;
                return;
            }

            if (_animation != null && _animation.Done)
            {
                if (_inDeathAnimation)
                {
                    _animation = null;
                    _model.Frame = 34;
                    _model.Flip = SpriteEffects.None;
                    _model.Position = new Vector2((16 * 7) - 8, 224);
                    //_cooldown = new Cooldown(0.5f);
                    _inDeathAnimation = false;
                }
                else
                {
                    _model.Position = _animation.Position;
                    _model.Frame = _animation.CurrentFrame;
                    MoveFinished?.Invoke(this, EventArgs.Empty);
                }
            }

            if (!_inDeathAnimation)
            {
                _animation = null;

                var state = Keyboard.GetState();
                var pressedKeys = state.GetPressedKeys();
                if (pressedKeys.Contains(Keys.Up) && _model.Position.Y > 16)
                {
                    _model.Flip = SpriteEffects.None;
                    _animation = new FrogAnimation(new int[] { 34, 33, 32, 34 },
                                                   _model.Position,
                                                   new Vector2(0, -Distance),
                                                   MoveCooldownPeriod);
                }
                else if (pressedKeys.Contains(Keys.Down) && _model.Position.Y < 240)
                {
                    _model.Flip = SpriteEffects.FlipVertically;
                    _animation = new FrogAnimation(new int[] { 34, 33, 32, 34 },
                                                   _model.Position,
                                                   new Vector2(0, Distance),
                                                   MoveCooldownPeriod);
                }
                else if (pressedKeys.Contains(Keys.Left) && _model.Position.X > 0)
                {
                    _model.Flip = SpriteEffects.None;
                    _animation = new FrogAnimation(new int[] { 37, 36, 35, 37 },
                                                   _model.Position,
                                                   new Vector2(-Distance, 0),
                                                   MoveCooldownPeriod);

                }
                else if (pressedKeys.Contains(Keys.Right) && _model.Position.X < 208)
                {
                    _model.Flip = SpriteEffects.FlipHorizontally;
                    _animation = new FrogAnimation(new int[] { 37, 36, 35, 37 },
                                                   _model.Position,
                                                   new Vector2(Distance, 0),
                                                   MoveCooldownPeriod);
                }
            }
        }
    }
}
