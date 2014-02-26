____getDateVal = function (val) {
    if (val == null) {
        return null;
    }
    var tv = val.substr(6);
    tv = tv.substr(0, tv.length - 2);
    var ms = parseInt(tv);
    return new Date(ms);
};
ko.bindingHandlers.localdatetime = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        //        ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
        //            $(element).datetimepicker("destroy");
        //        });
    },
    update: function (element, valueAccessor) {
        var value = ko.utils.unwrapObservable(valueAccessor());
        var current = $(element).text();
        if (value) {
            var dt = ____getDateVal(value).toLocaleString();
            if (dt != current) {
                $(element).text(____getDateVal(value).toLocaleString());
            }
        }
    }
}