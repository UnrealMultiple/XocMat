using Lagrange.Core.Common.Entity;
using Lagrange.Core.Message;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Entity;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;
using MessagePack;
using MessagePack.Resolvers;
using static Lagrange.Core.Message.MessageChain;

namespace Lagrange.XocMat.DB.Manager;


[Table("MessageRecord")]
public class MessageRecord : RecordBase<MessageRecord>
{
    public static readonly MessagePackSerializerOptions OPTIONS = MessagePackSerializerOptions.Standard
        .WithResolver(CompositeResolver.Create(
            new MessageEntityResolver(),
            ContractlessStandardResolverAllowPrivate.Instance,
            ContractlessStandardResolver.Instance
        ));

    [PrimaryKey]
    public int Id { get; set; }

    [Column(nameof(Type))]
    public int TypeInt { get; set; }

    public MessageType Type { get => (MessageType)TypeInt; set => TypeInt = (int)value; }

    [Column(nameof(Sequence))]
    public long SequenceLong { get; set; }
    public ulong Sequence { get => (ulong)SequenceLong; set => SequenceLong = (long)value; }

    [Column(nameof(ClientSequence))]
    public long ClientSequenceLong { get; set; }

    public ulong ClientSequence { get => (ulong)ClientSequenceLong; set => ClientSequenceLong = (long)value; }

    [Column(nameof(MessageId))]
    public long MessageIdLong { get; set; }

    public ulong MessageId { get => (ulong)MessageIdLong; set => MessageIdLong = (long)value; }

    public DateTimeOffset Time { get; set; }

    [Column(nameof(FromUin))]
    public long FromUinLong { get; set; }

    public ulong FromUin { get => (ulong)FromUinLong; set => FromUinLong = (long)value; }

    [Column(nameof(ToUin))]
    public long ToUinLong { get; set; }

    public ulong ToUin { get => (ulong)ToUinLong; set => ToUinLong = (long)value; }

    [Column("Entities", DataType = DataType.VarBinary)]
    public byte[] Entities { get; set; } = [];

    private static Context Contexts => Db.Context<MessageRecord>("MessageRecord");

    public static MessageChain? Query(ulong messageid)
    {
        return Contexts.Records.FirstOrDefault(x => (ulong)x.MessageIdLong == messageid)!;
    }

    //SQLite Error 1: 'near "LIMIT": syntax error'.
    internal static void Insert(MessageRecord record)
    {
        var currentCount = Contexts.Records.Count();
        if (currentCount > XocMatSetting.Instance.MaxCacheMessage)
        {
            var sql = @"
                DELETE FROM MessageRecord
                WHERE ROWID IN (
                    SELECT ROWID FROM MessageRecord
                    ORDER BY ROWID
                    LIMIT @n
                )";
            Contexts.Execute(sql, new { n = Math.Min(XocMatSetting.Instance.DeleteCacheMessage, XocMatSetting.Instance.MaxCacheMessage) });   
        }
        Contexts.Insert(record);
    }

    public static int CalcMessageHash(ulong msgId, uint seq)
    {
        return ((ushort)seq << 16) | (ushort)msgId;
    }

    public static implicit operator MessageRecord(MessageChain chain) => new()
    {
        Id = CalcMessageHash(chain.MessageId, chain.Sequence),
        Type = chain.Type,
        Sequence = chain.Sequence,
        ClientSequence = chain.ClientSequence,
        MessageId = chain.MessageId,
        Time = chain.Time,
        FromUin = chain.FriendUin,
        ToUin = chain.Type switch
        {
            MessageType.Group => (ulong)chain.GroupUin!,
            MessageType.Temp or
            MessageType.Friend => chain.TargetUin,
            _ => throw new NotImplementedException(),
        },
        
        Entities = MessagePackSerializer.Serialize<List<IMessageEntity>>(chain, OPTIONS)
    };

    public static implicit operator MessageChain(MessageRecord record)
    {
        var chain = record.Type switch
        {
            MessageType.Group => new MessageChain(
                (uint)record.ToUin,
                (uint)record.FromUin,
                (uint)record.Sequence,
                record.MessageId
            ),
            MessageType.Temp or
            MessageType.Friend => new MessageChain(
                (uint)record.FromUin,
                string.Empty,
                string.Empty,
                (uint)record.ToUin,
                (uint)record.Sequence,
                (uint)record.ClientSequence,
                record.MessageId
            ),
            _ => throw new NotImplementedException(),
        };

        var entities = MessagePackSerializer.Deserialize<List<IMessageEntity>>(record.Entities, OPTIONS);
        chain.AddRange(entities);
        chain.Time = record.Time.DateTime;

        return chain;
    }
}
