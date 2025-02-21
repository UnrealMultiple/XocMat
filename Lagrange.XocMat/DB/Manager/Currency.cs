using LinqToDB;
using LinqToDB.Mapping;


namespace Lagrange.XocMat.DB.Manager;

[Table("Currency")]
public class Currency : RecordBase<Currency>
{
    [Column("QQ")]
    [PrimaryKey]
    public long UserId { get; set; }

    [Column("num")]
    public long Num { get; set; }

    private static Context context => Db.Context<Currency>("Currency");

    public static Currency? Query(long id)
    {
        return context.Records.FirstOrDefault(x => x.UserId == id);
    }

    public static Currency? Del(long id, long num)
    {
        Currency usercurr = Query(id) ?? throw new Exception("用户没有星币可以扣除!");
        usercurr.Num -= num;
        context.Update(usercurr);
        return usercurr;
    }


    public static Currency Add(long id, long num)
    {
        Currency? usercurr = Query(id);
        if (usercurr == null)
        {
            Currency curr = new Currency()
            {
                UserId = id,
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
