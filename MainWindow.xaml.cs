using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace ContrabandAge
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Dictionary<int, string> monthDic = new()
        {
            { 1, "JAN" },
            { 2, "FEB" },
            { 3, "MAR" },
            { 4, "APR" },
            { 5, "MAY" },
            { 6, "JUN" },
            { 7, "JUL" },
            { 8, "AUG" },
            { 9, "SEP" },
            { 10, "OCT" },
            { 11, "NOV" },
            { 12, "DEC" }
        };

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private int _age = 31;
        private string _ageString = "31";
        public string Age
        {
            get => _ageString.ToString();
            set
            {
                if (value.Trim() == "")
                {
                    _ageString = value.Trim();
                    return;
                }
                int newValue = int.Parse(OnlyNumber().Replace(value, ""));
                if (newValue != _age)
                {
                    _age = newValue;
                    _ageString = _age.ToString();
                    OnPropertyChanged(nameof(Age));
                    UpdateYearDiff();
                }
            }
        }

        private DateOnly _birthFrom = DateOnly.FromDayNumber(0);
        public string BirthFrom
        {
            get => $"{_birthFrom.Day:D2} {monthDic[_birthFrom.Month]} {_birthFrom.Year}";
        }

        private DateOnly _birthTo = DateOnly.FromDayNumber(0);
        public string BirthTo
        {
            get => $"{_birthTo.Day:D2} {monthDic[_birthTo.Month]} {_birthTo.Year}";
        }

        private DateOnly _currentDay = DateOnly.ParseExact("09.05.1979", "dd.MM.yyyy", CultureInfo.InvariantCulture);
        private string _currentDayString = "09.05.1979";
        public string CurrentDay
        {
            get => _currentDayString;
            set
            {
                bool success = DateOnly.TryParseExact(value, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly new_value);
                if (success && new_value != _currentDay)
                {
                    _currentDay = new_value;
                    _currentDayString = _currentDay.ToString("dd.MM.yyyy");
                    OnPropertyChanged(nameof(CurrentDay));
                    UpdateYearDiff();
                }
                else
                {
                    _currentDayString = value;
                }
            }
        }

        public void IncreaseDate()
        {
            CurrentDay = _currentDay.AddDays(1).ToString("dd.MM.yyyy");
        }

        public void DecreaseDate()
        {
            CurrentDay = _currentDay.AddDays(-1).ToString("dd.MM.yyyy");
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            UpdateYearDiff();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                {
                    DecreaseDate();
                }
                else
                {
                    IncreaseDate();
                }
            }
        }

        private void UpdateYearDiff()
        {
            _birthTo = _currentDay.AddYears(-1 * _age);
            _birthFrom = _birthTo.AddYears(-1).AddDays(1);
            OnPropertyChanged(nameof(BirthFrom));
            OnPropertyChanged(nameof(BirthTo));
        }

        [GeneratedRegex("[^0-9]")]
        private static partial Regex OnlyNumber();

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }
    }
}