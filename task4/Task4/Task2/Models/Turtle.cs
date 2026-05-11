using Task2.Models;

namespace Task2.Models
{
    public class Turtle : LivingBeing
    {
        public Turtle() : base(3.0, 0.4, 0.3)
        {
        }

        public override string Move()
        {
            return base.Move();
        }

        public override string Stand()
        {
            return base.Stand();
        }

        public override string GetInfo()
        {
            return $"Черепаха | Скорость: {CurrentSpeed:F1} / {MaxSpeed} км/ч";
        }

    }
}