using System;
using Task2.Models;

namespace Task2.Models
{
    public class Dog : LivingBeing
    {
        public event EventHandler Bark;

        public Dog() : base(45.0, 2.5, 2.0)
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

        public string MakeSound()
        {
            Bark?.Invoke(this, EventArgs.Empty);
            return "Собака лает!";
        }

        public override string GetInfo()
        {
            return $"Собака | Скорость: {CurrentSpeed:F1} / {MaxSpeed} км/ч | Шаг разгона: {AccelerationStep:F1} км/ч";
        }
    }
}