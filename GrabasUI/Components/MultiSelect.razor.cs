using Microsoft.AspNetCore.Components;

namespace GrabasUI.Components
{
	public partial class MultiSelect<T> : ComponentBase
	{
		#region Parameters

		[Parameter]
		public required IEnumerable<T> Items { get; set; }

		[Parameter]
		public IEnumerable<T>? SelectedItems { get; set; }

		[Parameter]
		public required string IdentifierProperty { get; set; }

		[Parameter]
		public required string DisplayProperty { get; set; }

		[Parameter]
		public EventCallback<List<T>> ValuesChanged { get; set; }

		[Parameter]
		public bool CanSearch { get; set; } = true;

		[Parameter]
		public string SearchPlaceHolder { get; set; } = "Search...";

		[Parameter]
		public string Height { get; set; } = string.Empty;

		[Parameter]
		public string Width { get; set; } = "200px";

		[Parameter]
		public string CustomStyle { get; set; } = string.Empty;

		[Parameter]
		public string ComponentId { get; set; } = Guid.NewGuid().ToString("N");

		#endregion

		#region Properties

		private List<T> ItemList = new List<T>();
		private List<T> FilteredItemList = new List<T>();
		private List<T> SelectedValues = new List<T>();
		private bool isDropdownOpen = false;

		#endregion

		#region Methods

		protected override async Task OnParametersSetAsync()
		{
			// Check if Items is not null and has records
			if (Items != null && Items.Any())
			{
				// Check if ItemList is empty or if the items have changed
				if (ItemList.Count == 0 || !ItemsAreEqual(ItemList, Items))
				{
					ItemList = Items.ToList();      // Initialize ItemList with the provided items
					FilteredItemList = ItemList;    // Initialize the filtered list with all items

					// Check if SelectedItems is not null and has records
					if (SelectedItems != null && SelectedItems.Any())
					{
						// If SelectedItems is not equal to the current SelectedValues, update SelectedValues so that the UI reflects the changes
						SelectedValues = SelectedItems.ToList();
					}
				}
			}

			await base.OnParametersSetAsync();
		}

		// This method checks if the items in the list are equal based on the IdentifierProperty
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

		// This method retrieves the value of a property from an item using reflection
		private object? GetPropertyValue(T item, string propertyName)
		{
			var propertyInfo = typeof(T).GetProperty(propertyName);
			if (propertyInfo == null)
			{
				throw new InvalidOperationException($"Property '{propertyName}' not found on type '{typeof(T).Name}'");
			}
			return propertyInfo.GetValue(item);
		}

		// This method filters the list based on the current searchText immediately
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

		// Check if the item is selected based on the current selection
		private bool IsSelected(T item)
		{
			bool isSelected = false;
			var identifierValue = GetPropertyValue(item, IdentifierProperty);

			if (SelectedValues != null && SelectedValues.Any())
			{
				isSelected = SelectedValues.Any(selectedItem =>
				{
					var selectedIdentifierValue = GetPropertyValue((T)selectedItem, IdentifierProperty);
					return selectedIdentifierValue != null && selectedIdentifierValue.Equals(identifierValue);
				});
			}

			return isSelected;
		}

		// This method will handle changes in selection
		private async Task OnValueChanged(T item)
		{
			try
			{
				if (item == null)
				{
					throw new ArgumentNullException(nameof(item));
				}

				if (IsSelected(item))
				{
					// If the item is already selected, remove it
					var selectedItem = SelectedValues.FirstOrDefault(selected => GetPropertyValue((T)selected, IdentifierProperty)?.Equals(GetPropertyValue(item, IdentifierProperty)) == true);
					if (selectedItem != null)
					{
						SelectedValues.Remove(selectedItem);
					}
				}
				else
				{
					// If the item is not selected, add it
					SelectedValues.Add(item);
				}

				// Trigger the ValuesChanged callback
				if (ValuesChanged.HasDelegate)
				{
					await ValuesChanged.InvokeAsync(SelectedValues);
				}

				// Re-render the component
				await InvokeAsync(StateHasChanged);
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
			StateHasChanged(); // Force re-render
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

		// This method will remove an item from the selected values if it was selected prior
		private async Task<bool> RemoveItem(object Identifier)
		{
			bool retval = false;

			if (SelectedValues != null && SelectedValues.Any())
			{
				var itemToRemove = SelectedValues.FirstOrDefault(item =>
				{
					var propertyValue = GetPropertyValue((T)item, IdentifierProperty);
					return propertyValue != null && propertyValue.Equals(Identifier);
				});

				if (itemToRemove != null)
				{
					SelectedValues.Remove(itemToRemove);
					retval = true;
					await ValuesChanged.InvokeAsync(SelectedValues);
					await InvokeAsync(StateHasChanged);
				}
			}

			return retval;
		}

		#endregion
	}
}
