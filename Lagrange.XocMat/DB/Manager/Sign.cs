using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Exceptions;
using LinqToDB;
using LinqToDB.Mapping;

namespace Lagrange.XocMat.DB.Manager;

[Table("Sign")]
public class Sign : RecordBase<Sign>
{
    [Column("GroupId")]
    [PrimaryKey(2)]
    public long GroupID { get; init; }

    [Column("QQ")]
    [PrimaryKey(1)]
    public long UserId { get; init; }

    [Column("LastDate")]
    public string LastDate { get; set; } = string.Empty;

    [Column("date")]
    public long Date { get; set; }

    private static Context context => Db.Context<Sign>("Sign");

    public static Sign? Query(long groupid, long id)
    {
        return context.Records.FirstOrDefault(x => x.GroupID == groupid && x.UserId == id);
    }

    public static List<Sign> GetSigns() => context.Records.ToList();

    public static Sign SingIn(long groupid, long id)
    {
        var signinfo = Query(groupid, id);
        var Now = DateTime.Now.ToString("yyyyMMdd");
        if (signinfo == null)
        {
            var signin = new Sign()
            {
                Date = 1,
                UserId = id,
                GroupID = groupid,
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
