namespace DepthCharts;

public interface IDepthChartService
{
    void AddPlayer(Player player, string position, int? depth = null);

    void RemovePlayer(Player player, string position);

    IReadOnlyList<DepthChartRow> GetFullDepthChart();
}
