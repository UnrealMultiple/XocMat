using SixLabors.Fonts;

namespace Lagrange.XocMat.Utility.Images;

public class TableBuilder
{
    private string _title = string.Empty;
    private List<List<string>> _tableData;
    private Font _font = ImageUtils.Instance.FontFamily.CreateFont(24);
    private bool _titleBottom;
    private string _backgroundImagePath = string.Empty;
    private TableGenerator _generator;

    public TableBuilder()
    {
        _tableData = [];
        _generator = new TableGenerator();
    }

    public TableBuilder SetTitle(string title)
    {
        _title = title;
        return this;
    }

    public TableBuilder AddRow(params string[] rowData)
    {
        _tableData.Add(new List<string>(rowData));
        return this;
    }

    public TableBuilder SetFont(Font font)
    {
        _font = font;
        return this;
    }

    public TableBuilder SetTitleBottom(bool titleBottom)
    {
        _titleBottom = titleBottom;
        return this;
    }

    public TableBuilder SetBackgroundImagePath(string backgroundImagePath)
    {
        _backgroundImagePath = backgroundImagePath;
        return this;
    }

    public string GetTitle() => _title;
    public string[,] GetTableData() => ConvertTo2DArray(_tableData);
    public Font GetFont() => _font;
    public bool IsTitleBottom() => _titleBottom;
    public string GetBackgroundImagePath() => _backgroundImagePath;

    public async Task<byte[]> BuildAsync() => await _generator.GenerateTable(this);

    private string[,] ConvertTo2DArray(List<List<string>> data)
    {
        int rows = data.Count;
        int cols = data.Max(row => row.Count);
        string[,] array = new string[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                array[i, j] = j < data[i].Count ? data[i][j] : string.Empty;
            }
        }
        return array;
    }
}
