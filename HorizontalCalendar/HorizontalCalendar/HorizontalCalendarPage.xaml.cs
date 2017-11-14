using System;
using System.Collections.Generic;
using PropertyChanged;
using Xamarin.Forms;

namespace HorizontalCalendar
{
	[ImplementPropertyChanged]
	public partial class HorizontalCalendarPage : ContentPage
	{
		public DateTime MinimumDate { get; set; } = DateTime.Now;
		public DateTime MaximumDate { get; set; } = DateTime.Now.AddDays(30);

		public HorizontalCalendarPage()
		{
			InitializeComponent();
			this.BindingContext = this;
		}
	}
}
