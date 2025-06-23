// ========== PointSetting.cs ここから ==========
using System.ComponentModel;

namespace RaceManagerApp
{
    public class PointSetting : INotifyPropertyChanged
    {
        private int _position;
        public int Position
        {
            get => _position;
            set { _position = value; OnPropertyChanged(nameof(Position)); }
        }

        private int _points;
        public int Points
        {
            get => _points;
            set { _points = value; OnPropertyChanged(nameof(Points)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
// ========== PointSetting.cs ここまで ==========