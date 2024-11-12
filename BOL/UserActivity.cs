using BOL.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOL;
public class UserActivity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime DateTime { get; set; }
    [MaxValue(300)]
    public string Description { get; set; }
    public LogActivity Activity { get; set; }

    //Additional Property
    public DateTime DateStart { get; set; } = DateTime.Now.AddDays(-10);
    public DateTime DateEnd { get; set; } = DateTime.Now;
}
