(function () {
    "use strict";

    var jsFileLocation = "/DesktopModules/Aricie.DigitalDisplays/js/ng-scripts/";

    angular
        .module("itemApp", ["ngRoute", "ngDialog", "ngProgress", "ui.sortable"])
        .config(function ($routeProvider) {
            $routeProvider.
                otherwise({
                    templateUrl: jsFileLocation + "Templates/index.html",
                    controller: "itemController",
                    controllerAs: "vm"
                });
        });
})();