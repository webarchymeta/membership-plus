(function ($) {
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

    //CKEDITOR.disableAutoInline = true;
    //CKEDITOR.inline('editable');

    ko.bindingHandlers.simpleHtmlEditor = {
        init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
            var txtBoxID = $(element).attr("id");
            var options = allBindingsAccessor().richTextOptions || {
                'toolbar': [
                     ['Font', 'FontSize', 'TextColor', '-', 'Bold', 'Italic', 'Underline'],
                     ['NumberedList', 'BulletedList', '-', 'Blockquote', '-', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'],
                     ['Smiley', 'Link', 'Unlink', 'Table']
                ]
            };
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

    var instances_by_id = {}; // needed for referencing instances during updates.
    var init_queue = $.Deferred(); // jQuery deferred object used for creating TinyMCE instances synchronously
    init_queue.resolve();

    ko.bindingHandlers.tinymce = {
        init: function (element, valueAccessor, allBindingsAccessor, context) {

            var options = allBindingsAccessor().tinymceOptions || {
                'plugins': ['link', 'image', 'emoticons'],
                'toolbar': 'undo redo | bold italic | bullist numlist | link image emoticons',
                'menubar': false,
                'statusbar': false,
                'schema': 'html5',
                'setup': function (editor) {
                    editor.on('init', function (e) {
                        console.log('wysiwyg initialised');
                    });
                }
            };
            var modelValue = valueAccessor();
            var value = ko.utils.unwrapObservable(valueAccessor());
            var $element = $(element);

            options.setup = function (ed) {
                ed.on('change', function (e) {
                    if (ko.isWriteableObservable(modelValue)) {
                        var current = modelValue();
                        if (current !== this.getContent()) {
                            modelValue(this.getContent());
                        }
                    }
                });
                ed.on('keyup', function (e) {
                    if (ko.isWriteableObservable(modelValue)) {
                        var current = modelValue();
                        var editorValue = this.getContent({ format: 'raw' });
                        if (current !== editorValue) {
                            modelValue(editorValue);
                        }
                    }
                });
                ed.on('beforeSetContent', function (e, l) {
                    if (ko.isWriteableObservable(modelValue)) {
                        if (typeof (e.content) != 'undefined') {
                            var current = modelValue();
                            if (current !== e.content) {
                                modelValue(e.content);
                            }
                        }
                    }
                });
            };

            //handle destroying an editor 
            ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
                $(element).parent().find("span.mceEditor,div.mceEditor").each(function (i, node) {
                    var tid = node.id.replace(/_parent$/, ''),
                        ed = tinymce.get(tid);
                    if (ed) {
                        ed.remove();
                        // remove referenced instance if possible.
                        if (instances_by_id[tid]) {
                            delete instances_by_id[tid];
                        }
                    }
                });
            });

            setTimeout(function () {
                if (!element.id) {
                    element.id = tinymce.DOM.uniqueId();
                }
                tinyMCE.init(options);
                tinymce.execCommand("mceAddEditor", true, element.id);
            }, 0);
            $element.html(value);

        },
        update: function (element, valueAccessor, allBindingsAccessor, context) {
            var $element = $(element),
                value = ko.utils.unwrapObservable(valueAccessor()),
                id = $element.attr('id');

            //handle programmatic updates to the observable
            // also makes sure it doesn't update it if it's the same. 
            // otherwise, it will reload the instance, causing the cursor to jump.
            if (id !== undefined) {
                var tinymceInstance = tinyMCE.get(id);
                if (!tinymceInstance)
                    return;
                var content = tinymceInstance.getContent({ format: 'raw' });
                if (content !== value) {
                    //$element.html(value);
                    //this should be more proper but ctr+c, ctr+v is broken, above need fixing
                    tinymceInstance.setContent(value,{ format: 'raw' })
                }
            }
        }
    };
}(jQuery));