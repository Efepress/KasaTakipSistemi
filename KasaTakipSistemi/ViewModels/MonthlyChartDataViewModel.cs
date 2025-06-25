
namespace KasaTakipSistemi.ViewModels
{
    public class MonthlyChartDataViewModel
    {
        public List<Tuple<int, decimal>> Gelirler { get; set; } = new List<Tuple<int, decimal>>();
        public List<Tuple<int, decimal>> Giderler { get; set; } = new List<Tuple<int, decimal>>();
    }
}