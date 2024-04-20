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
        // Dictionary to convert month number to month name
        private readonly Dictionary<int, string> _monthDic = new()
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

        private readonly Settings settings;

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;
        /// <summary>
        /// Raise PropertyChanged event
        /// </summary>
        /// <param name="propertyName"></param>
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Binding Properties
        // The age as a string
        private string _ageString;
        // The age as a binding property
        // accessing the _ageString variable
        public string Age
        {
            get => _ageString.ToString();
            set
            {
                // if the value is empty
                // only update the _ageString
                if (value.Trim() == "")
                {
                    _ageString = value.Trim();
                    return;
                }
                // parse the value to an integer
                // removing any non-numeric characters
                int newValue = int.Parse(OnlyNumber().Replace(value, ""));
                // if the new value is different from the current value
                if (newValue != settings.Age)
                {
                    // update the _age and _ageString
                    settings.Age = newValue;
                    _ageString = newValue.ToString();
                    // raise the PropertyChanged event
                    OnPropertyChanged(nameof(Age));
                    // and update the birth dates
                    UpdateBirthDates();
                }
            }
        }

        // The earliest birth date of the person
        private DateOnly _birthFrom = DateOnly.FromDayNumber(0);
        // The earliest birth date as a Binding property
        public string BirthFrom
        {
            // return the formatted date
            // with the day, month name and year and leading zeros
            get => $"{_birthFrom.Day:D2} {_monthDic[_birthFrom.Month]} {_birthFrom.Year}";
        }

        // The latest birth date of the person
        private DateOnly _birthTo = DateOnly.FromDayNumber(0);
        // The latest birth date as a Binding property
        public string BirthTo
        {
            // return the formatted date
            // with the day, month name and year and leading zeros
            get => $"{_birthTo.Day:D2} {_monthDic[_birthTo.Month]} {_birthTo.Year}";
        }

        // The current day as a string
        private string _currentDayString;
        // The current day as a Binding property
        public string CurrentDay
        {
            get => _currentDayString;
            set
            {
                // try to parse the value to a DateOnly object
                bool success = DateOnly.TryParseExact(value, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly new_value);
                // if the parsing was successful and the new value is different from the current value
                if (success && new_value != settings.CurrentDay)
                {
                    // update the _currentDay and _currentDayString
                    settings.CurrentDay = new_value;
                    _currentDayString = new_value.ToString("dd.MM.yyyy");
                    // raise the PropertyChanged event
                    OnPropertyChanged(nameof(CurrentDay));
                    // and update the birth dates
                    UpdateBirthDates();
                }
                // if the parsing was not successful
                // only update the _currentDayString
                else
                {
                    _currentDayString = value;
                }
            }
        }

        #endregion



        public MainWindow()
        {

            // initialize the settings
            settings = new Settings();

            // update the age and current day
            _ageString = settings.Age.ToString();
            _currentDayString = settings.CurrentDay.ToString("dd.MM.yyyy");

            settings.SyncChange += OnSyncChanged;
            InitializeComponent();
            // set the DataContext to this
            DataContext = this;

            // initialize the birth dates
            UpdateBirthDates();
        }

        private void OnSyncChanged()
        {
            _ageString = settings.Age.ToString();
            _currentDayString = settings.CurrentDay.ToString("dd.MM.yyyy");
            OnPropertyChanged(nameof(Age));
            OnPropertyChanged(nameof(CurrentDay));
            UpdateBirthDates();
        }

        #region Event Handlers

        /// <summary>
        /// Window KeyDown event handler
        /// to increase or decrease the date
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // if the key is Enter
            if (e.Key == Key.Enter)
            {
                // and the Control key is pressed
                if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                {
                    // decrease the date
                    DecreaseDate();
                }
                // otherwise increase the date
                else
                {
                    IncreaseDate();
                }
            }
        }

        /// <summary>
        /// Minimize button Click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Close button Click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Window MouseLeftButtonDown event handler
        /// to enable dragging the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        #endregion


        /// <summary>
        /// Increase the CurrentDay by one day
        /// </summary>
        public void IncreaseDate()
        {
            CurrentDay = settings.CurrentDay.AddDays(1).ToString("dd.MM.yyyy");
        }

        /// <summary>
        /// Decrease the CurrentDay by one day
        /// </summary>
        public void DecreaseDate()
        {
            CurrentDay = settings.CurrentDay.AddDays(-1).ToString("dd.MM.yyyy");
        }

        /// <summary>
        /// Update the BirthFrom and BirthTo dates
        /// </summary>
        private void UpdateBirthDates()
        {
            _birthTo = settings.CurrentDay.AddYears(-1 * settings.Age);
            _birthFrom = _birthTo.AddYears(-1).AddDays(1);
            OnPropertyChanged(nameof(BirthFrom));
            OnPropertyChanged(nameof(BirthTo));
        }

        /// <summary>
        /// OnlyNumber Regex at compile time
        /// </summary>
        /// <returns></returns>
        [GeneratedRegex("[^0-9]")]
        private static partial Regex OnlyNumber();


    }
}