﻿using GrabaUIPackage.Components.Common.EventArgs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace GrabaUIPackage.Components
{
	public partial class DataGrid<TItem> : ComponentBase
	{
		private int totalPages;
		private int currentPage;
		private int[]? pagingNumbers;
		private int internalPageSize;

		[Parameter]
		public RenderFragment? ChildContent { get; set; }

		[Parameter]
		public IEnumerable<TItem>? Items { get; set; }

		[Parameter]
		public bool Sortable { get; set; }

		[Parameter]
		public int? RowCount { get; set; }

		[Parameter]
		public bool? FrontendPagination { get; set; } = false;

		[Parameter]
		public EventCallback<PageChangedEventArgs> OnPageChanged { get; set; }

		[Parameter]
		public EventCallback<PageSizeChangedEventArgs> OnPageSizeChanged { get; set; }

		[Parameter]
		public int[]? PageSizeOptions { get; set; }

		[Parameter]
		public int PageNumber { get; set; }

		[Parameter]
		public bool EnableRowSelection { get; set; }

		[Parameter]
		public bool IgnoreEnableRowSelection { get; set; }

		[Parameter]
		public TItem? SelectedRowItem
		{
			get { return SelectedItem; }
			set
			{
				if (EqualityComparer<TItem>.Default.Equals(SelectedItem, value))
				{
					return;
				}
				SelectedItem = value;
				_ = SelectedRowItemChanged.InvokeAsync(value);
			}
		}

		[Parameter]
		public EventCallback<TItem> SelectedRowItemChanged { get; set; }

		[Parameter]
		public int PageSize
		{
			get => internalPageSize;
			set
			{
				if (EqualityComparer<int>.Default.Equals(internalPageSize, value))
				{
					return;
				}

				internalPageSize = value;
				_ = OnPageSizeChanged.InvokeAsync(new PageSizeChangedEventArgs { PageSize = value });
			}
		}

		[Parameter]
		public bool Filterable { get; set; }

		[Parameter]
		public object? FilterModel { get; set; }

		[Parameter]
		public EventCallback<object> OnSearch { get; set; }

		[Parameter]
		public string RowSelectionIdentifierProperty { get; set; } = string.Empty;

		[Parameter]
		public bool MultiSelectionEnabled { get; set; }

		[Parameter]
		public IEnumerable<TItem>? SelectedRowItems
		{
			get => SelectedItems;
			set
			{
				SelectedItems = value?.ToList() ?? [];
				_ = SelectedRowItemsChanged.InvokeAsync(SelectedItems);
			}
		}

		[Parameter]
		public EventCallback<IEnumerable<TItem>> SelectedRowItemsChanged { get; set; }

		[Parameter]
		public string? CustomCssClass { get; set; }

		public List<GridColumn<TItem>> Columns { get; } = [];
		private IEnumerable<TItem>? ItemList { get; set; }
		private bool isSortedAscending;
		private string? activeSortColumn;
		private TItem? SelectedItem;
		private string? enableSelectionCssClass;
		private Dictionary<string, string> columnFilterValues = [];
		private List<TItem> SelectedItems { get; set; } = [];

		public bool AreAllRowsSelected => ItemList != null && ItemList.Any() && ItemList.All(item => IsRowSelected(item));

		private bool IsRowSelected(TItem item)
		{
			if (MultiSelectionEnabled)
			{
				return SelectedItems.Contains(item);
			}
			else
			{
				return SelectedItem != null && SelectedItem.Equals(item);
			}
		}

		private void SelectAllRows(ChangeEventArgs e)
		{
			if (ItemList == null) return;

			if (e.Value is bool isChecked)
			{
				if (isChecked)
				{
					// Add all items that aren't already selected
					foreach (var item in ItemList.Where(item => !SelectedItems.Contains(item)))
					{
						SelectedItems.Add(item);
					}
				}
				else
				{
					// Remove all items that are currently displayed
					foreach (var item in ItemList.ToList())
					{
						SelectedItems.Remove(item);
					}
				}

				SelectedRowItems = SelectedItems;
			}
		}

		private void OnRowCheckboxChange(TItem item, ChangeEventArgs e)
		{
			if (e.Value is bool isChecked)
			{
				if (MultiSelectionEnabled)
				{
					if (isChecked && !SelectedItems.Contains(item))
					{
						SelectedItems.Add(item);
					}
					else if (!isChecked)
					{
						SelectedItems.Remove(item);
					}

					SelectedRowItems = SelectedItems;
				}
				else if (!MultiSelectionEnabled && EnableRowSelection)
				{
					if (isChecked && (SelectedItem == null || !SelectedItem.Equals(item)))
					{
						SelectedRowItem = item;
						SelectedItem = item;
					}
					else if (!isChecked && SelectedItem != null && SelectedItem.Equals(item))
					{
						SelectedRowItem = default;
						SelectedItem = default;
					}
				}
			}
		}

		protected override void OnParametersSet()
		{
			if (Filterable && FilterModel is null)
			{
				throw new ArgumentNullException(nameof(FilterModel));
			}

			if (PageSizeOptions == null)
			{
				PageSizeOptions = [10, 20, 30];
				if (internalPageSize == 0) // Only set if not already set
				{
					PageSize = 10;
				}
			}
			else if (internalPageSize == 0) // Only set if not already set
			{
				PageSize = PageSizeOptions.FirstOrDefault();
			}

			currentPage = PageNumber;
			if (currentPage == 0)
			{
				currentPage = 1;
			}

			if (Items != null)
			{
				if (RowCount.HasValue)
				{
					ItemList = Items;
				}
				else if (Items != null)
				{
					ItemList = Items.Skip((currentPage - 1) * PageSize).Take(PageSize);
				}

				CalculatePagingNumbers(currentPage, RowCount ?? (Items != null ? Items.Count() : 0));
			}

			if (EnableRowSelection)
			{
				enableSelectionCssClass = string.IsNullOrEmpty(CustomCssClass)
					? "table-hover"
					: $"{CustomCssClass} table-hover";
			}
			else
			{
				enableSelectionCssClass = CustomCssClass;
			}

			base.OnParametersSet();
		}

		public async Task UpdateList()
		{
			if (RowCount.HasValue)
			{
				await OnPageChanged.InvokeAsync(new PageChangedEventArgs() { CurrentPage = currentPage, PageSize = PageSize });
				await OnPageSizeChanged.InvokeAsync(new PageSizeChangedEventArgs() { PageSize = PageSize });
				ItemList = Items;
			}
			else
			{
				ItemList = Items?.Skip((currentPage - 1) * PageSize).Take(PageSize);

				if (FrontendPagination.HasValue && FrontendPagination.Value)
				{
					await OnPageChanged.InvokeAsync(new PageChangedEventArgs() { CurrentPage = currentPage, PageSize = PageSize });
					await OnPageSizeChanged.InvokeAsync(new PageSizeChangedEventArgs() { PageSize = PageSize });
				}
			}

			CalculatePagingNumbers(currentPage, RowCount ?? (Items != null ? Items.Count() : 0));
		}

		public async Task NavigateToPage(string direction)
		{
			if (direction == "next")
			{
				currentPage++;
			}
			else if (direction == "previous")
			{
				currentPage--;
			}

			await UpdateList();
		}

		public async Task SelectPage(int pageNumber)
		{
			currentPage = pageNumber;

			await UpdateList();
		}

		public void AddColumn(GridColumn<TItem> column)
		{
			column.Table = this;
			Columns.Add(column);
			StateHasChanged();
		}

		private int CalculatePagerItems()
		{
			int retVal = currentPage * PageSize;
			if (retVal > (RowCount ?? (Items != null ? Items.Count() : 0)))
			{
				retVal = RowCount ?? (Items != null ? Items.Count() : 0);
			}

			return retVal;
		}

		private void SortTable(string columnName)
		{
			if (ItemList != null)
			{
				if (columnName != activeSortColumn)
				{
					ItemList = [.. ItemList.OrderBy(x => x?.GetType()?.GetProperty(columnName)?.GetValue(x, null))];
					isSortedAscending = true;
					activeSortColumn = columnName;
				}
				else
				{
					if (isSortedAscending)
					{
						ItemList = [.. ItemList.OrderByDescending(x => x?.GetType()?.GetProperty(columnName)?.GetValue(x, null))];
					}
					else
					{
						ItemList = [.. ItemList.OrderBy(x => x?.GetType()?.GetProperty(columnName)?.GetValue(x, null))];
					}
					isSortedAscending = !isSortedAscending;
				}
			}
		}

		private async Task ChangePageSize(int value)
		{
			PageSize = value;
			currentPage = 1;

			await UpdateList();
		}

		private void CalculatePagingNumbers(int currentPage, int totalRows)
		{
			var defaultPagerSize = 7; // Default pager size moved here
			totalPages = (int)Math.Ceiling(totalRows / (decimal)PageSize);

			if (totalPages <= defaultPagerSize)
			{
				pagingNumbers = [.. Enumerable.Range(1, totalPages)];
			}
			else
			{
				int middlePosition = defaultPagerSize / 2;

				if (currentPage < middlePosition + 1)
				{
					pagingNumbers = [.. Enumerable.Range(1, defaultPagerSize - 1), .. new[] { totalPages }];
				}
				else if (currentPage > totalPages - middlePosition)
				{
					pagingNumbers = [.. new[] { 1 }, .. Enumerable.Range(totalPages - defaultPagerSize + 2, defaultPagerSize - 1)];
				}
				else
				{
					pagingNumbers = [.. new[] { 1 }, .. Enumerable.Range(currentPage - middlePosition + 1, defaultPagerSize - 2), totalPages];
				}
			}
		}

		private void OnRowClick(TItem item)
		{
			if (!EnableRowSelection) return;

			if (MultiSelectionEnabled)
			{
				if (!SelectedItems.Remove(item))
				{
					SelectedItems.Add(item);
				}

				// Invoke setter for SelectedRowItems so that the parent component can access the selected items
				SelectedRowItems = SelectedItems;
			}
			else
			{
				// Single selection mode
				SelectedItems.Clear();
				SelectedItems.Add(item);
				SelectedRowItem = item;
				SelectedItem = item;
			}
		}

		private string GetRowSelectedClass(TItem item)
		{
			List<string> classes = [];

			// Add custom class if provided
			if (!string.IsNullOrEmpty(CustomCssClass))
			{
				classes.Add(CustomCssClass);
			}

			if (IsRowSelected(item))
			{
				classes.Add("selected-row");
			}

			return string.Join(" ", classes);
		}

		private void OnInput(string filter, string property)
		{
			if (!string.IsNullOrEmpty(property))
			{
				var propertyInfo = FilterModel?.GetType()?.GetProperty(property);
				if (propertyInfo != null)
				{
					try
					{
						Type propertyType = propertyInfo.PropertyType;
						bool isNullable = IsNullableType(propertyType);

						if (isNullable)
						{
							propertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
						}

						object? convertedValue = string.IsNullOrEmpty(filter) && isNullable ? null : Convert.ChangeType(filter, propertyType);

						propertyInfo.SetValue(FilterModel, convertedValue);
						columnFilterValues[property] = filter;
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Error caught on filter input: {ex.Message}");
					}
				}
				else
				{
					throw new ArgumentException($"Property {property} doesn't exist in {nameof(FilterModel)}.");
				}
			}
		}

		private string GetFilterValue(string property)
		{
			if (string.IsNullOrEmpty(property))
				return string.Empty;

			return columnFilterValues.TryGetValue(property, out var value) ? value : string.Empty;
		}

		public static bool IsNullableType(Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}

		public void ClearFilter()
		{
			columnFilterValues = [];
		}

		async Task FilterTextKeyUp(KeyboardEventArgs args)
		{
			if (args.Key == "Enter")
			{
				await OnSearch.InvokeAsync(FilterModel);
			}
		}
	}
}