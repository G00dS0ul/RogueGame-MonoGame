using System.Text;
using Microsoft.Xna.Framework;
using RogueSharp_MonoGame.Core;
using RogueSharp_MonoGame.Interfaces;
using RogueSharp;
using RogueSharp.DiceNotation;
using Monster = RogueSharp_MonoGame.Core.Monster;

namespace RogueSharp_MonoGame.Systems
{
    public class CommandSystem
    {
        public bool IsPlayerTurn { get; set; }
        public bool MovePlayer(Direction? direction)
        {
            var x = GameSession.Player.X;
            var y = GameSession.Player.Y;

            switch (direction)
            {
                case Direction.Up:
                    y = GameSession.Player.Y - 1; 
                    break;
                case Direction.Down:
                    y = GameSession.Player.Y + 1; 
                    break;
                case Direction.Left:
                    x = GameSession.Player.X - 1;
                    break;
                case Direction.Right:
                    x = GameSession.Player.X + 1;
                    break;
                default:
                    return false;
            }

            if (GameSession.DungeonMap.SetActorPosition(GameSession.Player, x, y))
            {
                return true;
            }

            var monster = GameSession.DungeonMap.GetMonsterAt(x, y);

            if (monster != null)
            {
                Attack(GameSession.Player, monster);
                return true;
            }

            return false;
        }

        public void Attack(Actor attacker, Actor defender)
        {
            var attackMessage = new StringBuilder();
            var defenseMessage = new StringBuilder();

            var hits = ResolveAttack(attacker, defender, attackMessage);

            var block = ResolveDefense(defender, hits, attackMessage, defenseMessage);

            GameSession.MessageLog.Add(attackMessage.ToString());
            if (!string.IsNullOrWhiteSpace(defenseMessage.ToString()))
            {
                GameSession.MessageLog.Add(defenseMessage.ToString());
            }

            var damage = hits - block;

            ResolveDamage(defender, damage);

        }

        public void EndPlayerTurn()
        {
            IsPlayerTurn = false;
        }

        public void ActivateMonsters()
        {
            ISchedulable schedulable = GameSession.SchedulingSystem.Get();
            if(schedulable is Player)
            {
                IsPlayerTurn = true;
                GameSession.SchedulingSystem.Add(GameSession.Player);
            }
            else
            {
                var monster = schedulable as Core.Monster;

                if (monster != null)
                {
                    monster.PerformAction(this);
                    GameSession.SchedulingSystem.Add(monster);
                }

                ActivateMonsters();
            }
        }

        public void MoveMonster(Core.Monster monster, ICell cell)
        {
            if (!GameSession.DungeonMap.SetActorPosition(monster, cell.X, cell.Y))
            {
                if (GameSession.Player.X == cell.X && GameSession.Player.Y == cell.Y)
                {
                    Attack(monster, GameSession.Player);
                }
            }
        }
        private static int ResolveAttack(Actor attacker, Actor defender, StringBuilder attackMessage)
        {
            var hits = 0;

            attackMessage.AppendFormat("{0} attacks {1} and rolls: ", attacker.Name, defender.Name);

            var attackDice = new DiceExpression().Dice(attacker.Attack, 100);
            var attackResult = attackDice.Roll();

            foreach (var termResult in attackResult.Results)
            {
                attackMessage.Append( termResult.Value + ", ");

                if(termResult.Value >= 100 - attacker.AttackChance)
                {
                    hits++;
                }
            }

            return hits;
        }

        private static int ResolveDefense(Actor defender, int hits, StringBuilder attackMessage,
            StringBuilder defenseMessage)
        {
            var block = 0;

            if (hits > 0)
            {
                attackMessage.AppendFormat( " scoring {0} hits.", hits);
                defenseMessage.AppendFormat( " {0} defends and rolls: ", defender.Name);

                var defenseDice = new DiceExpression().Dice(defender.Defense, 100);
                var defenseRoll = defenseDice.Roll();

                foreach (var termResult in defenseRoll.Results)
                {
                    defenseMessage.Append(termResult.Value + ", ");

                    if (termResult.Value >= 100 - defender.DefenseChance)
                    {
                        block++;
                    }
                }
                defenseMessage.AppendFormat( " blocking {0} hits.", block);
            }
            else
            {
                attackMessage.Append("and misses completely.");
            }

            return block;
        }

        private static void ResolveDamage(Actor defender, int damage)
        {
            if (damage > 0)
            {
                defender.Health -= damage;

                GameSession.MessageLog.Add($"{defender.Name} was hit for {damage} damage ");

                if (defender.Health <= 0)
                {
                    ResolveDeath(defender);
                }
            }
            else
            {
                GameSession.MessageLog.Add($"{defender.Name} blocked all damage");
            }
        }

        private static void ResolveDeath(Actor defender)
        {
            if (defender is Player)
            {
                GameSession.MessageLog.Add($"{defender.Name} was killed, GAME OVER MAN!");
            }
            else if (defender is Core.Monster)
            {
                GameSession.DungeonMap.RemoveMonster((Core.Monster)defender);
                GameSession.MessageLog.Add($"{defender.Name} died and dropped {defender.Gold} gold.");
            }
        }
    }
}
