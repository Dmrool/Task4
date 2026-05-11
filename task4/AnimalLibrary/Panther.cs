using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimalLibrary
{
    public class Panther : LivingBeing
    {
        private bool _isOnTree;
        public event EventHandler Roar;

        public bool IsOnTree
        {
            get => _isOnTree;
            private set
            {
                if (_isOnTree != value)
                {
                    _isOnTree = value;
                    OnPropertyChanged();
                }
            }
        }

        public Panther() : base(80.0, 3.0, 2.5)
        {
            IsOnTree = false;
        }

        public override string Move()
        {
            if (IsOnTree)
            {
                return "Нельзя двигаться, пока пантера на дереве! Сначала нужно слезть!";
            }
            return base.Move();
        }

        public override string Stand()
        {
            if (IsOnTree)
            {
                return "Нельзя стоять на земле, пока пантера на дереве! Сначала нужно слезть!";
            }
            return base.Stand();
        }

        public string MakeSound()
        {
            Roar?.Invoke(this, EventArgs.Empty);
            if (IsOnTree)
            {
                return "Пантера рычит с дерева!";
            }
            return "Пантера рычит!";
        }

        public string ClimbTree()
        {
            if (IsOnTree)
            {
                return "Пантера уже на дереве!";
            }

            if (CurrentSpeed > 0)
            {
                return "Нельзя залезть на дерево во время движения! Сначала нужно остановиться!";
            }

            IsOnTree = true;
            return "Пантера залезла на дерево!";
        }

        public string GetDownFromTree()
        {
            if (!IsOnTree)
            {
                return "Пантера и так на земле!";
            }

            IsOnTree = false;
            return "Пантера спустилась с дерева!";
        }

        public override string GetInfo()
        {
            string treeStatus = IsOnTree ? " на дереве" : " на земле";
            return $"Пантера{treeStatus} | Скорость: {CurrentSpeed:F1} / {MaxSpeed} км/ч | Шаг разгона: {AccelerationStep:F1} км/ч";
        }
    }
}