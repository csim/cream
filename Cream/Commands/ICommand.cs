using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cream.Providers;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;

namespace Cream.Commands
{
    public interface ICommand
    {
        void Execute();
    }
}
