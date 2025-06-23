// ========== Driver.cs 【最終修正版 v2】 ここから ==========
using System;
using System.ComponentModel;

namespace RaceManagerApp
{
    public class Driver : INotifyPropertyChanged
    {
        // 基本情報
        public string Bib { get; set; } = "";
        public string Name { get; set; } = "";

        private bool _isAbsent;
        public bool IsAbsent
        {
            get => _isAbsent;
            set { _isAbsent = value; OnPropertyChanged(nameof(IsAbsent)); }
        }

        // 予選
        public double QualifyingTime { get; set; }
        public int QualifyingPosition { get; set; }

        // グループ
        public string Group { get; set; } = "";

        // ペナルティ関連
        private int _yellowCardCount;
        public int YellowCardCount
        {
            get => _yellowCardCount;
            set { _yellowCardCount = value; OnPropertyChanged(nameof(YellowCardCount)); }
        }

        // --- レース1 ---
        private int _race1Minutes;
        public int Race1Minutes
        {
            get => _race1Minutes;
            set { _race1Minutes = value; OnPropertyChanged(nameof(Race1Minutes)); OnPropertyChanged(nameof(Race1FinalTime)); OnPropertyChanged(nameof(Race1FinalTimeString)); }
        }
        private double _race1Seconds;
        public double Race1Seconds
        {
            get => _race1Seconds;
            set { _race1Seconds = value; OnPropertyChanged(nameof(Race1Seconds)); OnPropertyChanged(nameof(Race1FinalTime)); OnPropertyChanged(nameof(Race1FinalTimeString)); }
        }
        public TimeSpan Race1Time => TimeSpan.FromSeconds(Race1Minutes * 60 + Race1Seconds);

        private int _race1LapsDown;
        public int Race1LapsDown
        {
            get => _race1LapsDown;
            set { _race1LapsDown = value; OnPropertyChanged(nameof(Race1LapsDown)); }
        }
        private double _race1Penalty;
        public double Race1Penalty
        {
            get => _race1Penalty;
            set { _race1Penalty = value; OnPropertyChanged(nameof(Race1Penalty)); OnPropertyChanged(nameof(Race1FinalTime)); OnPropertyChanged(nameof(Race1FinalTimeString)); }
        }
        public double Race1FinalTime => Race1Time.TotalSeconds + Race1Penalty;
        public string Race1FinalTimeString
        {
            get
            {
                var ts = TimeSpan.FromSeconds(Race1FinalTime);
                return $"{ts.Minutes}m{ts.Seconds:00}.{ts.Milliseconds:000}s";
            }
        }
        public int Race1Position { get; set; }
        public int Race1Points { get; set; }

        // --- レース2 ---
        private int _race2Minutes;
        public int Race2Minutes
        {
            get => _race2Minutes;
            set { _race2Minutes = value; OnPropertyChanged(nameof(Race2Minutes)); OnPropertyChanged(nameof(Race2FinalTime)); OnPropertyChanged(nameof(Race2FinalTimeString)); }
        }
        private double _race2Seconds;
        public double Race2Seconds
        {
            get => _race2Seconds;
            set { _race2Seconds = value; OnPropertyChanged(nameof(Race2Seconds)); OnPropertyChanged(nameof(Race2FinalTime)); OnPropertyChanged(nameof(Race2FinalTimeString)); }
        }
        public TimeSpan Race2Time => TimeSpan.FromSeconds(Race2Minutes * 60 + Race2Seconds);

        private int _race2LapsDown;
        public int Race2LapsDown
        {
            get => _race2LapsDown;
            set { _race2LapsDown = value; OnPropertyChanged(nameof(Race2LapsDown)); }
        }
        private double _race2Penalty;
        public double Race2Penalty
        {
            get => _race2Penalty;
            set { _race2Penalty = value; OnPropertyChanged(nameof(Race2Penalty)); OnPropertyChanged(nameof(Race2FinalTime)); OnPropertyChanged(nameof(Race2FinalTimeString)); }
        }
        public double Race2FinalTime => Race2Time.TotalSeconds + Race2Penalty;
        public string Race2FinalTimeString
        {
            get
            {
                var ts = TimeSpan.FromSeconds(Race2FinalTime);
                return $"{ts.Minutes}m{ts.Seconds:00}.{ts.Milliseconds:000}s";
            }
        }
        public int Race2Position { get; set; }
        public int Race2Points { get; set; }

        // --- 総合 ---
        public int TotalPoints { get; set; }
        public int FinalPositionInGroup { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
// ========== Driver.cs 【最終修正版 v2】 ここまで ==========