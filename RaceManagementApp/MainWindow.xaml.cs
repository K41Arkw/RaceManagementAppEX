// ========== MainWindow.xaml.cs 【最終修正版 v3】 ここから ==========
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace RaceManagerApp
{
    public partial class MainWindow : Window
    {
        #region Properties
        public ObservableCollection<Driver> AllDrivers { get; set; } = new();
        public ObservableCollection<PointSetting> PointSystem1 { get; set; } = new();
        public ObservableCollection<PointSetting> PointSystem2 { get; set; } = new();

        public ObservableCollection<Driver> GroupA { get; set; } = new();
        public ObservableCollection<Driver> GroupB { get; set; } = new();
        public ObservableCollection<Driver> GroupC { get; set; } = new();
        private List<ObservableCollection<Driver>> AllGroups { get; set; } = new();
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            AllGroups = new List<ObservableCollection<Driver>> { GroupA, GroupB, GroupC };
        }

        #region UI Event Handlers

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(ParticipantCountTextBox.Text, out int count) && count > 0 && count % 6 == 0)
            {
                SetupGrid.Visibility = Visibility.Collapsed;
                MainPanel.Visibility = Visibility.Visible;
                InitializeDrivers(count);
                InitializePointSystem(6);

                SetupRaceDataGrid((DataGrid)GroupAGrid_R1.Content, 1);
                SetupRaceDataGrid((DataGrid)GroupBGrid_R1.Content, 1);
                SetupRaceDataGrid((DataGrid)GroupCGrid_R1.Content, 1);
                SetupRaceDataGrid((DataGrid)GroupAGrid_R2.Content, 2);
                SetupRaceDataGrid((DataGrid)GroupBGrid_R2.Content, 2);
                SetupRaceDataGrid((DataGrid)GroupCGrid_R2.Content, 2);
            }
            else
            {
                MessageBox.Show("参加人数は6の倍数の正の整数で入力してください。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddDriverButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BibTextBox.Text) || string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("ゼッケンと名前を入力してください。");
                return;
            }
            if (AllDrivers.Any(d => d.Bib == BibTextBox.Text))
            {
                MessageBox.Show("そのゼッケンは既に使用されています。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            AllDrivers.Add(new Driver { Bib = BibTextBox.Text, Name = NameTextBox.Text });
            BibTextBox.Clear();
            NameTextBox.Clear();
        }

        private void AbsentButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Driver driver)
            {
                driver.IsAbsent = true;
            }
        }

        private void ExportTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog
            {
                Filter = "CSVファイル (*.csv)|*.csv",
                FileName = "エントリーリスト_テンプレート.csv"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(sfd.FileName, "ゼッケン,名前\n", Encoding.UTF8);
                    MessageBox.Show($"テンプレートファイルを出力しました。\n{sfd.FileName}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"ファイルの出力中にエラーが発生しました。\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ImportListButton_Click(object sender, RoutedEventArgs e)
        {
            if (AllDrivers.Any())
            {
                var result = MessageBox.Show("現在のエントリーリストはクリアされます。よろしいですか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }

            var ofd = new OpenFileDialog
            {
                Filter = "CSVファイル (*.csv)|*.csv"
            };

            if (ofd.ShowDialog() == true)
            {
                try
                {
                    var lines = File.ReadAllLines(ofd.FileName, Encoding.UTF8);
                    if (lines.Length <= 1)
                    {
                        MessageBox.Show("ファイルにデータがありません（ヘッダー行を除く）。");
                        return;
                    }

                    AllDrivers.Clear();
                    // ヘッダー行をスキップして読み込み
                    foreach (var line in lines.Skip(1))
                    {
                        var parts = line.Split(',');
                        if (parts.Length >= 2)
                        {
                            AllDrivers.Add(new Driver { Bib = parts[0].Trim(), Name = parts[1].Trim() });
                        }
                    }
                    MessageBox.Show("エントリーリストを読み込みました。");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"ファイルの読み込み中にエラーが発生しました。\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void FinalizeQualifyingButton_Click(object sender, RoutedEventArgs e)
        {
            var activeDrivers = AllDrivers.Where(d => !d.IsAbsent).ToList();
            if (activeDrivers.Any(d => d.QualifyingTime <= 0))
            {
                MessageBox.Show("タイムが0以下の選手がいます。正しいタイムを入力してください。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var sortedDrivers = activeDrivers.OrderBy(d => d.QualifyingTime).ToList();
            for (int i = 0; i < sortedDrivers.Count; i++)
            {
                sortedDrivers[i].QualifyingPosition = i + 1;
            }

            CalculateAndAssignGroups(sortedDrivers);

            MessageBox.Show("グループ分けが完了しました。「レース1」タブに進んでください。");
            MainTabControl.SelectedIndex = 2;
        }

        private void LapsDownButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Driver driver)
            {
                int raceNumber = GetRaceNumberFromSender(sender);
                if (raceNumber == 1) driver.Race1LapsDown++;
                else driver.Race2LapsDown++;
                UpdateRacePositions(raceNumber);
            }
        }

        private void YellowButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Driver driver)
            {
                driver.YellowCardCount++;
                if (driver.YellowCardCount >= 2)
                {
                    if (double.TryParse(YellowPenaltyTextBox.Text, out double penaltySec))
                    {
                        int raceNumber = GetRaceNumberFromSender(sender);
                        if (raceNumber == 1) driver.Race1Penalty += penaltySec;
                        else driver.Race2Penalty += penaltySec;
                        driver.YellowCardCount = 0;
                        UpdateRacePositions(raceNumber);
                    }
                    else
                    {
                        MessageBox.Show("ペナルティ秒数に正しい数値を設定してください。");
                        driver.YellowCardCount--;
                    }
                }
            }
        }

        private void RedButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Driver driver)
            {
                if (double.TryParse(RedPenaltyTextBox.Text, out double penaltySec))
                {
                    int raceNumber = GetRaceNumberFromSender(sender);
                    if (raceNumber == 1) driver.Race1Penalty += penaltySec;
                    else driver.Race2Penalty += penaltySec;
                    UpdateRacePositions(raceNumber);
                }
                else
                {
                    MessageBox.Show("ペナルティ秒数に正しい数値を設定してください。");
                }
            }
        }

        private void RaceTime_LostFocus(object sender, RoutedEventArgs e)
        {
            int raceNumber = (MainTabControl.SelectedIndex == 2) ? 1 : 2;
            UpdateRacePositions(raceNumber);
        }

        private void FinalizeRace1Button_Click(object sender, RoutedEventArgs e)
        {
            UpdateRacePositions(1);
            foreach (var driver in AllDrivers.Where(d => !d.IsAbsent))
            {
                var pointEntry = PointSystem1.FirstOrDefault(p => p.Position == driver.Race1Position);
                driver.Race1Points = pointEntry?.Points ?? 0;
            }
            MessageBox.Show("レース1の結果を保存しました。「レース2」タブに進んでください。");
            MainTabControl.SelectedIndex = 3;
        }

        private void FinalizeRace2Button_Click(object sender, RoutedEventArgs e)
        {
            UpdateRacePositions(2);
            foreach (var driver in AllDrivers.Where(d => !d.IsAbsent))
            {
                var pointEntry = PointSystem2.FirstOrDefault(p => p.Position == driver.Race2Position);
                driver.Race2Points = pointEntry?.Points ?? 0;
                driver.TotalPoints = driver.Race1Points + driver.Race2Points;
            }

            CalculateFinalStandings();
            MessageBox.Show("最終結果が計算されました。「最終結果」タブで確認してください。");
            MainTabControl.SelectedIndex = 4;
        }

        private void ExportToCsvButton_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog
            {
                Filter = "CSVファイル (*.csv)|*.csv",
                FileName = $"レース結果_{DateTime.Now:yyyyMMdd}.csv",
                DefaultExt = ".csv"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("最終順位,合計ポイント,ゼッケン,名前,グループ,予選順位,予選タイム,R1順位,R1周回遅れ,R1走行タイム,R1ペナルティ,R1合算タイム,R1ポイント,R2順位,R2周回遅れ,R2走行タイム,R2ペナルティ,R2合算タイム,R2ポイント");

                    var sortedDrivers = AllDrivers.Where(d => !d.IsAbsent).OrderBy(d => d.Group).ThenBy(d => d.FinalPositionInGroup);
                    foreach (var d in sortedDrivers)
                    {
                        string race1TimeStr = d.Race1Time.ToString(@"m\:ss\.fff");
                        string race2TimeStr = d.Race2Time.ToString(@"m\:ss\.fff");
                        sb.AppendLine($"{d.FinalPositionInGroup},{d.TotalPoints},\"{d.Bib}\",\"{d.Name}\",\"{d.Group}\",{d.QualifyingPosition},{d.QualifyingTime},{d.Race1Position},{d.Race1LapsDown},\"{race1TimeStr}\",{d.Race1Penalty},{d.Race1FinalTime},{d.Race1Points},{d.Race2Position},{d.Race2LapsDown},\"{race2TimeStr}\",{d.Race2Penalty},{d.Race2FinalTime},{d.Race2Points}");
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show($"ファイルが正常に保存されました。\n{sfd.FileName}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"ファイルの保存中にエラーが発生しました。\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveScreenshotButton_Click(object sender, RoutedEventArgs e)
        {
            // 現在の表示タブに応じて、データをソートする
            switch (MainTabControl.SelectedIndex)
            {
                case 2: // レース1
                    UpdateRacePositions(1);
                    break;
                case 3: // レース2
                    UpdateRacePositions(2);
                    break;
                case 4: // 最終結果
                    CalculateFinalStandings();
                    break;
            }

            // UIが更新されるのを待つ
            this.Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle);

            var rtb = new RenderTargetBitmap((int)this.ActualWidth, (int)this.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(this);
            var pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

            var sfd = new SaveFileDialog
            {
                Filter = "PNG画像 (*.png)|*.png",
                FileName = $"レース画面_{DateTime.Now:yyyyMMdd_HHmmss}.png",
                DefaultExt = ".png"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    using (var fs = File.OpenWrite(sfd.FileName))
                    {
                        pngEncoder.Save(fs);
                    }
                    MessageBox.Show($"スクリーンショットを保存しました: {sfd.FileName}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"スクリーンショットの保存中にエラーが発生しました。\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        #endregion

        #region Logic
        private void InitializeDrivers(int count)
        {
            AllDrivers.Clear();
            for (int i = 1; i <= count; i++)
            {
                AllDrivers.Add(new Driver { Bib = i.ToString(), Name = $"選手 {i:D2}" });
            }
        }

        private void InitializePointSystem(int maxPosition)
        {
            PointSystem1.Clear();
            PointSystem2.Clear();
            for (int i = 1; i <= maxPosition; i++)
            {
                int points = Math.Max(0, (10 - (i - 1)));
                PointSystem1.Add(new PointSetting { Position = i, Points = points });
                PointSystem2.Add(new PointSetting { Position = i, Points = points });
            }
        }

        private void CalculateAndAssignGroups(List<Driver> activeSortedDrivers)
        {
            foreach (var group in AllGroups) group.Clear();

            int totalSlots;
            if (!int.TryParse(ParticipantCountTextBox.Text, out totalSlots)) return;

            int groupCount = totalSlots / 6;
            int activeCount = activeSortedDrivers.Count;

            if (groupCount == 0) return;

            int baseSize = activeCount / groupCount;
            int remainder = activeCount % groupCount;

            int currentIndex = 0;
            for (int i = 0; i < groupCount; i++)
            {
                int currentGroupSize = baseSize + (i < remainder ? 1 : 0);
                for (int j = 0; j < currentGroupSize; j++)
                {
                    if (currentIndex < activeSortedDrivers.Count)
                    {
                        var driver = activeSortedDrivers[currentIndex];
                        driver.Group = ((char)('A' + i)).ToString();
                        AllGroups[i].Add(driver);
                        currentIndex++;
                    }
                }
            }
        }

        private void UpdateRacePositions(int raceNumber)
        {
            foreach (var group in AllGroups)
            {
                UpdateGroupRacePositions(group, raceNumber);
            }
        }

        private void UpdateGroupRacePositions(ObservableCollection<Driver> group, int raceNumber)
        {
            if (!group.Any()) return;

            List<Driver> sortedGroup;
            if (raceNumber == 1)
            {
                sortedGroup = group.OrderBy(d => d.Race1LapsDown).ThenBy(d => d.Race1FinalTime).ToList();
                for (int i = 0; i < sortedGroup.Count; i++)
                {
                    sortedGroup[i].Race1Position = i + 1;
                }
            }
            else
            {
                sortedGroup = group.OrderBy(d => d.Race2LapsDown).ThenBy(d => d.Race2FinalTime).ToList();
                for (int i = 0; i < sortedGroup.Count; i++)
                {
                    sortedGroup[i].Race2Position = i + 1;
                }
            }

            foreach (var driver in group)
            {
                driver.OnPropertyChanged("");
            }
        }

        private void CalculateFinalStandings()
        {
            foreach (var group in AllGroups)
            {
                CalculateGroupFinalStandings(group);
            }
        }

        private void CalculateGroupFinalStandings(ObservableCollection<Driver> group)
        {
            if (!group.Any()) return;

            List<Driver> sortedGroup;
            bool isPointsAscending = PointsRuleCheckBox.IsChecked == true;

            if (isPointsAscending)
            {
                sortedGroup = group.OrderBy(d => d.TotalPoints).ThenBy(d => d.QualifyingTime).ToList();
            }
            else
            {
                sortedGroup = group.OrderByDescending(d => d.TotalPoints).ThenBy(d => d.QualifyingTime).ToList();
            }

            for (int i = 0; i < sortedGroup.Count; i++)
            {
                sortedGroup[i].FinalPositionInGroup = i + 1;
            }

            var temp = sortedGroup.ToList();
            group.Clear();
            temp.ForEach(d => group.Add(d));
        }

        private void SetupRaceDataGrid(DataGrid dataGrid, int raceNumber)
        {
            dataGrid.Columns.Clear();

            var gridColumn = new DataGridTextColumn { Header = "グリッド", IsReadOnly = true, Width = new DataGridLength(50) };
            if (raceNumber == 1) gridColumn.Binding = new Binding("QualifyingPosition");
            else gridColumn.Binding = new Binding("Race1Position");
            dataGrid.Columns.Add(gridColumn);

            dataGrid.Columns.Add(new DataGridTextColumn { Header = "ゼッケン", Binding = new Binding("Bib"), IsReadOnly = true, Width = new DataGridLength(60) });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "名前", Binding = new Binding("Name"), IsReadOnly = true, Width = new DataGridLength(1, DataGridLengthUnitType.Star) });

            var lapsDownColumn = new DataGridTemplateColumn { Header = "周回遅れ", Width = new DataGridLength(80) };
            var lapsDownFactory = new FrameworkElementFactory(typeof(StackPanel));
            lapsDownFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            var lapsDownTextBlock = new FrameworkElementFactory(typeof(TextBlock));
            lapsDownTextBlock.SetBinding(TextBlock.TextProperty, new Binding($"Race{raceNumber}LapsDown"));
            lapsDownTextBlock.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
            var lapsDownButton = new FrameworkElementFactory(typeof(Button));
            lapsDownButton.SetValue(ContentProperty, "+1");
            lapsDownButton.SetValue(MarginProperty, new Thickness(5, 0, 0, 0));
            lapsDownButton.SetValue(TagProperty, raceNumber);
            lapsDownButton.AddHandler(Button.ClickEvent, new RoutedEventHandler(LapsDownButton_Click));
            lapsDownFactory.AppendChild(lapsDownTextBlock);
            lapsDownFactory.AppendChild(lapsDownButton);
            lapsDownColumn.CellTemplate = new DataTemplate { VisualTree = lapsDownFactory };
            dataGrid.Columns.Add(lapsDownColumn);

            var lostFocusEvent = new EventSetter(LostFocusEvent, new RoutedEventHandler(RaceTime_LostFocus));
            var cellStyle = new Style(typeof(DataGridCell));
            cellStyle.Setters.Add(lostFocusEvent);

            var raceTimeMinBinding = new Binding($"Race{raceNumber}Minutes") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
            var raceTimeMinColumn = new DataGridTextColumn { Header = "分", Binding = raceTimeMinBinding, Width = new DataGridLength(40), CellStyle = cellStyle };
            dataGrid.Columns.Add(raceTimeMinColumn);

            var raceTimeSecBinding = new Binding($"Race{raceNumber}Seconds") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
            var raceTimeSecColumn = new DataGridTextColumn { Header = "秒", Binding = raceTimeSecBinding, Width = new DataGridLength(60), CellStyle = cellStyle };
            dataGrid.Columns.Add(raceTimeSecColumn);

            dataGrid.Columns.Add(new DataGridTextColumn { Header = "ペナ(s)", Binding = new Binding($"Race{raceNumber}Penalty"), IsReadOnly = true, Width = new DataGridLength(60) });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "合算", Binding = new Binding($"Race{raceNumber}FinalTimeString"), IsReadOnly = true, Width = new DataGridLength(90) });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "順位", Binding = new Binding($"Race{raceNumber}Position"), IsReadOnly = true, Width = new DataGridLength(50) });

            var penaltyColumn = new DataGridTemplateColumn { Header = "ペナルティ", Width = new DataGridLength(100) };
            var penaltyFactory = new FrameworkElementFactory(typeof(StackPanel));
            penaltyFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            var yellowButton = new FrameworkElementFactory(typeof(Button));
            yellowButton.SetValue(ContentProperty, "黄");
            yellowButton.SetValue(BackgroundProperty, Brushes.Yellow);
            yellowButton.SetValue(TagProperty, raceNumber);
            yellowButton.AddHandler(Button.ClickEvent, new RoutedEventHandler(YellowButton_Click));
            var redButton = new FrameworkElementFactory(typeof(Button));
            redButton.SetValue(ContentProperty, "赤");
            redButton.SetValue(MarginProperty, new Thickness(5, 0, 0, 0));
            redButton.SetValue(BackgroundProperty, Brushes.Red);
            redButton.SetValue(ForegroundProperty, Brushes.White);
            redButton.SetValue(TagProperty, raceNumber);
            redButton.AddHandler(Button.ClickEvent, new RoutedEventHandler(RedButton_Click));
            var yellowCount = new FrameworkElementFactory(typeof(TextBlock));
            yellowCount.SetBinding(TextBlock.TextProperty, new Binding("YellowCardCount"));
            yellowCount.SetValue(MarginProperty, new Thickness(5, 0, 0, 0));
            yellowCount.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);

            penaltyFactory.AppendChild(yellowButton);
            penaltyFactory.AppendChild(redButton);
            penaltyFactory.AppendChild(yellowCount);
            penaltyColumn.CellTemplate = new DataTemplate { VisualTree = penaltyFactory };
            dataGrid.Columns.Add(penaltyColumn);
        }

        private int GetRaceNumberFromSender(object sender)
        {
            if ((sender as FrameworkElement)?.Tag is int raceNum)
            {
                return raceNum;
            }
            return (MainTabControl.SelectedIndex == 2) ? 1 : 2;
        }

        #endregion
    }
}
// ========== MainWindow.xaml.cs 【最終修正版 v3】 ここまで ==========