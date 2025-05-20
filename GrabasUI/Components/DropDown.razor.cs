using Microsoft.AspNetCore.Components;

namespace GrabaUIPackage.Components
{
	public partial class DropDown<T> : ComponentBase
	{
		#region Parameters

		[Parameter]
		public required IEnumerable<T> Items { get; set; }

		[Parameter]
		public required string IdentifierProperty { get; set; }

		[Parameter]
		public required string DisplayProperty { get; set; }

		[Parameter]
		public EventCallback<T> ValueChanged { get; set; }

		[Parameter]
		public bool CanSearch { get; set; } = false;

		[Parameter]
		public string SearchPlaceHolder { get; set; } = "Search...";

		[Parameter]
		public string Height { get; set; } = string.Empty;

		[Parameter]
		public string Width { get; set; } = "200px";

		[Parameter]
		public string CustomStyle { get; set; } = string.Empty;

		[Parameter]
		public T? SelectedItem { get; set; }

		[Parameter]
		public string ComponentId { get; set; } = Guid.NewGuid().ToString("N");

		#endregion

		#region Properties

		private List<T> ItemList = new List<T>();
		private List<T> FilteredItemList = new List<T>();
		private bool isDropdownOpen = false;
		private T? SelectedValue { get; set; }

		#endregion

		#region Methods

		protected override async Task OnParametersSetAsync()
		{
			if (Items != null && Items.Any())
			{
				// Check if ItemList is empty or if the items have changed
				if (ItemList.Count == 0 || !ItemsAreEqual(ItemList, Items))
				{
					ItemList = Items.ToList();      // Initialize ItemList with the provided items
					FilteredItemList = ItemList;    // Initialize the filtered list with all items

					// Check if SelectedItem is not null
					if (SelectedItem != null)
					{
						// If SelectedItems is not equal to the current SelectedValue, update SelectedValue so that the UI reflects the changes
						SelectedValue = SelectedItem;
					}
				}
			}

			await base.OnParametersSetAsync();
		}

		// This method checks if the items in both lists are equal.
		private bool ItemsAreEqual(IEnumerable<T> list1, IEnumerable<T> list2)
		{
			var identifierProperty = typeof(T).GetProperty(IdentifierProperty);
			if (identifierProperty == null)
			{
				throw new InvalidOperationException($"Property '{IdentifierProperty}' not found on type '{typeof(T).Name}'");
			}

			var list1Identifiers = list1.Select(item => identifierProperty.GetValue(item)).ToList();
			var list2Identifiers = list2.Select(item => identifierProperty.GetValue(item)).ToList();

			return list1Identifiers.SequenceEqual(list2Identifiers);
		}

		// Helper method to get property value dynamically via reflection
		private object? GetPropertyValue(T item, string propertyName)
		{
			var propertyInfo = typeof(T).GetProperty(propertyName);
			if (propertyInfo == null)
			{
				throw new InvalidOperationException($"Property '{propertyName}' not found on type '{typeof(T).Name}'");
			}
			return propertyInfo.GetValue(item);
		}

		// This method will be called to filter the items based on the search text
		private async Task FilterItems(ChangeEventArgs e)
		{
			var filterText = e.Value?.ToString() ?? string.Empty;
			if (string.IsNullOrWhiteSpace(filterText))
			{
				FilteredItemList = ItemList; // Show all items when search text is empty
			}
			else
			{
				// Filter items based on DisplayProperty (search text matching any part of the display property)
				FilteredItemList = ItemList.Where(item =>
				{
					var displayValue = GetPropertyValue(item, DisplayProperty)?.ToString();
					return displayValue != null && displayValue.Contains(filterText, StringComparison.CurrentCultureIgnoreCase);
				}).ToList();
			}

			await InvokeAsync(StateHasChanged);
		}

		// Triggered when the selection changes
		private async Task OnValueChanged(T item)
		{
			try
			{
				if (item == null)
				{
					throw new ArgumentNullException(nameof(item));
				}

				if (item != null)
				{
					SelectedValue = item;
					isDropdownOpen = false;
					FilteredItemList = ItemList; // Reset the filtered list to show all items

					if (ValueChanged.HasDelegate)
					{
						await ValueChanged.InvokeAsync(SelectedValue);
					}

					// Re-render the component
					await InvokeAsync(StateHasChanged);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				throw new InvalidOperationException($"Error in OnValueChanged: {ex.Message}", ex);
			}
		}

		// Toggle the dropdown visibility
		private void ToggleDropdown()
		{
			isDropdownOpen = !isDropdownOpen;
			StateHasChanged();
		}

		// Gets the CSS styling for the Height property
		private string GetHeight()
		{
			if (string.IsNullOrWhiteSpace(Height))
			{
				return string.Empty;
			}
			else
			{
				return $"height: {(string.IsNullOrWhiteSpace(Height) ? string.Empty : Height)};";
			}
		}

		// Gets the CSS styling for the Width property
		private string GetWidth()
		{
			return $"width: {Width};";
		}

		// Gets the custom CSS styling if provided through the CustomStyle parameter
		private string GetCustomStyle()
		{
			if (string.IsNullOrWhiteSpace(CustomStyle))
			{
				return string.Empty;
			}
			else
			{
				return $"{(string.IsNullOrWhiteSpace(CustomStyle) ? string.Empty : CustomStyle)};";
			}
		}


		#endregion
	}
}
