using System;
using Cream.Commands;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
namespace Cream.Providers
{
    public interface ICrmServiceProvider
    {
        CrmConnection GetCrmConnection();

        CrmOrganizationServiceContext GetCrmContext();

        OrganizationService GetCrmService();

        void Initialize(OptionBase options);
    }
}
