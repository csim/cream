using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cream.Providers;

namespace Cream.Commands
{
    public interface ICommand
    {
        void Execute();

        ICrmServiceProvider CrmServiceProvider { get; set; }
    }
}
