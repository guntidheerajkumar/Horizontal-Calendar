using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using PropertyChanged;
using Xamarin.Forms;

namespace HorizontalCalendar.Controls
{
	public partial class CalendarControl : ContentView
	{
		public static readonly BindableProperty SelectedDateProperty =
			BindableProperty.Create(nameof(SelectedDate), typeof(DateTime), typeof(CalendarControl), DateTime.MinValue, BindingMode.TwoWay,
									null);

		public static readonly BindableProperty MaximumDateProperty =
			BindableProperty.Create(nameof(MaximumDate), typeof(DateTime), typeof(CalendarControl), DateTime.MaxValue, BindingMode.TwoWay,
									propertyChanged: MaximumHandleBindingPropertyChangedDelegate);

		public static readonly BindableProperty MinimumDateProperty =
			BindableProperty.Create(nameof(MinimumDate), typeof(DateTime), typeof(CalendarControl), DateTime.MinValue, BindingMode.TwoWay,
							propertyChanged: MinimumHandleBindingPropertyChangedDelegate);

		public delegate void OnSelectedDateChangedDelegate();

		public OnSelectedDateChangedDelegate OnSelectedDateChanged { get; set; }

		public DateTime SelectedDate
		{
			get { return (DateTime)GetValue(SelectedDateProperty); }
			set { SetValue(SelectedDateProperty, value); }
		}

		private Grid horizontalCalendarGrid = new Grid();

		public static CalendarControlModel viewModel { get; set; }

		public DateTime MaximumDate
		{
			get { return (DateTime)GetValue(MaximumDateProperty); }
			set { SetValue(MaximumDateProperty, value); }
		}

		public DateTime MinimumDate
		{
			get { return (DateTime)GetValue(MinimumDateProperty); }
			set { SetValue(MinimumDateProperty, value); }
		}

		static void MaximumHandleBindingPropertyChangedDelegate(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as CalendarControl).MaximumDate = (DateTime)newValue;
			viewModel.MaximumDate = (DateTime)newValue;
		}

