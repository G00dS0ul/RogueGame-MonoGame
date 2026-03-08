using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueSharp_MonoGame.Interfaces;
using RogueSharp;
using IDrawable = RogueSharp_MonoGame.Interfaces.IDrawable;

namespace RogueSharp_MonoGame.Core
{
    public class Actor : IActor, IDrawable, ISchedulable
    {
        #region Backing Variabe

        private string _name;
        private int _awareness;
        private int _attack;
        private int _attackChance;
        private int _defense;
        private int _defenseChance;
        private int _gold;
        private int _health;
        private int _maxHealth;
        private int _speed;

        #endregion

        #region Properties

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
            }
        }

        public int Awareness
        {
            get =>_awareness;
            set
            {
                _awareness = value;
            }
        }

        public int Attack
        {
            get => _attack;
            set
            {
                _attack = value;
            }
        }

        public int AttackChance
        {
            get => _attackChance;
            set
            {
                _attackChance = value;
            }
        }

        public int Defense
        {
            get => _defense;
            set
            {
                _defense = value;
            }
        }

        public int DefenseChance
        {
            get => _defenseChance;
            set
            {
                _defenseChance = value;
            }
        }

        public int Gold
        {
            get => _gold;
            set
            {
                _gold = value;
            }
        }

        public int Health
        {
            get => _health;
            set
            {
                _health = value;
            }
        }

        public int MaxHealth
        {
            get => _maxHealth;
            set
            {
                _maxHealth = value;
            }
        }

        public int Speed
        {
            get => _speed;
            set
            {
                _speed = value;
            }
        }
        public Color Color { get; set; }
        public Char Symbol { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        #endregion


        public int Time
        {
            get => Speed;
        }
        public void Draw(SpriteBatch spriteBatch, Texture2D tileset)
        {
            //var cell = map.GetCell(X, Y);
            //if (!cell.IsExplored)
            //{
            //    return;
            //}

            //if (cell.IsInFov)
            //{
            //    console.Set(X, Y, Color, Colors.FloorBackgroundFov, Symbol);
            //}
            //else
            //{
            //    console.Set(X, Y, Color, Colors.FloorBackground, '.');
            //}
        }
    }
}
