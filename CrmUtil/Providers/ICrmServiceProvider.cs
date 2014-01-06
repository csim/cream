using System;
using CrmUtil.Commands.Crm;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
namespace CrmUtil.Providers
{
    public interface ICrmServiceProvider
    {
        CrmConnection GetCrmConnection();

        CrmOrganizationServiceContext GetCrmContext();

        OrganizationService GetCrmService();

        void Initialize(CrmCommonOptionBase options);
    }
}
