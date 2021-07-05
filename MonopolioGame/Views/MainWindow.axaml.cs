using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using MonopolioGame.ViewModels;
using System;
using System.Threading;

namespace MonopolioGame.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            Closing += (s, e) => (DataContext as MainWindowViewModel)?.Closing();

            AvaloniaXamlLoader.Load(this);
            SetupBoard();
        }

        private void SetupBoard()
        {
            Grid boardGrid = this.FindControl<Grid>("board");

            for(int j = 0; j < 40; j++)
            {
                int i = j - (j / 10) - 1; //Exclude the corners from the index
                //Ignore the corners of the board, as those are already done
                if (j % 10 == 0)
                    continue;
                Grid grid = new Grid();
                grid.Tag = i.ToString();
                SetGridPosition(grid, j);
                CreateTopPanel(grid, i);
                CreateNameTB(grid, i);
                CreatePriceTB(grid, i);
                CreateBottomPanel(grid, i);
                grid.Tapped += (o,e) => (grid.DataContext as MainWindowViewModel).
                                PropertyClicked((GetGridIndex(o as Grid)));
                boardGrid.Children.Add(grid);                
            }
        }

        private int GetGridIndex(Grid grid)
        {
            return int.Parse(grid.Tag.ToString());
        }

        #region BoardSetupAux
        private static Tuple<int, int> GetCoords(int index)
        {
            int row, column;

            if (index < 10)
            {
                row = 10;
                column = 10 - index;
            }
            else if (index < 20)
            {
                row = 20 - index;
                column = 0;
            }
            else if (index < 30)
            {
                row = 0;
                column = index - 20;
            }
            else
            {
                row = index - 30;
                column = 10;
            }

            return new Tuple<int, int>(row, column);
        }

        private static void SetGridPosition(Grid grid, int i)
        {
            Tuple<int, int> coords = GetCoords(i);

            Grid.SetRow(grid, coords.Item1);
            Grid.SetColumn(grid, coords.Item2);

            grid.Background = Brushes.White;
            grid.RowDefinitions.Add(new RowDefinition(1.0, GridUnitType.Star));
            grid.RowDefinitions.Add(new RowDefinition(2.0, GridUnitType.Star));
            grid.RowDefinitions.Add(new RowDefinition(2.0, GridUnitType.Star));
            grid.RowDefinitions.Add(new RowDefinition(1.0, GridUnitType.Star));
        }

        private static void CreateTopPanel(Grid grid, int i)
        {
            Panel panel = new Panel();
            panel.Bind(BackgroundProperty, new Binding(string.Format("PropertiesVM[{0}].Color", i)));
            Grid.SetRow(panel, 0);
            grid.Children.Add(panel);
        }

        private static void CreateNameTB(Grid grid, int i)
        {
            TextBlock nameTB = new TextBlock();
            Grid.SetRow(nameTB, 1);
            nameTB.FontSize = 12;
            nameTB.TextWrapping = TextWrapping.Wrap;
            nameTB.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
            nameTB.Bind(TextBlock.TextProperty, new Binding(string.Format("PropertiesVM[{0}].Name", i)));
            grid.Children.Add(nameTB);
        }

        private static void CreatePriceTB(Grid grid, int i)
        {
            TextBlock priceTB = new TextBlock();
            Grid.SetRow(priceTB, 3);
            priceTB.FontSize = 10;
            priceTB.TextWrapping = TextWrapping.Wrap;
            priceTB.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
            priceTB.Bind(TextBlock.TextProperty, new Binding(string.Format("PropertiesVM[{0}].Cost", i)));
            grid.Children.Add(priceTB);
        }

        private static void CreateBottomPanel(Grid grid, int i)
        {
            Panel modelPanel = new Panel();
            modelPanel.Height = modelPanel.Width = 10;
            modelPanel.Bind(BackgroundProperty, new Binding(string.Format("PropertiesVM[{0}].ColorPlayer", i)));
            Grid.SetRow(modelPanel, 2);
            grid.Children.Add(modelPanel);
        }
        #endregion
    }
}
