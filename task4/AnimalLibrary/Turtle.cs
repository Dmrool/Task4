using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimalLibrary
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