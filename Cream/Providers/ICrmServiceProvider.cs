using System;
using Cream.Commands;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
namespace Cream.Providers
{
    public interface ICrmServiceProvider
    {
        CrmConnection Connection { get; }

        CrmOrganizationServiceContext Context { get; }

        IOrganizationService Service { get; }
    }
}
