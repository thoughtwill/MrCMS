# Mr CMS

**Mr CMS is an open-source content management framework built on ASP.NET Core**

Mr CMS simplifies many of the time-consuming aspects of website creation, providing both a framework for developers to create bespoke websites and an easy-to-use CMS for content editors.

## Apps
Mr CMS is modular, with functionality encapsulated in "Apps". For example, a Blog App might include definitions for a blog list and individual blog posts. Mr CMS includes three basic apps:

- **Core**: Basic functionalities such as login and registration, alongside generic page types like 'TextPage'.
- **Articles**: Dedicated functionalities for article management.
- **Galleries**: Features for managing image galleries.

Developers are encouraged to modify or replace these apps as needed to suit specific requirements.
### Creating your first Mr CMS App
1. **Create the App:**
   - Create a new folder in the `Apps` directory (e.g., `Blog`).
   - Inside this folder, create a file named `BlogApp.cs`:

    ```csharp
    public class BlogApp : StandardMrCMSApp
    {
        public override string AppName => "Blog";
        public override string Version => "0.1";

        public override void SetupMvcOptions(MvcOptions options)
        {
        }
    }
    ```

2. **Define a New Page Type:**
   - Create a directory `Apps\BlogApp\Pages` and add a file named `Blog.cs`:

    ```csharp
    public class Blog : Webpage
    {
        [DisplayName("Featured Image")]
        public virtual string FeatureImage { get; set; }

        [AllowHtml]
        [StringLength(160, ErrorMessage = "Abstract cannot be longer than 500 characters.")]
        public virtual string Abstract { get; set; }
    }
    ```

3. **Create the View:**
   - Create a folder `Views\Pages` and add a Razor view to display the Blog:

    ```html
    <article>
        <div class="row">
            <div class="col-md-12">
                <h1 class="margin-top-0">@Editable(Model, p => p.Name, false)</h1>
                <a href="/@Model.Parent.UrlSegment" class="btn btn-default">Back</a>
                @Model.CreatedOn.Day @Model.CreatedOn.ToString("MMMMM") @Model.CreatedOn.Year
                <br />
                @if (!String.IsNullOrEmpty(Model.FeatureImage))
                {
                    <a href="/@Model.LiveUrlSegment" class="margin-top-0">@RenderImage(Model.FeatureImage)</a>
                }
                @Editable(Model, page => page.BodyContent, true)
            </div>
        </div>
    </article>
    ```

4. **Admin View for Editing:**
   - Add a view in `Apps\BlogApp\Areas\Admin\Views\Webpage` to edit the new fields:

    ```html
    <div class="form-group">
        @Html.LabelFor(model => model.FeatureImage, "Article Image")
        <br />
        @Html.TextBoxFor(model => model.FeatureImage, new { data_type = "media-selector" })
    </div>
    <div class="form-group">
        @Html.LabelFor(model => model.Abstract)
        @Html.TextAreaFor(model => model.Abstract, new { @rows = "2", @class = "form-control" })
    </div>
    ```### Creating your first Mr CMS App


## Feature list

*   Unlimited document types
*   Lucene based search architecture - easily create search indexes for super fast content filtering and searching of '000's of items.
*   Layouts & Layout areas give complete control over widgets on each page
*   Bulk media upload and management
*   Azure or file based media storage
*   Inline content editing
*   Error logging
*   Task management
*   SEO - Control over meta details
*   URL History table - keep track of URL changes. When a URL changes create 301 redirect to new location
*   URL History - add URLs at will to create redirects or if importing from another system add in the original URLs here
*   Form builder - create forms on the fly and collect their data
*   Enforce user login to page
*   SSL per page
*   User management & roles
*   Document version control
*   Complete control of page meta data - I.E when creating a page type (e.g BlogPage) control what type of pages can be added below it, say weather the page can maintain url hierarchy, say how many children should show in the page tree in item (if there are 000's of pages you might only show the top 5 pages) and lots more
*   Content managed into apps
*   Themes - override standard views
*   Multi-site capability - run multiple domains through one set of files & admin system
*   ACL
*   Webpage import and export
*   Site duplication button to duplicate a website quickly
*   Azure support

## Release History

## 1.0 - TBC 2019
*   Feature: Upgrade to .NET Core
*   Feature: Bootstrap 4 integration in admin / Core app
*   Feature: Notifications (Web Push)
*   Feature: Admin LTE admin base for more responsive experience

## 0.6 - January 2019
*	Feature: Restore Cloudflare IP
*	Feature: Tracking Scripts (top of body). Ability to add scripts below <body> (e.g Google AdTag manager iFrame) 
*	Feature: Add test email functionality
*	Feature: Raygun implementation for centralised logging 
*	Feature/SEO: Self referencing canonical or over-ride on SEO and properties tab
*   Feature: Move and Merge Webpages
*	Performance: Full page caching. Ability to output cache a full webpage. 
*	Fix: Lucene write lock when cache cleared and index is being written to.
*	Security: Record logins 
*	Security: Email Alerts for login 
*	Security: Monitor the scripts per page (header/footer). Alert on change. 
*	Security: Two factor authentication via email 
*	Security: File types now configured in web.config 
*	Note: Now targets .NET 4.7

## 0.5.1 - September 2016
*   Swapped out old Azure caching for Redis cache
*   Brought in the concept of a staging URL and staging Robots.txt - this allows us to use Azure Deployment Slots
*   Message queue now allows attachments
*   The Mr CMS admin logo can now be swapped out through System Settings - Site Settings
*   Removed Self Execute Tasks which uses HostingEnvironment.QueueBackgroundWorkItem due to unreliability of execution. Instead use external task system to poll the task executor URL.
*   Files are moved off disk for use with Azure websites scaling and deployment slots
*   Settings are migrated back to the database. Note: Remove unused .json files from App_Data/Settings. Settings will auto migrate but issues may occur if unused .json files are in the directory
*   Message templates moved back to database
*   Connection string now in ConnectionStrings.Config
*   Sitemaps now output to disk and are scheduled for rebuild via tasks
*   Favicons can be rendered at different sizes using  @Html.RenderFavicon(size: new Size(16, 16))
*   Better media management and uploading notifications
*   Azure probe settings for using traffic manager
*   Added more ACL rules for core functionality
*   Performance: Nhibernate configured for non-eager loading of entities to improve database performance
*   Performance: Limited URL lengths to 450 to allow for indexes to be applied in SQL Server
*   Performance: Whitespace filter now compiled 
*   Fixed: Daylight savings date/time issue
*   Fixed: Empty writes to the universal search index
*   Fixed: Issue with the close icon on the inline widget editing
*   Fixed: 404s were returning 404 status but with a blank response. Fixed to return 404 HTML see Views/Error/FileNotFound.cshtml
*   Breaking Change: Localised scripts now rendered separately to other scripts using @{ @Html.RenderLocalisedScripts();}
*   Breaking Change: Rewrite of the task execution system so that tasks are system wide rather than site specific. Tasks can now be enabled or disabled. This is a breaking change and tasks should be updated to disable site filters for any multisite websites.
*   Breaking Change: Mail settings are set at the system level rather than site level. Mail settings will need to be reconfigured
*   Breaking Change: System settings now stored in mrcms.config - the new location for hosting specific settings
*   Breaking Change: Remove Azure Directory for Lucene (We are slowly moving away from Lucene and looking at other solutions which work better with cloud based hosting and scaling)
