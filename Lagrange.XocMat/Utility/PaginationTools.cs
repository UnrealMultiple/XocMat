using System.Collections;
using System.Text;
using Lagrange.Core.Event.EventArg;
using Lagrange.Core.Message;
using Lagrange.XocMat.Extensions;

namespace Lagrange.XocMat.Utility;

public static class PaginationTools
{
    public delegate Tuple<string> LineFormatterDelegate(object lineData, int lineIndex, int pageNumber);

    #region [Nested: Settings Class]
    public class Settings
    {
        public bool IncludeHeader { get; set; }

        private string headerFormat;
        public string HeaderFormat
        {
            get { return headerFormat; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                headerFormat = value;
            }
        }

        public bool IncludeFooter { get; set; }

        private string footerFormat;
        public string FooterFormat
        {
            get { return footerFormat; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                footerFormat = value;
            }
        }

        public string NothingToDisplayString { get; set; }
        public LineFormatterDelegate? LineFormatter { get; set; }

        private int maxLinesPerPage;

        public int MaxLinesPerPage
        {
            get { return maxLinesPerPage; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("该值必须大于0!");

                maxLinesPerPage = value;
            }
        }

        private int pageLimit;

        public int PageLimit
        {
            get { return pageLimit; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("该值必须大于0!");

                pageLimit = value;
            }
        }


        public Settings()
        {
            IncludeHeader = true;
            headerFormat = "第 {{0}} 页，共 {{1}} 页";
            IncludeFooter = true;
            footerFormat = "输入 /<指令> {{0}} 查看更多";
            NothingToDisplayString = "";
            LineFormatter = null;
            maxLinesPerPage = 4;
            pageLimit = 0;
        }
    }
    #endregion

    public static void SendPage(
      FriendMessageEvent args, int pageNumber, IEnumerable dataToPaginate, int dataToPaginateCount, Settings? settings = null)
    {
        settings ??= new Settings();

        if (dataToPaginateCount == 0)
        {
            if (settings.NothingToDisplayString != null)
            {
                _ = args.Reply(settings.NothingToDisplayString);
            }
            return;
        }

        int pageCount = ((dataToPaginateCount - 1) / settings.MaxLinesPerPage) + 1;
        if (settings.PageLimit > 0 && pageCount > settings.PageLimit)
            pageCount = settings.PageLimit;
        if (pageNumber > pageCount)
            pageNumber = pageCount;
        MessageBuilder builder = MessageBuilder.Friend(Convert.ToUInt32(args.Chain.FriendUin));
        if (settings.IncludeHeader)
        {
            _ = builder.Text(string.Format(settings.HeaderFormat, pageNumber, pageCount));
        }

        int listOffset = (pageNumber - 1) * settings.MaxLinesPerPage;
        int offsetCounter = 0;
        int lineCounter = 0;
        foreach (object? lineData in dataToPaginate)
        {
            if (lineData == null)
                continue;
            if (offsetCounter++ < listOffset)
                continue;
            if (lineCounter++ == settings.MaxLinesPerPage)
                break;

            string lineMessage;
            if (lineData is Tuple<string>)
            {
                Tuple<string> lineFormat = (Tuple<string>)lineData;
                lineMessage = lineFormat.Item1;
            }
            else if (settings.LineFormatter != null)
            {
                try
                {
                    Tuple<string> lineFormat = settings.LineFormatter(lineData, offsetCounter, pageNumber);
                    if (lineFormat == null)
                        continue;

                    lineMessage = lineFormat.Item1;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("LineFormatter 引用的方法引发了异常。有关详细信息，请参阅内部异常。", ex);
                }
            }
            else
            {
                lineMessage = lineData?.ToString() ?? "";
            }

            if (lineMessage != null)
            {
                _ = builder.Text("\n" + lineMessage);
            }
        }

        if (lineCounter == 0)
        {
            if (settings.NothingToDisplayString != null)
            {
                _ = args.Reply(settings.NothingToDisplayString);
            }
        }
        else if (settings.IncludeFooter && pageNumber + 1 <= pageCount)
        {
            _ = builder.Text("\n" + string.Format(settings.FooterFormat, pageNumber + 1, pageNumber, pageCount));

        }
        _ = args.Reply(builder);
    }

