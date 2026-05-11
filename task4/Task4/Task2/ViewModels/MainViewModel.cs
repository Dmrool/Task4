// MainViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Win32;

namespace ReflectionTaskManager.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _assemblyPath;
        private string _selectedClassName;
        private MethodDisplay _selectedMethod;
        private string _parametersInput;
        private string _executionResult;
        private bool _isLibraryLoaded;

        private Assembly _loadedAssembly;
        private Type _selectedType;
        private Type _interfaceType;

        public event PropertyChangedEventHandler PropertyChanged;

        // 🔹 Путь к DLL
        public string AssemblyPath
        {
            get => _assemblyPath;
            set { _assemblyPath = value; OnPropertyChanged(); }
        }

        
        public ObservableCollection<string> ClassNames { get; } = new ObservableCollection<string>();

        // 🔹 Выбранный класс
        public string SelectedClassName
        {
            get => _selectedClassName;
            set
            {
                if (_selectedClassName != value)
                {
                    _selectedClassName = value;
                    OnPropertyChanged();
                    LoadMethods();
                }
            }
        }

        // 🔹 Коллекция методов (БЕЗ set!)

        public ObservableCollection<MethodDisplay> Methods { get; } = new ObservableCollection<MethodDisplay>();

        // 🔹 Выбранный метод
        public MethodDisplay SelectedMethod
        {
            get => _selectedMethod;
            set
            {
                if (_selectedMethod != value)
                {
                    _selectedMethod = value;
                    OnPropertyChanged();
                    ((RelayCommand)ExecuteMethodCommand).RaiseCanExecuteChanged();
                }
            }
        }

        // 🔹 Ввод параметров
        public string ParametersInput
        {
            get => _parametersInput;
            set { _parametersInput = value; OnPropertyChanged(); }
        }

        // 🔹 Результат выполнения
        public string ExecutionResult
        {
            get => _executionResult;
            set { _executionResult = value; OnPropertyChanged(); }
        }

        // 🔹 Флаг загрузки
        public bool IsLibraryLoaded
        {
            get => _isLibraryLoaded;
            set { _isLibraryLoaded = value; OnPropertyChanged(); }
        }

        // 🔹 Команды
        public ICommand LoadLibraryCommand { get; }
        public ICommand ExecuteMethodCommand { get; }
        public ICommand BrowseCommand { get; }

        public MainViewModel()
        {
            LoadLibraryCommand = new RelayCommand(LoadLibrary);
            ExecuteMethodCommand = new RelayCommand(ExecuteSelectedMethod, CanExecuteMethod);
            BrowseCommand = new RelayCommand(BrowseForDll);
        }

        private void BrowseForDll()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Library Files (*.dll)|*.dll|All Files (*.*)|*.*";
            openFileDialog.Title = "Выберите библиотеку классов";

            if (openFileDialog.ShowDialog() == true)
            {
                AssemblyPath = openFileDialog.FileName;
            }
        }

        private void LoadLibrary()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AssemblyPath) || !File.Exists(AssemblyPath))
                {
                    ExecutionResult = "❌ Ошибка: Укажите корректный путь к DLL файлу.";
                    return;
                }

                _loadedAssembly = Assembly.LoadFrom(AssemblyPath);

                // Ищем интерфейс по имени внутри загруженной сборки
                _interfaceType = _loadedAssembly.GetTypes()
                    .FirstOrDefault(t => t.Name == "ILivingBeing" && t.IsInterface);

                if (_interfaceType == null)
                {
                    ExecutionResult = "❌ Интерфейс ILivingBeing не найден в библиотеке!";
                    IsLibraryLoaded = false;
                    return;
                }

                // Находим все подходящие классы
                var types = _loadedAssembly.GetTypes()
                    .Where(t => _interfaceType.IsAssignableFrom(t)
                             && !t.IsInterface
                             && !t.IsAbstract
                             && t.GetConstructor(Type.EmptyTypes) != null)
                    .OrderBy(t => t.Name)
                    .ToList();

                // 🔹 Очищаем и заполняем коллекцию (не заменяем её!)
                ClassNames.Clear();
                foreach (var type in types)
                {
                    ClassNames.Add(type.FullName);
                }

                // 🔹 Принудительно уведомляем UI об изменении коллекции (на всякий случай)
                OnPropertyChanged(nameof(ClassNames));

                IsLibraryLoaded = true;
                ExecutionResult = $"✅ Библиотека загружена. Найдено классов: {ClassNames.Count}\n" +
                                $"Интерфейс: {_interfaceType.FullName}";
            }
            catch (ReflectionTypeLoadException ex)
            {
                var loaderExceptions = string.Join("\n", ex.LoaderExceptions
                    .Where(e => e != null).Select(e => e.Message));
                ExecutionResult = $"❌ Ошибка загрузки типов:\n{loaderExceptions}";
                IsLibraryLoaded = false;
            }
            catch (Exception ex)
            {
                ExecutionResult = $"❌ Ошибка: {ex.Message}\n\n{ex.InnerException?.Message}";
                IsLibraryLoaded = false;
            }
        }

        private void LoadMethods()
        {
            Methods.Clear();
            _selectedType = null;
            SelectedMethod = null;

            if (string.IsNullOrEmpty(SelectedClassName) || _loadedAssembly == null)
                return;

            _selectedType = _loadedAssembly.GetType(SelectedClassName);
            if (_selectedType == null)
            {
                ExecutionResult = $"❌ Тип {SelectedClassName} не найден!";
                return;
            }

            // 🔹 Получаем ВСЕ публичные методы
            var allMethods = _selectedType.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            // 🔹 Фильтруем
            var methods = allMethods
                .Where(m =>
                {
                    // Исключаем методы из базовых классов object и интерфейса
                    if (m.DeclaringType == typeof(object))
                        return false;

                    if (_interfaceType != null && m.DeclaringType == _interfaceType)
                        return false;

                    // Исключаем специальные имена (get_, set_, add_, remove_)
                    if (m.IsSpecialName)
                        return false;

                    // Оставляем только обычные методы
                    return true;
                })
                .OrderBy(m => m.Name)
                .ThenBy(m => m.GetParameters().Length)
                .ToList();

            // 🔹 Добавляем в коллекцию
            foreach (var method in methods)
            {
                Methods.Add(new MethodDisplay(method));
            }

            OnPropertyChanged(nameof(Methods));

            // Для отладки - покажем сколько методов найдено
            ExecutionResult += $"\n\n📋 Найдено методов: {methods.Count}";
        }

        private bool CanExecuteMethod() =>
            IsLibraryLoaded && SelectedMethod != null && _selectedType != null;

        private void ExecuteSelectedMethod()
        {
            if (SelectedMethod == null || _selectedType == null) return;

            try
            {
                object instance = Activator.CreateInstance(_selectedType);

                ParameterInfo[] paramInfos = SelectedMethod.MethodInfo.GetParameters();
                object[] parameters = null;

                if (paramInfos.Length > 0)
                {
                    string[] inputs = ParametersInput?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        ?? Array.Empty<string>();

                    if (inputs.Length < paramInfos.Length)
                    {
                        ExecutionResult = $"❌ Ошибка параметров:\n" +
                                        $"Требуется: {paramInfos.Length}, введено: {inputs.Length}\n" +
                                        $"Формат: значение1, значение2, ...";
                        return;
                    }

                    parameters = new object[paramInfos.Length];
                    for (int i = 0; i < paramInfos.Length; i++)
                    {
                        try
                        {
                            string rawValue = inputs[i].Trim();
                            Type targetType = paramInfos[i].ParameterType;

                            if (targetType.IsEnum)
                                parameters[i] = Enum.Parse(targetType, rawValue, true);
                            else if (targetType == typeof(bool))
                                parameters[i] = bool.Parse(rawValue);
                            else if (targetType == typeof(string))
                                parameters[i] = rawValue;
                            else
                                parameters[i] = Convert.ChangeType(rawValue, targetType);
                        }
                        catch (Exception ex)
                        {
                            ExecutionResult = $"❌ Ошибка параметра #{i + 1} «{paramInfos[i].Name}»:\n" +
                                            $"Тип: {paramInfos[i].ParameterType.Name}\n" +
                                            $"Значение: «{inputs[i].Trim()}»\n" +
                                            $"Детали: {ex.Message}";
                            return;
                        }
                    }
                }

                object result = SelectedMethod.MethodInfo.Invoke(instance, parameters);

                ExecutionResult = $"✅ Метод выполнен!\n" +
                                $"Результат: {result?.ToString() ?? "void"}\n\n";

                var getInfoMethod = _selectedType.GetMethod("GetInfo", Type.EmptyTypes);
                if (getInfoMethod != null && getInfoMethod.ReturnType == typeof(string))
                {
                    var state = getInfoMethod.Invoke(instance, null) as string;
                    ExecutionResult += $"📊 Состояние: {state}";
                }
            }
            catch (TargetInvocationException ex)
            {
                var realEx = ex.InnerException ?? ex;
                ExecutionResult = $"❌ Ошибка выполнения метода:\n{realEx.Message}\n\n" +
                                $"Тип: {realEx.GetType().Name}";
            }
            catch (Exception ex)
            {
                ExecutionResult = $"❌ Ошибка: {ex.Message}\n\n" +
                                $"Тип: {ex.GetType().Name}";
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // 🔹 Вспомогательный класс для отображения метода
    public class MethodDisplay
    {
        public MethodInfo MethodInfo { get; }
        public string DisplayName { get; }

        public MethodDisplay(MethodInfo method)
        {
            MethodInfo = method;
            var parameters = string.Join(", ", method.GetParameters()
                .Select(p => $"{p.ParameterType.Name} {p.Name}"));
            DisplayName = $"{method.ReturnType.Name} {method.Name}({parameters})";
        }

        public override string ToString() => DisplayName;
    }

    // 🔹 Реализация ICommand
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;
        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();
        public void Execute(object parameter) => _execute();
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}