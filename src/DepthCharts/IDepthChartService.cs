namespace DepthCharts;

public interface IDepthChartService
{
    void AddPlayer(Player player, string position, int depth);

    IReadOnlyList<DepthChartRow> GetFullDepthChart();
}
