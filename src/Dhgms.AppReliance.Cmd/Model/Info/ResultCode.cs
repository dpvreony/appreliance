using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dhgms.AppReliance.Cmd.Model.Info
{
    enum ResultCode
    {
        Success = 0,
        InvalidNumberOfArguments,
        FileNotFound,
        DependencyIssue,
        InvalidFileType
    }
}
