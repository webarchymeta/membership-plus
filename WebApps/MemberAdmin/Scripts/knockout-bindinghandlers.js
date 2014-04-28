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

ko.bindingHandlers.simpleHtmlEditor = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        var txtBoxID = $(element).attr("id");
        var options = allBindingsAccessor().richTextOptions || {};
        options.toolbar = [
             ['Font', 'FontSize', 'TextColor', '-', 'Bold', 'Italic', 'Underline'],
             ['NumberedList', 'BulletedList', '-', 'Blockquote', '-', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'],
             ['Smiley', 'Link', 'Unlink', 'Table']
        ];
        options.uiColor = '#F49A68';
        options.removePlugins = 'elementspath';
        options.resize_enabled = false;
        options.toolbarLocation = 'bottom';
        options.height = '80px';
        //handle disposal (if KO removes by the template binding)
        ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
            if (CKEDITOR.instances[txtBoxID]) { CKEDITOR.remove(CKEDITOR.instances[txtBoxID]); };
        });
        //$(element).ckeditor(options);
        CKEDITOR.replace(txtBoxID, options);
        //wire up the blur event to ensure our observable is properly updated
        CKEDITOR.instances[txtBoxID].focusManager.blur = function () {
            var observable = valueAccessor();
            var x = CKEDITOR.instances[txtBoxID].getData();
            observable(x);
        };
    },
    update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        var val = ko.utils.unwrapObservable(valueAccessor());
        var txtBoxID = $(element).attr("id");
        CKEDITOR.instances[txtBoxID].setData(val);
        //$(element).val(val);
    }
}