    public static void SendPage(
      GroupMessageEvent args, int pageNumber, IEnumerable dataToPaginate, int dataToPaginateCount, Settings? settings = null)
    {
        settings ??= new Settings();

        if (dataToPaginateCount == 0)
        {
            if (settings.NothingToDisplayString != null)
            {
                _ = args.Reply(settings.NothingToDisplayString);
            }
            return;
        }

        int pageCount = ((dataToPaginateCount - 1) / settings.MaxLinesPerPage) + 1;
        if (settings.PageLimit > 0 && pageCount > settings.PageLimit)
            pageCount = settings.PageLimit;
        if (pageNumber > pageCount)
            pageNumber = pageCount;
        MessageBuilder builder = MessageBuilder.Group(Convert.ToUInt32(args.Chain.GroupUin));
        if (settings.IncludeHeader)
        {
            _ = builder.Text(string.Format(settings.HeaderFormat, pageNumber, pageCount));
        }

        int listOffset = (pageNumber - 1) * settings.MaxLinesPerPage;
        int offsetCounter = 0;
        int lineCounter = 0;
        foreach (object? lineData in dataToPaginate)
        {
            if (lineData == null)
                continue;
            if (offsetCounter++ < listOffset)
                continue;
            if (lineCounter++ == settings.MaxLinesPerPage)
                break;

            string lineMessage;
            if (lineData is Tuple<string>)
            {
                Tuple<string> lineFormat = (Tuple<string>)lineData;
                lineMessage = lineFormat.Item1;
            }
            else if (settings.LineFormatter != null)
            {
                try
                {
                    Tuple<string> lineFormat = settings.LineFormatter(lineData, offsetCounter, pageNumber);
                    if (lineFormat == null)
                        continue;

                    lineMessage = lineFormat.Item1;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("LineFormatter 引用的方法引发了异常。有关详细信息，请参阅内部异常。", ex);
                }
            }
            else
            {
                lineMessage = lineData?.ToString() ?? "";
            }

            if (lineMessage != null)
            {
                _ = builder.Text("\n" + lineMessage);
            }
        }

        if (lineCounter == 0)
        {
            if (settings.NothingToDisplayString != null)
            {
                _ = args.Reply(settings.NothingToDisplayString);
            }
        }
        else if (settings.IncludeFooter && pageNumber + 1 <= pageCount)
        {
            _ = builder.Text("\n" + string.Format(settings.FooterFormat, pageNumber + 1, pageNumber, pageCount));

        }
        _ = args.Reply(builder);
    }

    public static void SendPage(GroupMessageEvent args, int pageNumber, IList dataToPaginate, Settings? settings = null)
    {
        PaginationTools.SendPage(args, pageNumber, dataToPaginate, dataToPaginate.Count, settings);
    }

    public static void SendPage(FriendMessageEvent args, int pageNumber, IList dataToPaginate, Settings? settings = null)
    {
        PaginationTools.SendPage(args, pageNumber, dataToPaginate, dataToPaginate.Count, settings);
    }

    public static List<string> BuildLinesFromTerms(IEnumerable terms, Func<object, string>? termFormatter = null, string separator = ", ", int maxCharsPerLine = 80)
    {
        List<string> lines = [];
        StringBuilder lineBuilder = new();

        foreach (object term in terms)
        {
            if (term == null && termFormatter == null)
                continue;

            string termString;
            if (termFormatter != null)
            {
                try
                {
                    if ((termString = termFormatter(term!)) == null)
                        continue;
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("术语格式化程序表示的方法引发了异常。有关详细信息，请参阅内部异常。", ex);
                }
            }
            else
            {
                termString = term?.ToString() ?? "";
            }

            if (lineBuilder.Length + termString.Length + separator.Length < maxCharsPerLine)
            {
                _ = lineBuilder.Append(termString).Append(separator);
            }
            else
            {
                lines.Add(lineBuilder.ToString());
                _ = lineBuilder.Clear().Append(termString).Append(separator);
            }
        }

        if (lineBuilder.Length > 0)
        {
            lines.Add(lineBuilder.ToString().Substring(0, lineBuilder.Length - separator.Length));
        }
        return lines;
    }

    public static bool TryParsePageNumber(List<string> commandParameters, int expectedParameterIndex, GroupMessageEvent errorMessageReceiver, out int pageNumber)
    {
        pageNumber = 1;
        if (commandParameters.Count <= expectedParameterIndex)
            return true;

        string pageNumberRaw = commandParameters[expectedParameterIndex];
        if (!int.TryParse(pageNumberRaw, out pageNumber) || pageNumber < 1)
        {
            if (errorMessageReceiver != null)
                _ = errorMessageReceiver.Reply(string.Format("“{0}”不是有效的页码。", pageNumberRaw));

            pageNumber = 1;
            return false;
        }

        return true;
    }

    public static bool TryParsePageNumber(List<string> commandParameters, int expectedParameterIndex, FriendMessageEvent errorMessageReceiver, out int pageNumber)
    {
        pageNumber = 1;
        if (commandParameters.Count <= expectedParameterIndex)
            return true;

        string pageNumberRaw = commandParameters[expectedParameterIndex];
        if (!int.TryParse(pageNumberRaw, out pageNumber) || pageNumber < 1)
        {
            if (errorMessageReceiver != null)
                _ = errorMessageReceiver.Reply(string.Format("“{0}”不是有效的页码。", pageNumberRaw));

            pageNumber = 1;
            return false;
        }

        return true;
    }
}
