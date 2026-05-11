using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Task2.Models
{
    public abstract class LivingBeing : INotifyPropertyChanged, ILivingBeing
    {
        private double _currentSpeed;
        public double MaxSpeed { get; protected set; }
        public double AccelerationStep { get; protected set; }
        public double DecelerationStep { get; protected set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public double CurrentSpeed
        {
            get => _currentSpeed;
            set
            {
                if (_currentSpeed != value)
                {
                    _currentSpeed = Math.Max(0, Math.Min(value, MaxSpeed));
                    OnPropertyChanged();
                }
            }
        }

        protected LivingBeing(double maxSpeed, double accelerationStep, double decelerationStep)
        {
            MaxSpeed = maxSpeed;
            AccelerationStep = accelerationStep;
            DecelerationStep = decelerationStep;
            CurrentSpeed = 0;
        }

        public virtual string Move()
        {
            double oldSpeed = CurrentSpeed;
            CurrentSpeed += AccelerationStep;

            if (CurrentSpeed >= MaxSpeed)
            {
                return $"Разогналась с {oldSpeed:F1} до {CurrentSpeed:F1} км/ч - Достигнута максимальная скорость!";
            }
            return $"Разогналась с {oldSpeed:F1} до {CurrentSpeed:F1} км/ч";
        }

        public virtual string Stand()
        {
            double oldSpeed = CurrentSpeed;
            CurrentSpeed -= DecelerationStep;

            if (CurrentSpeed <= 0)
            {
                CurrentSpeed = 0;
                return $"Замедлилась с {oldSpeed:F1} до 0 км/ч - Полная остановка!";
            }
            return $"Замедлилась с {oldSpeed:F1} до {CurrentSpeed:F1} км/ч";
        }

        public abstract string GetInfo();

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}