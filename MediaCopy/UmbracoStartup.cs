using System.Linq;
using System.Web.Mvc;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Trees;

namespace MediaCopy
{
    public class UmbracoStartup : IApplicationEventHandler
    {
        public void OnApplicationInitialized(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
        }

        public void OnApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
        }

        public void OnApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            TreeControllerBase.MenuRendering += ContentTreeController_MenuRendering;
        }

        private void ContentTreeController_MenuRendering(TreeControllerBase sender, MenuRenderingEventArgs e)
        {
            var textService = DependencyResolver.Current.GetService<ILocalizedTextService>();
            var userService = DependencyResolver.Current.GetService<IUserService>();
            var umbracoContext = DependencyResolver.Current.GetService<UmbracoContext>();

            if (int.TryParse(e.NodeId, out int nodeId))
                switch (sender.TreeAlias)
                {
                    case Constants.Trees.Media:
                        if (nodeId != 0 &&
                            nodeId != Constants.System.RecycleBinMedia &&
                            nodeId != Constants.System.Root)
                        {
                            var deleteIndex = e.Menu.Items.FindIndex(x => x.Alias == "delete");

                            var i = new MenuItem
                            {
                                Alias = "convertNode",
                                Name = textService?.Localize("mediaCopy/copy"),
                                Icon = "documents"
                            };
                            i.LaunchDialogView("/App_Plugins/MediaCopy/backoffice/mediaCopy/mediaCopy.html",
                                textService?.Localize("mediaCopy/copy"));

                            if (HasTreePermission(userService, umbracoContext, nodeId, ActionCopy.Instance.Letter.ToString()))
                                e.Menu.Items.Insert(deleteIndex, i);
                        }
                        break;
                }
        }

        private bool HasTreePermission(IUserService userService, UmbracoContext context, int nodeId, string letter)
        {
            var permissions = userService?.GetPermissions(context.Security.CurrentUser, nodeId);
            return permissions?.Any(x => x.AssignedPermissions.InvariantContains(letter)) ?? false;
        }
    }
}