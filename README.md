# GrabasUI

GrabasUI is a Blazor component library providing reusable, customizable UI components for modern web applications. The library includes advanced data grid, dropdown, multiselect, and context menu components, designed for flexibility and ease of integration.

## Features

- **DataGrid**: Powerful, customizable data table with sorting, paging, filtering, and row selection (single/multi).
- **DropDown**: Generic dropdown component with search, custom display, and flexible styling.
- **MultiSelect**: Generic multi-select dropdown with search, custom display, and flexible styling.
- **ContextMenu**: Context menu with dynamic items and event handling.
- **GridColumn**: Declarative column configuration for DataGrid.

## Installation

1. Add the NuGet package to your Blazor project:

2. Register the required services in your `Program.cs`:

3. Add static assets and styles in your `App.razor` or `_Host.cshtml`:

## Usage

### DataGrid

#### DataGrid Parameters

- `TItem`: Type of the data item.
- `Items`: Data source.
- `Sortable`: Enable/disable sorting.
- `PageSizeOptions`: Array of page sizes.
- `PageNumber`: Current page.
- `PageSize`: Rows per page.
- `EnableRowSelection`: Enable row selection.
- `MultiSelectionEnabled`: Enable multi-row selection.
- `RowSelectionIdentifierProperty`: Unique property for row selection.
- `SelectedRowItemsChanged`: Event callback for selected rows.

### DropDown

#### DropDown Parameters

- `T`: Type of the data item.
- `Items`: Data source.
- `IdentifierProperty`: Unique property for selection.
- `DisplayProperty`: Property to display.
- `SelectedItem`: Currently selected item.
- `ValueChanged`: Event callback for selection.
- `CanSearch`: Enable search box.
- `SearchPlaceHolder`: Placeholder text for search.
- `Width`, `Height`, `CustomStyle`: Styling options.

### MultiSelect

#### MultiSelect Parameters

- `T`: Type of the data item.
- `Items`: Data source.
- `IdentifierProperty`: Unique property for selection.
- `DisplayProperty`: Property to display.
- `SelectedItems`: Currently selected items.
- `ValuesChanged`: Event callback for selection.
- `CanSearch`: Enable search box.
- `SearchPlaceHolder`: Placeholder text for search.
- `Width`, `Height`, `CustomStyle`: Styling options.

### ContextMenu

#### ContextMenu Parameters

- `Data`: Data context for the menu.
- `CssClass`: Custom CSS class.
- `Caption`: Menu caption.
- `ContextButtonCssClass`: CSS class for the context button.
- `ChildContent`: Menu items.

#### ContextMenuItem Parameters

- `Text`: Display text.
- `OnClick`: Click event callback.
- `Disabled`: Disable the item.

## Example

See the `Weather.razor` page for a full example combining DataGrid, DropDown, and MultiSelect.

## License

See [LICENSE.txt](./LICENSE.txt).

## Author

Antonio Glešiæ

## Repository

[https://github.com/antglesic/Grabas-UI-Package](https://github.com/antglesic/Grabas-UI-Package)
