using Markdig;
using PuppeteerSharp;

namespace Lagrange.XocMat.Utility;

internal class MarkdownHelper
{
    private static IBrowser? browser = null;

    private static readonly Dictionary<string, string> ReplaceDic = new()
    {
        { "\n", "\\n" },
        { "\r\n", "\\n" },
        { "\r", "\\n" },
        { "'", "\\'" },
        { "|", "\\|" }
    };

    public static async Task<byte[]> ToImage(string md)
    {
        if (browser == null || !browser.IsConnected || browser.IsClosed || browser.Process.HasExited)
        {
            await new BrowserFetcher().DownloadAsync();
            browser = await Puppeteer.LaunchAsync(new LaunchOptions()
            {
                Headless = true,
            });
        }

        using IPage Page = await browser.NewPageAsync();
        await Page.GoToAsync($"http://docs.oiapi.net/view.php?theme=light", 5000).ConfigureAwait(false);
        IElementHandle App = await Page.QuerySelectorAsync("body").ConfigureAwait(false);
        await Page.WaitForNetworkIdleAsync(new()
        {
            Timeout = 5000
        });
        string guid = Guid.NewGuid().ToString();
        MarkdownPipeline option = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseAlertBlocks()
            .UsePipeTables()
            .UseEmphasisExtras()
            .UseListExtras()
            .UseSoftlineBreakAsHardlineBreak()
            .UseFootnotes()
            .UseFooters()
            .UseCitations()
            .UseGenericAttributes()
            .UseGridTables()
            .UseAbbreviations()
            .UseEmojiAndSmiley()
            .UseDefinitionLists()
            .UseCustomContainers()
            .UseFigures()
            .UseMathematics()
            .UseBootstrap()
            .UseMediaLinks()
            .UseSmartyPants()
            .UseAutoIdentifiers()
            .UseTaskLists()
            .UseDiagrams()
            .UseYamlFrontMatter()
            .UseNonAsciiNoEscape()
            .UseAutoLinks()
            .UseGlobalization()
            .Build();
        string postData = Markdig.Markdown.ToHtml(md, option);
        foreach ((string oldChar, string newChar) in ReplaceDic)
        {
            postData = postData.Replace(oldChar, newChar);
        }

        await Page.EvaluateExpressionAsync("document.body.style.backgroundColor = 'white'");
        await Page.EvaluateExpressionAsync($"document.querySelector('#app').innerHTML = '{postData.Trim()}'");
        await App!.EvaluateFunctionAsync("element => element.style.width = 'fit-content'");

        BoundingBox clip = await App!.BoundingBoxAsync().ConfigureAwait(false);
        byte[] ret = await Page.ScreenshotDataAsync(new()
        {
            Clip = new()
            {
                Width = clip!.Width,
                Height = clip.Height,
                X = clip.X,
                Y = clip.Y
            },
            Type = ScreenshotType.Png,
        });
        return ret;
    }
}
