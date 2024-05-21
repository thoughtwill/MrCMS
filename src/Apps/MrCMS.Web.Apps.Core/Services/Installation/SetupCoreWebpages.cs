using MrCMS.Entities.Documents.Web;
using MrCMS.Entities.Documents.Web.FormProperties;
using MrCMS.Helpers;
using MrCMS.Settings;
using MrCMS.Web.Apps.Core.Pages;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MrCMS.Web.Apps.Core.Services.Installation
{
    public class SetupCoreWebpages : ISetupCoreWebpages
    {
        private readonly ISession _session;
        private readonly IConfigurationProvider _configurationProvider;

        public SetupCoreWebpages(ISession session, IConfigurationProvider configurationProvider)
        {
            _session = session;
            _configurationProvider = configurationProvider;
        }


        public async Task Setup()
        {
            var form = await AddContactUsForm();
            await _session.TransactAsync(async session =>
            {
                foreach (var page in GetBasicPages(form))
                {
                    await session.SaveAsync(page);
                }
            });

            var siteSettings = _configurationProvider.GetSiteSettings<SiteSettings>();
            await _configurationProvider.SaveSettings(siteSettings);

            await _session.TransactAsync(async session =>
            {
                await session.SaveAsync(GetSearchPage());

                var webpages = await _session.QueryOver<Webpage>().ListAsync();
                var publishOn = DateTime.UtcNow;
                foreach (var webpage in webpages)
                {
                    webpage.PublishOn = publishOn;
                    await session.UpdateAsync(webpage);
                }
            });
        }

        private SearchPage GetSearchPage()
        {
            return new SearchPage
            {
                Name = "Search",
                UrlSegment = "search",
            };
        }


        private IReadOnlyList<Webpage> GetBasicPages(Form contactUsForm)
        {
            var webpages = new List<Webpage>();
            var homePage = new TextPage
            {
                Name = "Home",
                UrlSegment = "home",
                BodyContent =
                    "<h1>Mr CMS</h1> <p>Welcome to Mr CMS.</p><p> Turn on inline editing above, then click here to start editing. </p>",
                RevealInNavigation = true,
            };
            //CurrentRequestData.HomePage = homePage;
            webpages.Add(homePage);
            webpages.Add(new TextPage
            {
                Name = "Page 2",
                UrlSegment = "page-2",
                BodyContent = "<h1>Another page</h1><p>Just another page!</p>",
                RevealInNavigation = true,
            });
            //contact us
            var contactUs = new TextPage
            {
                Name = "Contact us",
                UrlSegment = "contact-us",
                BodyContent =
                    $"<h1>Contact</h1>Contact us at www.mrcms.com (coming soon). <p>Test form</a> [form id=\"{contactUsForm.Id}\"]",
                RevealInNavigation = true,
            };
            webpages.Add(contactUs);
            return webpages;
        }


        private async Task<Form> AddContactUsForm()
        {
            var form = new Form {Name = "Contact Us Form"};
            var fieldName = new TextBox
            {
                Name = "Name",
                LabelText = "Your Name",
                Required = true,
                Form = form,
                DisplayOrder = 0
            };

            var fieldEmail = new Email
            {
                Name = "Email",
                LabelText = "Your Email",
                Required = true,
                Form = form,
                DisplayOrder = 1
            };
            await _session.TransactAsync(async s =>
            {
                await s.SaveAsync(form);
                await s.SaveAsync(fieldName);
                await s.SaveAsync(fieldEmail);
            });
            return form;
        }
    }
}