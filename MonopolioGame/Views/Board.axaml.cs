using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using MonopolioGame.ViewModels;
using System.Collections;
using System.Collections.ObjectModel;

namespace MonopolioGame.Views
{
    public partial class Board : UserControl
    {
        public static readonly StyledProperty<int> RowCountProperty =
            AvaloniaProperty.Register<Board, int>(nameof(RowCount));

        public int RowCount
        {
            get
            {
                return GetValue(RowCountProperty);
            }
            set
            {
                SetValue(RowCountProperty, value);
            }
        }

        public static readonly StyledProperty<int> ColumnCountProperty =
            AvaloniaProperty.Register<Board, int>(nameof(ColumnCount));

        public int ColumnCount
        {
            get
            {
                return GetValue(ColumnCountProperty);
            }
            set
            {
                SetValue(ColumnCountProperty, value);
            }
        }

        public static readonly StyledProperty<DataTemplate> ItemsTemplateProperty =
            AvaloniaProperty.Register<Board, DataTemplate>(nameof(ItemsTemplate));

        public DataTemplate ItemsTemplate
        {
            get
            {
                return GetValue(ItemsTemplateProperty);
            }
            set
            {               
                SetValue(ItemsTemplateProperty, value);
                CreateBoard();
            }
        }

        public static readonly StyledProperty<IEnumerable> ItemsSourceProperty =
            AvaloniaProperty.Register<Board, IEnumerable>(nameof(ItemsSource));

        public IEnumerable ItemsSource
        {
            get
            {
                return GetValue(ItemsSourceProperty);
            }
            set
            {
                SetValue(ItemsSourceProperty, value);
                //CreateBoard();
            }
        }

        private IEnumerable[] tempList;

        public Board()
        {
            InitializeComponent();
            CreateBoard();
        }

        private void InitializeComponent()
        {          
            AvaloniaXamlLoader.Load(this);           
        }

        private void FillTempList()
        {
            int i = 0;
            tempList = new IEnumerable[ 2 * (RowCount + ColumnCount) - 4];
            foreach (var v in ItemsSource)
            {
                object[] temp = new object[1];
                temp[0] = v;
                tempList[i++] = new ObservableCollection<object>(temp);
            }
        }

        private void CreateGridCells()
        {
            Grid grid = this.FindControl<Grid>("TheGrid");

            for (int i = 0; i < RowCount; i++)
                grid.RowDefinitions.Add(new RowDefinition(new GridLength(1.0, GridUnitType.Star)));

            for (int i = 0; i < ColumnCount; i++)
                grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1.0, GridUnitType.Star)));
        }
        
        private void UpdateDeltas(int row, int column, ref int rowDelta, ref int columnDelta)
        {
            if(row == RowCount - 1 && column == 0)
            {
                rowDelta = -1;
                columnDelta = 0;
            }
            else if(row == 0 && column == 0)
            {
                rowDelta = 0;
                columnDelta = 1;
            }
            else if(row == 0 && column == ColumnCount - 1)
            {
                rowDelta = 1;
                columnDelta = 0;
            }
        }

        private void FillCells()
        {
            Grid grid = this.FindControl<Grid>("TheGrid");
            grid.Children.Clear();

            int row = RowCount - 1, column = ColumnCount - 1;
            int rowDelta = 0, columnDelta = -1;

            foreach(IEnumerable e in tempList)
            {
                ItemsControl c = new ItemsControl
                {
                    Items = e,
                    ItemTemplate = ItemsTemplate,
                };

                Grid.SetColumn(c, column);
                Grid.SetRow(c, row);
                grid.Children.Add(c);
                UpdateDeltas(row, column, ref rowDelta, ref columnDelta);
                row += rowDelta;
                column += columnDelta;
            }
            return;
        }

        internal void CreateBoard()
        {
            RowCount = ColumnCount = 11;
            var temp = new ObservableCollection<PropertyViewModel>();
            for (int i = 0; i < 40; i++)
            {
                IBrush c = (i % 2 == 0) ? new SolidColorBrush(Colors.Red, 1) : new SolidColorBrush(Colors.Pink, 1);

                //temp.Add(new PropertyViewModel(c, i));
            }

            ItemsSource = temp;
            if (ItemsSource == null)
                return;

            FillTempList();
            CreateGridCells();
            FillCells();
        }
    }
}
