/*Task 3: Lead Conversion Workflow
Build a custom workflow that automatically converts a Lead to an Account, 
Contact, and Opportunity when the Lead's status changes to "Qualified." 
The workflow should create the relevant records and establish the necessary relationships.*/

using System;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk.Query;


namespace CustomWorkflow
{
    public class Task3Activity : CodeActivity
    {
        protected override void Execute(CodeActivityContext context)
        {
            // Get the workflow context and service objects
            IWorkflowContext workflowContext = context.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(workflowContext.UserId);
            ITracingService tracing = context.GetExtension<ITracingService>();

            // Get the Lead record
            Entity lead = service.Retrieve("lead", workflowContext.PrimaryEntityId, new ColumnSet("statuscode", "fullname", "companyname", "telephone1", "emailaddress1"));

            // Check if the Lead's status is "Qualified"
            if (lead.Contains("statuscode") && lead.GetAttributeValue<OptionSetValue>("statuscode").Value == 3)
            {
                // Create Opportunity
                Entity opportunity = new Entity("opportunity");
                opportunity["name"] = lead.GetAttributeValue<string>("companyname");
                opportunity["customerid"] = new EntityReference("account", CreateAccount(lead, service));
                opportunity["parentcontactid"] = new EntityReference("contact", CreateContact(lead, service));
                service.Create(opportunity);
            }
        }

        // Create Account and Contact records and return their IDs
        private Guid CreateAccount(Entity lead, IOrganizationService service)
        {
            Entity account = new Entity("account");
            account["name"] = lead.GetAttributeValue<string>("companyname");
            return service.Create(account);
        }

        private Guid CreateContact(Entity lead, IOrganizationService service)
        {
            Entity contact = new Entity("contact");
            contact["firstname"] = lead.GetAttributeValue<string>("fullname");
            contact["lastname"] = "Generated from Lead";
            contact["emailaddress1"] = lead.GetAttributeValue<string>("emailaddress1");
            contact["telephone1"] = lead.GetAttributeValue<string>("telephone1");
            return service.Create(contact);
        }
    }
}

