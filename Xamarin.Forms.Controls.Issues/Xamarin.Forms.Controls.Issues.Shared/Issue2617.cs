using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2617, "Error on binding ListView with duplicated items", PlatformAffected.UWP)]

#if UITEST
	[NUnit.Framework.Category(UITestCategories.ListView)]
#endif
	public class Issue2617 : TestContentPage
	{
		public Label SuccessLabel { get; private set; }
		public Button OneMillionButton { get; private set; }
		public Button ClearItemSourceButton { get; private set; }
		public ListView listView { get; private set; }
		public ListView listViewIsGrouped { get; private set; }

		class MyHeaderViewCell : ViewCell
		{
			public MyHeaderViewCell()
			{
				Height = 25;
				var label = new Label { VerticalOptions = LayoutOptions.Center };
				label.SetBinding(Label.TextProperty, nameof(GroupedItem.Name));
				View = label;
			}
		}

		class GroupedItem : List<string>
		{
			public GroupedItem()
			{
				AddRange(Enumerable.Range(0, 3).Select(i => "Group item"));
			}
			public string Name { get; set; }
		}

		protected override void Init()
		{
			listView = new ListView
			{
				ItemsSource = Enumerable.Range(0, 3).Select(x => "Item 1"),
				ItemTemplate = new DataTemplate(() =>
				{
					Label nameLabel = new Label();
					nameLabel.SetBinding(Label.TextProperty, new Binding("."));
					var cell = new ViewCell
					{
						View = new Frame()
						{
							Content = nameLabel
						},
					};
					return cell;
				}),
				AutomationId = "ListViewToScroll"
			};
			listViewIsGrouped = new ListView
			{
				ItemsSource = Enumerable.Range(0, 3).Select(x => new GroupedItem() { Name = $"Group {x}" }),
				IsGroupingEnabled = true,
				GroupHeaderTemplate = new DataTemplate(typeof(MyHeaderViewCell)),
				ItemTemplate = new DataTemplate(() =>
				{
					Label nameLabel = new Label();
					nameLabel.SetBinding(Label.TextProperty, new Binding("."));
					var cell = new ViewCell
					{
						View = new Frame()
						{
							Content = nameLabel
						},
					};
					return cell;
				})
			};

			SuccessLabel = new Label() { Text = "Wait For It" };
			OneMillionButton = new Button()
			{
				Text = "One Million",
				Command = new Command(() => listView.ItemsSource = Enumerable.Range(0, 1000000).Select(x => x == 999999 ? "Scroll to me" : "Item 1"))
			};
			ClearItemSourceButton = new Button()
			{
				Text = "Clear ItemsSource",
				Command = new Command(() => listView.ItemsSource = null)
			};

			Content = new StackLayout
			{
				Children =
				{
					SuccessLabel,
					OneMillionButton,
					ClearItemSourceButton,
					listView,
					listViewIsGrouped,
				}
			};
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			await Task.Delay(1000);
			ClearItemSourceButton.SendClicked();
			await Task.Delay(1000);
			OneMillionButton.SendClicked();
			await Task.Delay(1000);
			listViewIsGrouped.ItemsSource = null;
			await Task.Delay(100);
			listView.ScrollTo("Scroll to me", ScrollToPosition.Center, true);
			await Task.Delay(1000);
			SuccessLabel.HeightRequest = 200;
			SuccessLabel.Text = "Success";
			SuccessLabel.HorizontalTextAlignment = TextAlignment.Center;
			listView.ItemsSource = null;
		}


#if UITEST
		[Test]
		public void BindingToValuesTypesAndScrollingNoCrash()
		{
			RunningApp.WaitForElement("Success");
			RunningApp.ScrollDown("ListViewToScroll");
		}
#endif
	}
}