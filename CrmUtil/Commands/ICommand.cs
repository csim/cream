using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrmUtil.Providers;

namespace CrmUtil.Commands
{
    public interface ICommand
    {
        void Execute();
    }

    public interface ICrmCommand : ICommand
    {
        ICrmServiceProvider CrmServiceProvider { get; set; }
    }
}
