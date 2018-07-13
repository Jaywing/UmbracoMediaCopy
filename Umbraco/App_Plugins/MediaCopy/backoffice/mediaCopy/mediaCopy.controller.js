angular
  .module("umbraco")
  .controller("MediaCopy.CopyController", function(
    $scope,
    userService,
    eventsService,
    mediaExtendedResource,
    appState,
    treeService,
    navigationService
  ) {
    var dialogOptions = $scope.dialogOptions;
    
    $scope.loading = false;

    $scope.dialogTreeEventHandler = $({});
    var node = dialogOptions.currentNode;

    $scope.treeModel = { hideHeader: false };

    userService.getCurrentUser().then(function(userData) {
      $scope.treeModel.hideHeader =
        userData.startMediaIds.length > 0 &&
        userData.startMediaIds.indexOf(-1) == -1;
    });

    function nodeSelectHandler(ev, args) {
      if (args && args.event) {
        args.event.preventDefault();
        args.event.stopPropagation();
      }

      eventsService.emit("MediaCopy.Controller.select", args);

      if ($scope.target) {
        $scope.target.selected = false;
      }

      $scope.target = args.node;
      $scope.target.selected = true;
      $scope.error = null;
    }

    function nodeExpandedHandler(ev, args) {
      if (args.node.metaData.isContainer) {
        openMiniListView(args.node);
      }
    }

    $scope.dialogTreeEventHandler.bind("treeNodeSelect", nodeSelectHandler);
    $scope.dialogTreeEventHandler.bind("treeNodeExpanded", nodeExpandedHandler);

    $scope.copy = function() {
      $scope.loading = true;

      mediaExtendedResource
        .copy({
          id: node.id,
          destinationId: $scope.target ? $scope.target.id : -1
        })
        .then(
          function(result) {
            $scope.loading = false;

            if (result != undefined && !result.Success) {
              $scope.success = false;
              $scope.error = true;
              $scope.errorMessage = result.Message;
            } else {
              $scope.error = false;
              $scope.success = true;

              var activeNode = appState.getTreeState("selectedNode");

              navigationService
                .syncTree({
                  tree: "media",
                  path: result.Path,
                  forceReload: true,
                  activate: false
                })
                .then(function(args) {
                  if (activeNode) {
                    var activeNodePath = treeService.getPath(activeNode).join();

                    navigationService.syncTree({
                      tree: "media",
                      path: activeNodePath,
                      forceReload: false,
                      activate: true
                    });
                  }
                });
            }
          },
          function(error) {
            $scope.loading = false;
            $scope.success = false;
            $scope.error = true;
            $scope.errorMessage = error.errorMsg;
          }
        );
    };

    $scope.$on("$destroy", function() {
      $scope.dialogTreeEventHandler.unbind("treeNodeSelect", nodeSelectHandler);
      $scope.dialogTreeEventHandler.unbind(
        "treeNodeExpanded",
        nodeExpandedHandler
      );
    });

    // Mini list view
    $scope.selectListViewNode = function(node) {
      node.selected = node.selected === true ? false : true;
      nodeSelectHandler({}, { node: node });
    };

    $scope.closeMiniListView = function() {
      $scope.miniListView = undefined;
    };

    function openMiniListView(node) {
      $scope.miniListView = node;
    }
  });
