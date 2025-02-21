using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Exceptions;
using LinqToDB;
using LinqToDB.Mapping;

namespace Lagrange.XocMat.DB.Manager;

[Table("Sign")]
public class Sign : RecordBase<Sign>
{

    [Column("QQ")]
    [PrimaryKey]
    public long UserId { get; init; }

    [Column("LastDate")]
    public string LastDate { get; set; } = string.Empty;

    [Column("date")]
    public long Date { get; set; }

    private static Context context => Db.Context<Sign>("Sign");

    public static Sign? Query(long id)
    {
        return context.Records.FirstOrDefault(x => x.UserId == id);
    }

    public static List<Sign> GetSigns() => context.Records.ToList();

    public static Sign SingIn(long id)
    {
        Sign? signinfo = Query(id);
        string Now = DateTime.Now.ToString("yyyyMMdd");
        if (signinfo == null)
        {
            Sign signin = new Sign()
            {
                Date = 1,
                UserId = id,
                LastDate = Now,
            };
            context.Insert(signin);
            return signin;
        }
        else
        {
            if (signinfo.LastDate == Now)
            {
                throw new SignException($"{XocMatSetting.Instance.RepeatCheckinNotice}");
            }
            else
            {
                signinfo.Date += 1;
                signinfo.LastDate = Now;
                context.Update(signinfo);
            }
        }
        return signinfo;
    }
}
