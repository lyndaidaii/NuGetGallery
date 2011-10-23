﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17020
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NuGetGallery.Views.Packages
{
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using NuGetGallery;

    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "1.2.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Packages/DisplayPackage.cshtml")]
    public class DisplayPackage : System.Web.Mvc.WebViewPage<DisplayPackageViewModel>
    {
        public DisplayPackage()
        {
        }
        public override void Execute()
        {



#line 2 "..\..\Views\Packages\DisplayPackage.cshtml"

            ViewBag.Tab = "Packages";
            Layout = "~/Views/Shared/TwoColumnLayout.cshtml";



#line default
#line hidden

            DefineSection("SideColumn", () =>
            {

                WriteLiteral("\r\n    <img class=\"logo\" src=\"");



#line 7 "..\..\Views\Packages\DisplayPackage.cshtml"
                Write(Model.IconUrl ?? @Links.Content.Images.packageDefaultIcon_png);


#line default
#line hidden
                WriteLiteral("\" alt=\"Icon for package ");



#line 7 "..\..\Views\Packages\DisplayPackage.cshtml"
                Write(Model.Id);


#line default
#line hidden
                WriteLiteral("\" />\r\n    <div id=\"stats\">\r\n        <div class=\"stat\">\r\n            <p class=\"sta" +
                "t-number\">");



#line 10 "..\..\Views\Packages\DisplayPackage.cshtml"
                Write(Model.TotalDownloadCount.ToString("n0"));


#line default
#line hidden
                WriteLiteral("</p>\r\n            <p class=\"stat-label\">\r\n                Downloads</p>\r\n        " +
                "</div>\r\n        <div class=\"stat\">\r\n            <p class=\"stat-number\">");



#line 15 "..\..\Views\Packages\DisplayPackage.cshtml"
                Write(Model.DownloadCount.ToString("n0"));


#line default
#line hidden
                WriteLiteral("</p>\r\n            <p class=\"stat-label\">\r\n                Downloads of v ");



#line 17 "..\..\Views\Packages\DisplayPackage.cshtml"
                Write(Model.Version);


#line default
#line hidden
                WriteLiteral("</p>\r\n        </div>\r\n        <div class=\"stat\">\r\n            <p class=\"stat-numb" +
                "er\">");



#line 20 "..\..\Views\Packages\DisplayPackage.cshtml"
                Write(Model.LastUpdated.ToShortDateString());


#line default
#line hidden
                WriteLiteral("</p>\r\n            <p class=\"stat-label\">\r\n                Last update</p>\r\n      " +
                "  </div>\r\n    </div>\r\n    <nav>\r\n        <ul class=\"links\">\r\n");



#line 27 "..\..\Views\Packages\DisplayPackage.cshtml"
                if (!String.IsNullOrEmpty(Model.ProjectUrl))
                {


#line default
#line hidden
                    WriteLiteral("                <li><a href=\"");



#line 28 "..\..\Views\Packages\DisplayPackage.cshtml"
                    Write(Model.ProjectUrl);


#line default
#line hidden
                    WriteLiteral("\" title=\"Visit the project site to learn more about this package\">\r\n             " +
                    "       Project site</a></li>\r\n");



#line 30 "..\..\Views\Packages\DisplayPackage.cshtml"
                }


#line default
#line hidden


#line 31 "..\..\Views\Packages\DisplayPackage.cshtml"
                if (!String.IsNullOrEmpty(Model.LicenseUrl))
                {


#line default
#line hidden
                    WriteLiteral("                <li><a href=\"");



#line 32 "..\..\Views\Packages\DisplayPackage.cshtml"
                    Write(Model.LicenseUrl);


#line default
#line hidden
                    WriteLiteral("\" title=\"Make sure you agree with the license\">License</a></li>\r\n");



#line 33 "..\..\Views\Packages\DisplayPackage.cshtml"
                }


#line default
#line hidden
                WriteLiteral("            <li><a href=\"");



#line 34 "..\..\Views\Packages\DisplayPackage.cshtml"
                Write(Url.Action(MVC.Packages.ReportAbuse(Model.Id, Model.Version)));


#line default
#line hidden
                WriteLiteral("\" title=\"Report Abuse\">\r\n                Report Abuse</a></li>\r\n            <li><" +
                "a href=\"");



#line 36 "..\..\Views\Packages\DisplayPackage.cshtml"
                Write(Url.Action(MVC.Packages.ContactOwners(Model.Id)));


#line default
#line hidden
                WriteLiteral("\">Contact Owners</a></li>\r\n");



#line 37 "..\..\Views\Packages\DisplayPackage.cshtml"
                if (Model.IsOwner(User))
                {


#line default
#line hidden
                    WriteLiteral("                <li><a href=\"");



#line 38 "..\..\Views\Packages\DisplayPackage.cshtml"
                    Write(Url.EditPackage(Model));


#line default
#line hidden
                    WriteLiteral("\">Edit Package</a></li>\r\n");



                    WriteLiteral("                <li><a href=\"");



#line 39 "..\..\Views\Packages\DisplayPackage.cshtml"
                    Write(Url.ManagePackageOwners(Model));


#line default
#line hidden
                    WriteLiteral("\">Manage Owners</a></li>\r\n");



                    WriteLiteral("                <li><a href=\"");



#line 40 "..\..\Views\Packages\DisplayPackage.cshtml"
                    Write(Url.DeletePackage(Model));


#line default
#line hidden
                    WriteLiteral("\" class=\"delete\">Delete Package</a></li>\r\n");



#line 41 "..\..\Views\Packages\DisplayPackage.cshtml"
                }


#line default
#line hidden
                WriteLiteral("        </ul>\r\n    </nav>\r\n");


            });

            WriteLiteral("\r\n<div class=\"package-page\">\r\n");



#line 46 "..\..\Views\Packages\DisplayPackage.cshtml"
            if (Model.Prerelease)
            {


#line default
#line hidden
                WriteLiteral("        <p class=\"prerelease-message\">\r\n            This is a prerelease version " +
                "of ");



#line 48 "..\..\Views\Packages\DisplayPackage.cshtml"
                Write(Model.Title);


#line default
#line hidden
                WriteLiteral(".\r\n        </p>\r\n");



#line 50 "..\..\Views\Packages\DisplayPackage.cshtml"
            }
            else if (!Model.LatestVersion)
            {


#line default
#line hidden
                WriteLiteral("        <p class=\"not-latest-message\">\r\n            This is a not the <a href=\"");



#line 53 "..\..\Views\Packages\DisplayPackage.cshtml"
                Write(Url.Package(Model.Id));


#line default
#line hidden
                WriteLiteral("\" title=\"View the latest version\">latest\r\n                version</a> of ");



#line 54 "..\..\Views\Packages\DisplayPackage.cshtml"
                Write(Model.Title);


#line default
#line hidden
                WriteLiteral(".\r\n        </p>\r\n");



#line 56 "..\..\Views\Packages\DisplayPackage.cshtml"
            }


#line default
#line hidden
            WriteLiteral("    <hgroup class=\"page-heading\">\r\n        <h1>");



#line 58 "..\..\Views\Packages\DisplayPackage.cshtml"
            Write(Model.Title);


#line default
#line hidden
            WriteLiteral("</h1>\r\n        <h2>");



#line 59 "..\..\Views\Packages\DisplayPackage.cshtml"
            Write(Model.Version);


#line default
#line hidden
            WriteLiteral("</h2>\r\n    </hgroup>\r\n    <p>");



#line 61 "..\..\Views\Packages\DisplayPackage.cshtml"
            Write(Model.Description);


#line default
#line hidden
            WriteLiteral("</p>\r\n\r\n");



#line 63 "..\..\Views\Packages\DisplayPackage.cshtml"
            if (!Model.Listed && Model.IsOwner(User))
            {


#line default
#line hidden
                WriteLiteral(@"        <p  class=""message"">
            This package is unlisted and hidden from package listings.
            You can see the package because you are one of its owners. To display the package
            in search results and the package feed, <a href=""");



#line 67 "..\..\Views\Packages\DisplayPackage.cshtml"
                Write(Url.EditPackage(Model));


#line default
#line hidden
                WriteLiteral("\">edit the package</a>.\r\n        </p>                             \r\n");



#line 69 "..\..\Views\Packages\DisplayPackage.cshtml"
            }


#line default
#line hidden
            WriteLiteral("    <p>\r\n        To install ");



#line 71 "..\..\Views\Packages\DisplayPackage.cshtml"
            Write(Model.Title);


#line default
#line hidden
            WriteLiteral(", run the following command in the <a href=\"http://docs.nuget.org/docs/start-here" +
            "/using-the-package-manager-console\">\r\n            Package Manager Console</a>\r\n " +
            "   </p>\r\n    <div class=\"nuget-badge\">\r\n        <p>\r\n            <code>PM&gt; In" +
            "stall-Package ");



#line 76 "..\..\Views\Packages\DisplayPackage.cshtml"
            Write(Model.Id);


#line default
#line hidden
            WriteLiteral("\r\n");



#line 77 "..\..\Views\Packages\DisplayPackage.cshtml"
            if (!Model.LatestVersion)
            {

#line default
#line hidden
                WriteLiteral(" -Version ");



#line 77 "..\..\Views\Packages\DisplayPackage.cshtml"
                Write(Model.Version);


#line default
#line hidden


#line 77 "..\..\Views\Packages\DisplayPackage.cshtml"
            }


#line default
#line hidden


#line 78 "..\..\Views\Packages\DisplayPackage.cshtml"
            if (Model.Prerelease)
            {

#line default
#line hidden
                WriteLiteral(" -pre ");



#line 78 "..\..\Views\Packages\DisplayPackage.cshtml"
            }

#line default
#line hidden
            WriteLiteral("</code></p>\r\n    </div>\r\n\r\n    <p>\r\n        <a href=\"");



#line 82 "..\..\Views\Packages\DisplayPackage.cshtml"
            Write(Url.Action(@MVC.Packages.Download()));


#line default
#line hidden
            WriteLiteral("\" title=\"Download link\">Where is the <strong>Download link?</strong></a>\r\n    </p" +
            ">\r\n\r\n    <h3>Release Notes</h3>\r\n    <p>\r\n        \r\n    </p>\r\n\r\n    <h3>Owners</" +
            "h3>\r\n    ");



#line 91 "..\..\Views\Packages\DisplayPackage.cshtml"
            Write(ViewHelpers.OwnersGravatar(Model.Owners, 32, Url));


#line default
#line hidden
            WriteLiteral("\r\n    <h3>Authors</h3>\r\n    <ul class=\"authors\">\r\n");



#line 94 "..\..\Views\Packages\DisplayPackage.cshtml"
            foreach (var author in Model.Authors)
            {


#line default
#line hidden
                WriteLiteral("            <li><a href=\"");



#line 95 "..\..\Views\Packages\DisplayPackage.cshtml"
                Write(Url.Search(author.Name));


#line default
#line hidden
                WriteLiteral("\" title=\"Search for ");



#line 95 "..\..\Views\Packages\DisplayPackage.cshtml"
                Write(author.Name);


#line default
#line hidden
                WriteLiteral("\">");



#line 95 "..\..\Views\Packages\DisplayPackage.cshtml"
                Write(author.Name);


#line default
#line hidden
                WriteLiteral("</a></li>\r\n");



#line 96 "..\..\Views\Packages\DisplayPackage.cshtml"
            }


#line default
#line hidden
            WriteLiteral("    </ul>\r\n");



#line 98 "..\..\Views\Packages\DisplayPackage.cshtml"
            if (!String.IsNullOrEmpty(Model.Copyright))
            {


#line default
#line hidden
                WriteLiteral("        <h3>Copyright</h3>\r\n");



                WriteLiteral("        <p>");



#line 100 "..\..\Views\Packages\DisplayPackage.cshtml"
                Write(Model.Copyright);


#line default
#line hidden
                WriteLiteral("</p>\r\n");



#line 101 "..\..\Views\Packages\DisplayPackage.cshtml"
            }


#line default
#line hidden


#line 102 "..\..\Views\Packages\DisplayPackage.cshtml"
            if (@Model.Tags.AnySafe())
            {


#line default
#line hidden
                WriteLiteral("        <h3>Tags</h3>\r\n");



                WriteLiteral("        <ul class=\"tags\">\r\n");



#line 105 "..\..\Views\Packages\DisplayPackage.cshtml"
                foreach (var tag in Model.Tags)
                {


#line default
#line hidden
                    WriteLiteral("                <li><a href=\"");



#line 106 "..\..\Views\Packages\DisplayPackage.cshtml"
                    Write(Url.Search(tag));


#line default
#line hidden
                    WriteLiteral("\" title=\"Search for ");



#line 106 "..\..\Views\Packages\DisplayPackage.cshtml"
                    Write(tag);


#line default
#line hidden
                    WriteLiteral("\">");



#line 106 "..\..\Views\Packages\DisplayPackage.cshtml"
                    Write(tag);


#line default
#line hidden
                    WriteLiteral("</a></li>\r\n");



#line 107 "..\..\Views\Packages\DisplayPackage.cshtml"
                }


#line default
#line hidden
                WriteLiteral("        </ul>\r\n");



#line 109 "..\..\Views\Packages\DisplayPackage.cshtml"
            }


#line default
#line hidden
            WriteLiteral("    <h3>Dependencies</h3>\r\n");



#line 111 "..\..\Views\Packages\DisplayPackage.cshtml"
            if (Model.Dependencies.Any())
            {


#line default
#line hidden
                WriteLiteral(@"        <table class=""sexy-table"">
            <thead>
                <tr>
                    <th class=""first"">
                        Id
                    </th>
                    <th class=""last"">
                        Version Range
                    </th>
                </tr>
            </thead>
            <tbody>
");



#line 124 "..\..\Views\Packages\DisplayPackage.cshtml"
                foreach (var dependency in Model.Dependencies)
                {


#line default
#line hidden
                    WriteLiteral("                    <tr>\r\n                        <td>\r\n                         " +
                    "   <a href=\"");



#line 127 "..\..\Views\Packages\DisplayPackage.cshtml"
                    Write(Url.Package(dependency.Id));


#line default
#line hidden
                    WriteLiteral("\">");



#line 127 "..\..\Views\Packages\DisplayPackage.cshtml"
                    Write(dependency.Id);


#line default
#line hidden
                    WriteLiteral("</a>\r\n                        </td>\r\n                        <td>");



#line 129 "..\..\Views\Packages\DisplayPackage.cshtml"
                    Write(dependency.VersionSpec);


#line default
#line hidden
                    WriteLiteral("\r\n                        </td>\r\n                    </tr>\r\n");



#line 132 "..\..\Views\Packages\DisplayPackage.cshtml"
                }


#line default
#line hidden
                WriteLiteral("            </tbody>\r\n        </table>\r\n");



#line 135 "..\..\Views\Packages\DisplayPackage.cshtml"
            }
            else
            {


#line default
#line hidden
                WriteLiteral("        <p>");



#line 137 "..\..\Views\Packages\DisplayPackage.cshtml"
                Write(Model.Id);


#line default
#line hidden
                WriteLiteral(" ");



#line 137 "..\..\Views\Packages\DisplayPackage.cshtml"
                Write(Model.Version);


#line default
#line hidden
                WriteLiteral(" does not have any dependencies\r\n        </p>\r\n");



#line 139 "..\..\Views\Packages\DisplayPackage.cshtml"
            }


#line default
#line hidden
            WriteLiteral(@"    <h3>Version History</h3>
    <table class=""sexy-table"">
        <thead>
            <tr>
                <th class=""first"">
                    Version
                </th>
                <th>
                    Downloads
                </th>
                <th class=""last"">
                    Last updated
                </th>
            </tr>
        </thead>
        <tbody>
");



#line 156 "..\..\Views\Packages\DisplayPackage.cshtml"
            foreach (var packageVersion in Model.PackageVersions)
            {


#line default
#line hidden


#line 157 "..\..\Views\Packages\DisplayPackage.cshtml"
                WriteLiteral("                <tr class=\"versionTableRow ");


#line default
#line hidden

#line 157 "..\..\Views\Packages\DisplayPackage.cshtml"
                if (packageVersion.LatestVersion)
                {

#line default
#line hidden
                    WriteLiteral("recommended ");



#line 157 "..\..\Views\Packages\DisplayPackage.cshtml"
                }

#line default
#line hidden


#line 157 "..\..\Views\Packages\DisplayPackage.cshtml"
                WriteLiteral("\">\r\n                    <td class=\"version\" ");


#line default
#line hidden

#line 158 "..\..\Views\Packages\DisplayPackage.cshtml"
                if (packageVersion.LatestVersion)
                {

#line default
#line hidden
                    WriteLiteral("title=\"Latest Version\"");



#line 158 "..\..\Views\Packages\DisplayPackage.cshtml"
                }

#line default
#line hidden
                WriteLiteral(">\r\n");



#line 159 "..\..\Views\Packages\DisplayPackage.cshtml"
                if (!packageVersion.IsCurrent(Model))
                {


#line default
#line hidden
                    WriteLiteral("                            <a href=\"");



#line 160 "..\..\Views\Packages\DisplayPackage.cshtml"
                    Write(Url.Package(packageVersion));


#line default
#line hidden
                    WriteLiteral("\">");



#line 160 "..\..\Views\Packages\DisplayPackage.cshtml"
                    Write(packageVersion.Title);


#line default
#line hidden
                    WriteLiteral(" ");



#line 160 "..\..\Views\Packages\DisplayPackage.cshtml"
                    Write(packageVersion.Version);


#line default
#line hidden
                    WriteLiteral("</a>\r\n");



#line 161 "..\..\Views\Packages\DisplayPackage.cshtml"
                }
                else
                {


#line default
#line hidden
                    WriteLiteral("                            <span>");



#line 163 "..\..\Views\Packages\DisplayPackage.cshtml"
                    Write(packageVersion.Title);


#line default
#line hidden
                    WriteLiteral(" ");



#line 163 "..\..\Views\Packages\DisplayPackage.cshtml"
                    Write(packageVersion.Version);


#line default
#line hidden
                    WriteLiteral("</span>\r\n");



#line 164 "..\..\Views\Packages\DisplayPackage.cshtml"
                }


#line default
#line hidden
                WriteLiteral("                    </td>\r\n                    <td>");



#line 166 "..\..\Views\Packages\DisplayPackage.cshtml"
                Write(packageVersion.DownloadCount);


#line default
#line hidden
                WriteLiteral("\r\n                    </td>\r\n                    <td>");



#line 168 "..\..\Views\Packages\DisplayPackage.cshtml"
                Write(packageVersion.LastUpdated.ToString("D"));


#line default
#line hidden
                WriteLiteral("\r\n                    </td>\r\n                </tr>\r\n");



#line 171 "..\..\Views\Packages\DisplayPackage.cshtml"
            }


#line default
#line hidden
            WriteLiteral("        </tbody>\r\n    </table>\r\n</div>\r\n");


        }
    }
}
#pragma warning restore 1591
