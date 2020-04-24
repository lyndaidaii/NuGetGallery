window["initializeNuGetInstrumentation"] = function (config) {
    var instrumentation = {
        initialize: function () {
            console.log("NuGet instrumentation stub: initialized %o", config);
        },
        trackMetric: function (metric, customProperties) {
            console.log("NuGet instrumentation stub: metric %o %o", metric, customProperties);
        }
    };
    instrumentation.initialize();
    return instrumentation;
};
