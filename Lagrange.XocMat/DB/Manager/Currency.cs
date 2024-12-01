using LinqToDB;
using LinqToDB.Mapping;


namespace Lagrange.XocMat.DB.Manager;

[Table("Currency")]
public class Currency : RecordBase<Currency>
{
    [Column("GroupId")]
    [PrimaryKey(2)]
    public long GroupID { get; set; }

    [Column("QQ")]
    [PrimaryKey(1)]
    public long UserId { get; set; }

    [Column("num")]
    public long Num { get; set; }

    private static Context context => Db.Context<Currency>("Currency");

    public static Currency? Query(long groupid, long id)
    {
        return context.Records.FirstOrDefault(x => x.GroupID == groupid && x.UserId == id);
    }

    public static Currency? Del(long groupid, long id, long num)
    {
        var usercurr = Query(groupid, id) ?? throw new Exception("用户没有星币可以扣除!");
        usercurr.Num -= num;
        context.Update(usercurr);
        return usercurr;
    }


    public static Currency Add(long groupid, long id, long num)
    {
        var usercurr = Query(groupid, id);
        if (usercurr == null)
        {
            var curr = new Currency()
            {
                UserId = id,
                GroupID = groupid,
                Num = num
            };
            context.Insert(curr);
            return curr;
        }
        else
        {
            usercurr.Num += num;
            context.Update(usercurr);
        }
        return usercurr;
    }
}
