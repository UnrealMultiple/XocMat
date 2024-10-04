using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lagrange.XocMat.Extensions;

public static class MessageChainExt
{
    public static IEnumerable<MentionEntity> GetMention(this MessageChain chain)
    {
        return chain.Where(c => c is MentionEntity).Select(c => ((MentionEntity)c));
    }

    public static IEnumerable<ImageEntity> GetImage(this MessageChain chain)
    {
        return chain.Where(c => c is ImageEntity).Select(c => ((ImageEntity)c));
    }

    public static string GetText(this MessageChain chain)
    {
        return string.Join("", chain.Where(c => c is TextEntity).Select(c => ((TextEntity)c)));
    }

    public static FileEntity? GetFile(this MessageChain chain)
    {
        return (FileEntity)chain.Where(c => c is FileEntity).First();
    }
}
