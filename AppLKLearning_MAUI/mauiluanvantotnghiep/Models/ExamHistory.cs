using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.Models
{
    public class ExamHistory
    {
        public int HistoryId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }
        public int ExamSetId { get; set; }
        public DateTime TakenAt { get; set; }
        public decimal? TotalScore { get; set; }
        public bool? IsPassed { get; set; }
        public int? DurationSec { get; set; }
        
        // Computed properties for display
        public string StatusText => IsPassed == true ? "Đậu" : IsPassed == false ? "Rớt" : "Chưa hoàn thành";
        public string StatusIcon => IsPassed == true ? "tick.png" : IsPassed == false ? "failed.png" : "https://img.icons8.com/pulsar-gradient/50/error.png";
        public string StatusColor => IsPassed == true ? "#4CAF50" : IsPassed == false ? "#F44336" : "#FF9800";
        public string ScoreText => TotalScore?.ToString("0") ?? "--";
        public string DurationText => DurationSec.HasValue ? $"{DurationSec.Value / 60}:{DurationSec.Value % 60:D2}" : "--:--";
        public string DateText => TakenAt.ToString("dd/MM/yyyy HH:mm");
        public string DateShort => TakenAt.ToString("dd/MM");
    }
}
