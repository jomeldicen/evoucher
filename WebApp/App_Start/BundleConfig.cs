using System.Web;
using System.Web.Optimization;

namespace WebApp
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                         //"~/Scripts/jquery-{version}.js",                         
                        "~/Content/ProjectFile/bower_components/jquery/dist/jquery.min.js",
                        "~/Content/ProjectFile/bower_components/jquery-ui/jquery-ui.min.js",
                        "~/Content/ProjectFile/bower_components/bootstrap/dist/js/bootstrap.min.js",
                        "~/Content/ProjectFile/plugins/jvectormap/jquery-jvectormap-1.2.2.min.js",
                        "~/Content/ProjectFile/plugins/jvectormap/jquery-jvectormap-world-mill-en.js",
                        "~/Content/ProjectFile/bower_components/moment/min/moment.min.js",
                        "~/Content/ProjectFile/bower_components/fastclick/lib/fastclick.js",
                        "~/Content/ProjectFile/js/adminlte.min.js",
                        "~/Content/ProjectFile/js/pages/dashboard.js",
                        "~/Content/ProjectFile/js/demo.js",
                        "~/Scripts/angular.min.js",
                         "~/Scripts/ProjectScriptFile/sweetalert.min.js",
                         "~/Scripts/ProjectScriptFile/app.js"
                         //"~/Content/ProjectFile/jquery.dataTables.min.js",
                         //"~/Content/ProjectFile/aungular-datatable.js",                   
                         ));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/ProjectFile/bower_components/bootstrap/dist/css/bootstrap.min.css",
                      //"~/Content/site.css",
                      "~/Content/ProjectFile/bower_components/font-awesome/css/font-awesome.min.css",
                      "~/Content/ProjectFile/bower_components/Ionicons/css/ionicons.min.css",
                      "~/Content/ProjectFile/css/AdminLTE.min.css",
                      "~/Content/ProjectFile/dist/css/skins/_all-skins.min.css",
                      "~/Content/ProjectFile/plugins/bootstrap-wysihtml5/bootstrap3-wysihtml5.min.css"
                      ));
        }
    }
}