		static void MinimumHandleBindingPropertyChangedDelegate(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as CalendarControl).MinimumDate = (DateTime)newValue;
			viewModel.MinimumDate = (DateTime)newValue;
		}

		public CalendarControl()
		{
			InitializeComponent();
			Content.BindingContext = viewModel = new CalendarControlModel(this);
			MonthPicker();
			YearDataPicker();
		}

		/// <summary>
		/// Creates the calendar grid.
		/// </summary>
		/// <param name="isMonthSelected">If set to <c>true</c> is month selected.</param>
		public void CreateCalendarGrid(bool isMonthSelected = false)
		{
			horizontalCalendarGrid.Children.Clear();
			horizontalCalendarGrid.ColumnDefinitions.Clear();
			horizontalCalendarGrid.RowSpacing = 0;
			horizontalCalendarGrid.HorizontalOptions = LayoutOptions.StartAndExpand;
			horizontalCalendarGrid.VerticalOptions = LayoutOptions.Start;
			horizontalCalendarGrid.Padding = new Thickness(0, 0, 0, 0);
			horizontalCalendarGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

			for (int columnIndex = 0; columnIndex < viewModel.CalenderList.Count; columnIndex++)
			{
				horizontalCalendarGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = 50 });
			}

			int index = 0;
			foreach (Calender item in viewModel.CalenderList)
			{
				var contentView = new ContentView()
				{
					BindingContext = item,
					HeightRequest = 55
				};

				var layout = new StackLayout()
				{
					HorizontalOptions = LayoutOptions.FillAndExpand,
					Orientation = StackOrientation.Vertical,
					Spacing = 3
				};

				var dayName = new Label
				{
					FontSize = 11,
					FontAttributes = FontAttributes.None,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					HorizontalTextAlignment = TextAlignment.Center,
					Text = item.DayShortName
				};

				layout.Children.Add(dayName);

				var dayValue = new Label
				{
					FontSize = 16,
					FontAttributes = FontAttributes.None,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Start,
					HorizontalTextAlignment = TextAlignment.Center,
					Text = item.Day
				};

				layout.Children.Add(dayValue);

				var isActive = new Label { Text = item.IsActive ? "true" : "false", IsVisible = false };
				layout.Children.Add(isActive);


				var selectedArrowIndicator = new Image
				{
					Source = "arrow.png",
					HeightRequest = 12,
					WidthRequest = 12,
					HorizontalOptions = LayoutOptions.Center
				};

				layout.Children.Add(selectedArrowIndicator);

				contentView.Content = layout;

				horizontalCalendarGrid.Children.Add(contentView, index, 1);

				//Selecting the current Date and changing the opacity to highlight the current date
				if (viewModel.CalenderList[index].Day == Convert.ToString(DateTime.Today.Day)
					&& viewModel.CalenderList[index].Month == DateTime.Today.Month && viewModel.CalenderList[index].Year == DateTime.Today.Year)
				{
					selectedArrowIndicator.IsVisible = true;
					layout.Opacity = 1f;
					dayName.Text = "TODAY";
					dayName.TextColor = (Color)Application.Current.Resources["SelectedDayText"];
					dayValue.TextColor = (Color)Application.Current.Resources["SelectedDayText"];
				}
				else
				{
					selectedArrowIndicator.IsVisible = false;
					dayName.TextColor = (Color)Application.Current.Resources["NonSelectedActiveDaysText"];
					dayValue.TextColor = (Color)Application.Current.Resources["NonSelectedActiveDaysText"];
				}

				//Tap Gesture for Selecting the date
				var tapGestureRecognizer = new TapGestureRecognizer();
				tapGestureRecognizer.Tapped += OnSelectedDateChangedEvent;

				//This is to make empty cells to have white background and non clickable.
				if (string.IsNullOrWhiteSpace(item.DayShortName))
				{
					layout.BackgroundColor = (Color)Application.Current.Resources["ContentBackground"];
					layout.IsEnabled = false;
				}
				else
				{
					dayName.TextColor = item.IsActive
						? (Color)Application.Current.Resources["NonSelectedActiveDaysText"]
						: (Color)Application.Current.Resources["InactiveDaysText"];

					dayValue.TextColor = item.IsActive
						? (Color)Application.Current.Resources["NonSelectedActiveDaysText"]
						: (Color)Application.Current.Resources["InactiveDaysText"];

					layout.IsEnabled = item.IsActive;

					if (item.IsActive)
					{
						layout.GestureRecognizers.Add(tapGestureRecognizer);
					}
				}

				index += 1;
			}

			calendarScrollView.Content = horizontalCalendarGrid;
			calendarScrollView.ForceLayout();

			if (isMonthSelected)
			{
				Device.BeginInvokeOnMainThread(() =>
				 {
					 var currentDate = viewModel.CalenderList.Where((obj) => obj.Day == Convert.ToString(DateTime.Today.Day)).FirstOrDefault();
					 if (viewModel.Month == DateTime.Today.Month && Convert.ToInt32(viewModel.Year) == DateTime.Today.Year)
					 {
						 var layout = GetDateLayout(Convert.ToString(DateTime.Today.Day));
						 calendarScrollView.ScrollToAsync(layout, ScrollToPosition.Center, true);
					 }
					 else
					 {
						 calendarScrollView.ScrollToAsync(0, 0, true);
					 }
				 });
			}
			else
			{
				Device.BeginInvokeOnMainThread(() =>
				 {
					 //Scroll To the current Date
					 var layout = GetDateLayout(Convert.ToString(DateTime.Today.Day));
					 calendarScrollView.ScrollToAsync(layout, ScrollToPosition.Center, true);
				 });
			}
		}

		/// <summary>
		/// Gets the selected date layout.
		/// </summary>
		/// <returns>The date layout.</returns>
		/// <param name="date">Date.</param>
		private StackLayout GetDateLayout(string date)
		{
			StackLayout selectedDateLayout = null;
			bool isActive = false;
			foreach (var item in horizontalCalendarGrid.Children)
			{
				var contentView = (ContentView)item;
				selectedDateLayout = (StackLayout)contentView?.Content;
				foreach (var layoutItem in selectedDateLayout.Children)
				{
					var dayValue = (Label)selectedDateLayout.Children[0];
					var dateValue = (Label)selectedDateLayout.Children[1];
					if (dateValue.Text.Trim() == date)
					{
						dayValue.TextColor = (Color)Application.Current.Resources["SelectedDayText"];
						dateValue.TextColor = (Color)Application.Current.Resources["SelectedDayText"];
						isActive = true;
						break;
					}
					break;
				}
				if (isActive)
					break;
			}
			return selectedDateLayout;
		}

		void Handle_Tapped(object sender, System.EventArgs e)

		{
			Device.BeginInvokeOnMainThread(() =>
			{
				MonthNamePicker.Focus();
				MonthNamePicker.SelectedIndex = viewModel.MonthList.FindIndex((obj) => obj.MonthName.ToUpperInvariant().Equals(viewModel.MonthName));
			});
		}

		void Year_Tapped(object sender, System.EventArgs e)

		{
			Device.BeginInvokeOnMainThread(() =>
			{
				YearPicker.Focus();
				YearPicker.SelectedIndex = viewModel.YearList.FindIndex((obj) => obj.Equals(viewModel.Year));
			});
		}

		/// <summary>
		/// Months the picker.
		/// </summary>
		private void MonthPicker()
		{
			MonthNamePicker.ItemsSource = viewModel.MonthList;
			MonthNamePicker.TextColor = (Color)Application.Current.Resources["MainText"];
		}

		private void YearDataPicker()
		{
			YearPicker.ItemsSource = viewModel.YearList;
			YearPicker.TextColor = (Color)Application.Current.Resources["MainText"];
		}

		private void OnSelectedDateChangedEvent(object sender, EventArgs args)
		{
			if (OnSelectedDateChanged != null)
			{
				SelectedDateLayout(sender, args);
				OnSelectedDateChanged();
			}
		}

		/// <summary>
		/// Selecteds the date layout.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		private void SelectedDateLayout(object sender, EventArgs e)
		{
			var selectedLayout = (StackLayout)sender;
			var selectedChildren = selectedLayout.Children;
			Label selectedDate = null;
			foreach (var item in horizontalCalendarGrid.Children)
			{
				var contentView = (ContentView)item;
				var layout = (StackLayout)contentView?.Content;
				foreach (var layoutItem in layout.Children)
				{
					if (layoutItem.GetType() == typeof(Image))
					{
						var imageItem = (Image)layoutItem;
						imageItem.IsVisible = false;
					}
				}
			}

			///from the selected children, making arrow visible for the selected item. 
			///Adjusting the scroll position.
			foreach (var item in selectedChildren)
			{
				selectedDate = (Label)selectedChildren[1];
				if (item.GetType() == typeof(Image))
				{
					((Image)item).IsVisible = true;
					calendarScrollView.ScrollToAsync(selectedLayout, ScrollToPosition.Center, true);
				}
			}

			viewModel.SelectedDate = new DateTime(viewModel.YearValue, viewModel.Month, Convert.ToInt32(selectedDate.Text));
			SelectedDate = new DateTime(viewModel.YearValue, viewModel.Month, Convert.ToInt32(selectedDate.Text));

			///Making opacity to 0.6 for all the items except the one which is selected
			foreach (var item in horizontalCalendarGrid.Children)
			{
				var contentView = (ContentView)item;
				var layout = (StackLayout)contentView?.Content;
				foreach (var layoutItem in layout.Children)
				{
					var dateLabel = (Label)layout.Children[0];
					var dateValue = (Label)layout.Children[1];
					var isActive = (Label)layout.Children[2];
					if (dateValue.Text.Trim() == selectedDate.Text.Trim())
					{
						dateLabel.TextColor = (Color)Application.Current.Resources["SelectedDayText"];
						dateValue.TextColor = (Color)Application.Current.Resources["SelectedDayText"];
					}
					else
					{
						dateLabel.TextColor = isActive.Text == "true"
						? (Color)Application.Current.Resources["NonSelectedActiveDaysText"]
						: (Color)Application.Current.Resources["InactiveDaysText"];

						dateValue.TextColor = isActive.Text == "true"
							? (Color)Application.Current.Resources["NonSelectedActiveDaysText"]
							: (Color)Application.Current.Resources["InactiveDaysText"];

					}
				}
			}
		}
	}

	/// <summary>
	/// View Model for Calendar View
	/// </summary>
	public class CalendarControlModel : BaseViewModel
	{
		//Calendar Collection which holds all the days of specific month
		ObservableCollection<Calender> _calenderList = new ObservableCollection<Calender>();
		public ObservableCollection<Calender> CalenderList
		{
			get
			{
				return _calenderList;
			}

			set
			{
				_calenderList = value;
				RaisePropertyChanged();
			}
		}

		/// <summary>
		/// Month Generic List for Picker
		/// </summary>
		List<Months> _monthList = new List<Months>();

		/// <summary>
		/// Selected Month Index
		/// </summary>
		int monthSelectedIndex = DateTime.Today.Month - 1;
		public int MonthSelectedIndex
		{
			get
			{
				return monthSelectedIndex;
			}
			set
			{
				if (monthSelectedIndex != value)
				{
					monthSelectedIndex = value;
					RaisePropertyChanged();
					string monthValue = _monthList[monthSelectedIndex].MonthName;
					MonthName = monthValue.ToUpper();
					Month = monthSelectedIndex + 1;
					ChangeMonth();
				}
			}
		}

		int yearSelectedIndex = DateTime.Today.Year - 1;
		public int YearSelectedIndex
		{
			get
			{
				return yearSelectedIndex;
			}
			set
			{
				if (yearSelectedIndex != value)
				{
					yearSelectedIndex = value;
					RaisePropertyChanged();
					YearValue = Convert.ToInt32(_yearList[yearSelectedIndex]);
					Year = _yearList[yearSelectedIndex];
					ChangeYear();
				}
			}
		}

		/// <summary>
		/// Validation for Maximum Date 
		/// </summary>
		DateTime maximumDate = DateTime.MaxValue;
		public DateTime MaximumDate
		{
			get
			{
				return maximumDate;
			}

			set
			{
				maximumDate = value;
				CreateCalenderData();
				var a = this._contentView as CalendarControl;
				if (a != null)
				{
					a.CreateCalendarGrid(true);
				}
				RaisePropertyChanged();
			}
		}

		/// <summary>
		/// Validation for Minimum Date
		/// </summary>
		DateTime minimumDate;
		public DateTime MinimumDate
		{
			get
			{
				return minimumDate;
			}

			set
			{
				minimumDate = value;
				CreateCalenderData();
				var a = this._contentView as CalendarControl;
				if (a != null)
				{
					a.CreateCalendarGrid(true);
				}
				RaisePropertyChanged();
			}
		}

		public List<Months> MonthList
		{
			get
			{
				return _monthList;
			}
			set
			{
				_monthList = value;
				RaisePropertyChanged();
			}
		}

		List<string> _yearList = new List<string>();
		public List<string> YearList
		{
			get
			{
				return _yearList;
			}
			set
			{
				_yearList = value;
				RaisePropertyChanged();
			}
		}

		/// <summary>
		/// Month Name to display in the Calendar Header
		/// </summary>
		string monthName = DateTime.Today.ToString("MMMMM").ToUpper();
		public string MonthName
		{
			get
			{
				return monthName;
			}

			set
			{
				monthName = value;
				RaisePropertyChanged();
			}
		}

		/// <summary>
		/// Year Name to display in the Calendar Header
		/// </summary>
		string year = Convert.ToString(DateTime.Today.Year);
		public string Year
		{
			get
			{
				return year;
			}

			set
			{
				year = value;
				RaisePropertyChanged();
			}
		}

		int yearValue = DateTime.Today.Year;
		public int YearValue
		{
			get
			{
				return yearValue;
			}

			set
			{
				yearValue = value;
				RaisePropertyChanged();
			}
		}

		/// <summary>
		/// Selected Date from the Calendar
		/// </summary>
		DateTime selectedDate = DateTime.Today;
		public DateTime SelectedDate
		{
			get
			{
				return selectedDate;
			}

			set
			{
				selectedDate = value;
				RaisePropertyChanged();
			}
		}

		int month = DateTime.Today.Month;
		public int Month
		{
			get
			{
				return month;
			}

			set
			{
				month = value;
				RaisePropertyChanged();
				CreateCalenderData(YearValue, value);
			}
		}
		bool isCalendarAvailable = false;

		public bool IsCalendarAvailable
		{
			get
			{
				return isCalendarAvailable;
			}

			set
			{
				isCalendarAvailable = value;
				RaisePropertyChanged();
			}
		}

		private ContentView _contentView;

		public CalendarControlModel(ContentView contentView)
		{
			this._contentView = contentView;
			int i = 0;
			foreach (string mName in DateTimeFormatInfo.CurrentInfo.MonthNames)
			{
				if (!string.IsNullOrWhiteSpace(mName))
				{
					_monthList.Add(new Months() { MonthIndex = i.ToString(), MonthName = mName });
					i += 1;
				}
			}

			for (int yearIndex = 2017; yearIndex < 2100; yearIndex++)
			{
				_yearList.Add(yearIndex.ToString());
			}
		}

		/// <summary>
		/// Creates the calender data.
		/// </summary>
		/// <param name="currentYear">Current year.</param>
		/// <param name="currentMonth">Current month.</param>
		public void CreateCalenderData(int currentYear = 0, int currentMonth = 0)
		{
			CalenderList = new ObservableCollection<Calender>();
			if (currentYear == 0 && currentMonth == 0)
			{
				currentYear = DateTime.Today.Year;
				currentMonth = DateTime.Today.Month;
			}

			var dt = new DateTime(currentYear, currentMonth, 1);
			MonthName = dt.ToString("MMMMM").ToUpper();
			Year = Convert.ToString(dt.Year);

			CalenderList.Clear();

			var days = DateTime.DaysInMonth(currentYear, currentMonth);

			//Adding empty spaces before starting day of the month 
			AddEmptyRowsForCalendarList(currentMonth);

			for (int dayIndex = 1; dayIndex <= days; dayIndex++)
			{
				DateTime now = new DateTime(currentYear, currentMonth, dayIndex);
				DateTime minDate = new DateTime(minimumDate.Year, minimumDate.Month, 1);
				DateTime maxDate = new DateTime(maximumDate.Year, maximumDate.Month, 1);
				string dayName = Convert.ToString(now.DayOfWeek);
				bool isActive = true;

				if (minDate < minimumDate && now < minimumDate)
				{
					isActive = false;
				}
				else if (now >= minDate && now <= maximumDate)
				{
					isActive = true;
				}
				else
				{
					isActive = false;
				}

				CalenderList.Add(new Calender()
				{
					Month = currentMonth,
					DayShortName = now.ToString("ddd").ToUpper(),
					Day = Convert.ToString(dayIndex),
					Year = yearValue,
					DayFullName = dayName,
					IsSelectedDate = false,
					IsActive = isActive
				});
			}

			//Adding empty spaces before starting day of the month 
			AddEmptyRowsForCalendarList(currentMonth);

			IsCalendarAvailable = CalenderList.Count > 0 ? true : false;
		}

		private void AddEmptyRowsForCalendarList(int currentMonth)
		{
			for (int i = 0; i < 3; i++)
			{
				CalenderList.Add(new Calender()
				{
					Month = currentMonth,
					DayShortName = string.Empty,
					Day = string.Empty,
					DayFullName = string.Empty,
					IsSelectedDate = false,
					IsActive = true
				});
			}
		}

		/// <summary>
		/// Changes the month.
		/// </summary>
		public void ChangeMonth()
		{
			var dt = new DateTime(YearValue, this.month, 1);
			MonthName = dt.ToString("MMMMM").ToUpper();
			Year = Convert.ToString(dt.Year);
			CreateCalenderData(YearValue, Month);
			var a = this._contentView as CalendarControl;
			if (a != null)
			{
				a.CreateCalendarGrid(true);
			}
		}

		/// <summary>
		/// Changes the year.
		/// </summary>
		public void ChangeYear()
		{
			var dt = new DateTime(YearValue, this.month, 1);
			MonthName = dt.ToString("MMMMM").ToUpper();
			Year = Convert.ToString(dt.Year);
			CreateCalenderData(YearValue, Month);
			var a = this._contentView as CalendarControl;
			if (a != null)
			{
				a.CreateCalendarGrid(true);
			}
		}
	}

	#region Classes

	[ImplementPropertyChanged]
	public class Calender
	{
		public string DayShortName
		{
			get;
			set;
		}

		public string DayFullName
		{
			get;
			set;
		}

		public string Day
		{
			get;
			set;
		}

		public int Month
		{
			get;
			set;
		}

		public int Year
		{
			get;
			set;
		}

		public bool IsSelectedDate
		{
			get;
			set;
		}

		public bool IsActive
		{
			get;
			set;
		}
	}

	[ImplementPropertyChanged]
	public class Months
	{
		public string MonthName
		{
			get;
			set;
		}

		public string MonthIndex
		{
			get;
			set;
		}
	}

	public class BaseViewModel : INotifyPropertyChanged
	{
		public BaseViewModel()
		{
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void RaisePropertyChanged([CallerMemberName] string propertyName = "") =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	#endregion

}
