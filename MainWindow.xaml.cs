using FastWpfGrid;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace WpfApp1
{
    public class GridModel1 : FastGridModelBase
    {
        private Dictionary<Tuple<int, int>, string> _editedCells = new Dictionary<Tuple<int, int>, string>();
        private static string[] _columnBasicNames = new[] { "", "Value:", "Long column value:" };
        public List<string> Columns = new List<string>();
        public List<List<string>> Rows = new List<List<string>>();

        public override Color? DecorationColor
        {
            get
            {
                return (Color)ColorConverter.ConvertFromString("#FF0000");
            }
        }

        public override CellDecoration Decoration
        {
            get { return CellDecoration.None; }
        }

        public override Color? BackgroundColor(int row, int col)
        {
            if (row == -1 || col == -1) return null;
            var key = Tuple.Create(row, col);
            if (_editedCells.ContainsKey(key))
            {
                // Edited
                return (Color)ColorConverter.ConvertFromString("#00FFFF");
            }
            return null;
        }

        public override int ColumnCount
        {
            get { return Columns.Count; }
        }

        public override int RowCount
        {
            get { return Rows.Count; }
        }

        public override string GetColumnHeaderText(int column)
        {
            return Columns[column];
        }

        public override string GetCellText(int row, int column)
        {
            var key = Tuple.Create(row, column);
            if (_editedCells.ContainsKey(key)) return _editedCells[key];


            return Rows[row][column];// String.Format("{0}{1},{2}", _columnBasicNames[column % _columnBasicNames.Length], row + 1, column + 1);
        }

        public override void SetCellText(int row, int column, string value)
        {
            var key = Tuple.Create(row, column);
            _editedCells[key] = value;
        }

        public override IFastGridCell GetGridHeader(IFastGridView view)
        {
            var impl = new FastGridCellImpl();
            //var btn = impl.AddImageBlock(view.IsTransposed ? "/Images/flip_horizontal_small.png" : "/Images/flip_vertical_small.png");
            //btn.CommandParameter = FastWpfGrid.FastGridControl.ToggleTransposedCommand;
            //btn.ToolTip = "Swap rows and columns";
            //impl.AddImageBlock("/Images/foreign_keysmall.png").CommandParameter = "FK";
            //impl.AddImageBlock("/Images/primary_keysmall.png").CommandParameter = "PK";
            return impl;
        }

        public override void HandleCommand(IFastGridView view, FastGridCellAddress address, object commandParameter, ref bool handled)
        {
            base.HandleCommand(view, address, commandParameter, ref handled);
            if (commandParameter is string) MessageBox.Show(commandParameter.ToString());
        }

        public override int? SelectedRowCountLimit
        {
            get { return 100; }
        }

        public override void HandleSelectionCommand(IFastGridView view, string command)
        {
            MessageBox.Show(command);
        }
    }

    public partial class MainWindow : Window
    {
        private readonly GridModel1 _model1;

        public MainWindow()
        {
            InitializeComponent();
            _model1 = new GridModel1();
            grid1.HeaderWidth = 10;
            grid1.AllowSelectAll = false;
            grid1.Model = _model1;
            for (int i = 0; i < 100; i++)
            {
                _model1.Columns.Add(string.Format("Column {0}", i));
            }
            AddRows(10000);
        }

        private void grid1_SelectedCellsChanged(object sender, FastWpfGrid.SelectionChangedEventArgs e)
        {
            var view = (IFastGridView)grid1;
            if (view.GetSelectedModelCells().Count > 1)
            {
                view.ShowSelectionMenu(new string[] { "CMD1", "CMD2" });
            }
            else
            {
                view.ShowSelectionMenu(null);
            }
        }

        private void Add100Rows(object sender, RoutedEventArgs e)
        {
            AddRows(100);
        }

        private void Add1000Rows(object sender, RoutedEventArgs e)
        {
            AddRows(1000);
        }

        private void Add10000Rows(object sender, RoutedEventArgs e)
        {
            AddRows(10000);
        }

        private bool isAdding = false;
        private void AddRows(int count)
        {
            if (isAdding) return;
            isAdding = true;
            Task.Run(() =>
            {
                var rows = new List<List<string>>();
                for (int i = 0; i < count; i++)
                {
                    var row = new List<string>();
                    for (int j = 0; j < _model1.Columns.Count; j++)
                    {
                        row.Add(string.Format("Row value: {0}-{1}", j, i));
                    }
                    rows.Add(row);
                }
                _model1.Rows.AddRange(rows);
                Dispatcher.Invoke(() =>
                {
                    grid1.NotifyAddedRows(); isAdding = false;
                });
            });
        }
    }
}
