using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;

namespace Lagrange.XocMat.Extensions;

public static class MessageChainExtension
{
    public static IEnumerable<MentionEntity> GetMention(this MessageChain chain)
    {
        return chain.GetMsg<MentionEntity>();
    }

    public static IEnumerable<ImageEntity> GetImage(this MessageChain chain)
    {
        return chain.GetMsg<ImageEntity>();
    }

    public static string GetText(this MessageChain chain)
    {
        return chain.GetMsg<TextEntity>().JoinToString("", t => t.Text);
    }

    public static FileEntity? GetFile(this MessageChain chain)
    {
        return chain.GetMsg<FileEntity>().FirstOrDefault();
    }

    public static IEnumerable<T> GetMsg<T>(this MessageChain chain) where T : IMessageEntity
    {
        return chain.Where(c => c is T).Select(c => (T)c);
    }
}
