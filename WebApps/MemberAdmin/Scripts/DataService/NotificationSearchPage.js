// start: query expression view models
function QToken(val) {
    var self = this;
    self.TkType = "";
    self.TkName = val;
    self.DisplayAs = val;
    self.IsExternal = false;
    self.IsEntityAccess = false;
    self.TkClass = "filternode";
    self.SortOrder = 0;
    self.FilterOrder = 0;
    self.CopyToken = function (token) {
        self.TkType = token.TkType;
        self.TkName = token.TkName;
        self.DisplayAs = token.DisplayAs;
        self.IsExternal = token.IsExternal;
        self.IsEntityAccess = token.IsEntityAccess;
        self.TkClass = "filternode " + token.TkType;
    }
}

function TokenOptions() {
    this.Hint = "";
    this.CurrentExpr = ko.observable("");
    this.QuoteVal = false;
    this.CanBeClosed = true;
    this.IsLocal = false;
    this.Options = [];
}

function QueryExpresion(sorters, filters) {
    this.OrderTks = [];
    this.FilterTks = [];
    for (var i = 0; i < sorters.length; i++) {
        this.OrderTks.push(sorters[i]);
    }
    for (var i = 0; i < filters.length; i++) {
        this.FilterTks.push(filters[i]);
    }
}
// end: query expression view model

function Notification(data) {
    var self = this;
    self.Initializing = true;
    self.categ = data.categ;
    self.data = data.data;
    self.IsEntitySelected = ko.observable(false);
    self.Initializing = false;
}

function NotificationPage() {
    var self = this;
    self.Index_ = ko.observable();
    self.PageNumber = ko.computed(function () {
        return self.Index_() + 1;
    });
    self.FirstItem = ko.observable({});
    self.LastItem = ko.observable({});
    self.CurrentItem = ko.observable(null);
    self.IsLastPage = ko.observable(false);
    self.IsDataLoaded = ko.observable(false);
    self.IsPageSelected = ko.observable(false);
    self.Items = ko.observableArray([]);
    self.PageLink = ko.computed(function () {
        return "javascript:loadpage(" + self.Index_() + ")";;
    });

    self.GetPageItems = function (s, callback) {
        if (self.IsDataLoaded())
            return;
        var qexpr = getQueryExpr();
        var lastItem = null;
        var ipage = self.Index_();
        if (self.Index_() > 0) {
            var blk = s.PageBlocks()[s.CurrBlockIndex()];
            if (blk.Pages()[0].Index_() != ipage) {
                for (var i = 0; i < blk.Pages().length; i++) {
                    if (blk.Pages()[i].Index_() == ipage - 1) {
                        lastItem = blk.Pages()[i].LastItem();
                        break;
                    }
                }
            } else {
                var prvb = s.PageBlocks()[s.CurrBlockIndex() - 1];
                lastItem = prvb.Pages()[prvb.Pages().length - 1].LastItem();
            }
        }
        $.ajax({
            url: appRoot + "Notification/GetNotifications",
            //url: .BaseUrl + "/GetPageItems",
            type: "POST",
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({
                set: JSON.stringify({
                    set: setName,
                    pageBlockSize: s.PageBlockSize(),
                    pageSize: s.PageSize_(),
                    setFilter: s.SetFilter,
                    appName: appName
                }),
                qexpr: JSON.stringify(qexpr),
                prevlast: lastItem == null ? null : JSON.stringify(lastItem)
            }),
            beforeSend: function () {
                self.Items.removeAll();
            },
            success: function (content) {
                var items = JSON.parse(content);
                for (var i = 0; i < items.length; i++)
                    self.Items.push(new Notification(items[i]));
                self.IsDataLoaded(true);
                callback(true);
            },
            error: function (jqxhr, textStatus) {
                alert(jqxhr.responseText);
                callback(false);
            },
            complete: function () {
            }
        });
    }
}

// end: entity page

// start: page block

function NotificationPageBlock(idx0, data) {
    var self = this;
    self.BlockIndex = 0;
    self.IsLastBlock = ko.observable(data.IsLastBlock);
    self.BlockCount = data.BlockCount;
    self.Pages = ko.observableArray([]);
    if (data.Pages) {
        for (var i = 0; i < data.Pages.length; i++) {
            var pdata = data.Pages[i];
            var page = new NotificationPage();
            page.Index_(idx0 + pdata.Index_);
            page.FirstItem(pdata.FirstItem);
            page.LastItem(pdata.LastItem);
            page.IsLastPage(pdata.IsLastPage);
            self.Pages.push(page);
        }
    }
    self.LastPage = function () {
        return self.Pages().length == 0 ? null : self.Pages()[self.Pages().length - 1];
    };
}

// end: page block

// start: truncated set view model

// must be set 
var appRoot = null;
var serviceUrl = null;
var loadingPage = false;
var dataSourceId = null;
var dbName = null;
var setName = null;
var adminMaxPriority = 0;

function NotificationSet(dataServiceUrl) {
    var self = this;
    self.BaseUrl = dataServiceUrl;
    self.TotalEntities = ko.observable(0);
    self.EntityCount = ko.observable(0);
    self.PageCount = ko.observable(0);
    self.SetFilter = "";
    self.AvailableRoles = [];
    // query
    self.CurrentSorters = ko.observable(null);
    self.CurrentFilters = ko.observable(null);
    self.SortersStack = [];
    self.FiltersStack = [];
    self.SorterPath = ko.observableArray();
    self.FilterPath = ko.observableArray();
    self.IsQueryStateChanged = ko.observable(false);
    self.IsQueryInitialized = ko.observable(false);
    // page blocks
    self.PageSize_ = ko.observable(pageSize);
    self.PageSize_.subscribe(function (val) {
        self.IsQueryStateChanged(true);
    });
    //self.PageWindowSize = ko.observable(10);
    self.PageBlockSize = ko.observable(pageBlockSize);
    self.PageBlockSize.subscribe(function (val) {
        self.IsQueryStateChanged(true);
    });
    self.PageCount = ko.observable();
    self.PagesWindow = ko.observableArray([]);
    self.CurrentPage = ko.observable({});
    self.CurrentSelectedUser = ko.observable(null);

    self.PageBlocks = ko.observableArray([]);
    self.CurrBlockIndex = ko.observable(0);
    self.CurrentBlock = ko.computed(function () {
        if (self.CurrBlockIndex() < 0 || self.CurrBlockIndex() >= self.PageBlocks().length - 1)
            return null;
        else
            return self.PageBlocks()[self.CurrBlockIndex()];
    });
    self.PrevBlock = ko.computed(function () {
        var idx = self.CurrBlockIndex();
        if (idx > 0) {
            return self.PageBlocks()[idx - 1];
        } else {
            return null;
        }
    });
    self.NextLoadedBlock = ko.computed(function () {
        var idx = self.CurrBlockIndex();
        if (idx >= 0 && idx < self.PageBlocks().length - 1) {
            return self.PageBlocks()[idx + 1];
        } else {
            return null;
        }
    });
    self.MoreNextBlock = ko.computed(function () {
        var idx = self.CurrBlockIndex();
        if (idx >= 0 && idx < self.PageBlocks().length - 1) {
            return true;
        } else {
            return self.PageBlocks().length > 0 && !self.PageBlocks()[self.PageBlocks().length - 1].IsLastBlock();
        }
    });
    self.LastPageBlock = ko.computed(function () {
        return self.PageBlocks().length == 0 ? null : self.PageBlocks()[self.PageBlocks().length - 1];
    });

    self.ResetPageState = function () {
        self.CurrBlockIndex(0);
        self.PageBlocks.removeAll();
        self.IsQueryStateChanged(false)
    };

    self.GetSetInfo = function () {
        $.ajax({
            url: self.BaseUrl + "/GetSetInfo",
            type: "POST",
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({
                sourceId: dataSourceId,
                set: JSON.stringify({ set: setName, setFilter: self.SetFilter })
            }),
            beforeSend: function () {
            },
            success: function (content) {
                var r = JSON.parse(content.GetSetInfoResult);
                self.TotalEntities(r.EntityCount);
                self.CurrentSorters(new TokenOptions());
                for (var i = 0; i < r.Sorters.length; i++) {
                    var tk = r.Sorters[i];
                    if (typeof tokenNameMap != 'undefined') {
                        if (tokenNameMap(tk, setName, false)) {
                            self.CurrentSorters().Options.push(tk);
                        }
                    } else {
                        self.CurrentSorters().Options.push(tk);
                    }
                }
                self.CurrentSorters().CanBeClosed = true;
                self.CurrentSorters().isLocal = false;
            },
            error: function (jqxhr, textStatus) {
                alert(jqxhr.responseText);
            },
            complete: function () {
            }
        });
    };

    self.GetNextSorterOps = function (callback) {
        var qtokens = [];
        for (var i = 0; i < self.SorterPath().length; i++)
            qtokens.push(self.SorterPath()[i]);
        $.ajax({
            url: self.BaseUrl + "/GetNextSorterOps",
            type: "POST",
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({
                sourceId: dataSourceId,
                set: setName,
                sorters: JSON.stringify(qtokens)
            }),
            beforeSend: function () {
            },
            success: function (content) {
                self.SortersStack.push(self.CurrentSorters());
                self.CurrentSorters(new TokenOptions());
                var r = JSON.parse(content.GetNextSorterOpsResult);
                self.CurrentSorters().Hint = r.Hint;
                self.CurrentSorters().CurrentExpr(r.CurrentExpr);
                self.CurrentSorters().QuoteVal = r.QuoteVal;
                self.CurrentSorters().CanBeClosed = r.CanBeClosed;
                self.CurrentSorters().IsLocal = false;
                for (var i = 0; i < r.Options.length; i++) {
                    var tk = new QToken();
                    tk.CopyToken(r.Options[i]);
                    if (typeof tokenNameMap != 'undefined') {
                        if (tokenNameMap(tk, setName, false)) {
                            self.CurrentSorters().Options.push(tk);
                        }
                    } else {
                        self.CurrentSorters().Options.push(tk);
                    }
                }
                callback(true);
            },
            error: function (jqxhr, textStatus) {
                alert(jqxhr.responseText);
                callback(false);
            },
            complete: function () {
            }
        });
    };

    self.FilterClosed = ko.observable(false);

    self.GetNextFilterOps = function (callback) {
        var qexpr = getQueryExpr();
        $.ajax({
            url: self.BaseUrl + "/GetNextFilterOps",
            type: "POST",
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({
                sourceId: dataSourceId,
                set: setName,
                qexpr: JSON.stringify(qexpr)
            }),
            beforeSend: function () {
            },
            success: function (content) {
                if (self.CurrentFilters() != null)
                    self.FiltersStack.push(self.CurrentFilters());
                var r = JSON.parse(content.GetNextFilterOpsResult);
                self.FilterClosed(r.CanBeClosed);
                self.CurrentFilters(new TokenOptions());
                self.CurrentFilters().Hint = r.Hint;
                self.CurrentFilters().CurrentExpr(r.CurrentExpr);
                self.CurrentFilters().QuoteVal = r.QuoteVal;
                self.CurrentFilters().CanBeClosed = r.CanBeClosed;
                self.CurrentFilters().IsLocal = false;
                for (var i = 0; i < r.Options.length; i++) {
                    var tk = new QToken();
                    tk.CopyToken(r.Options[i]);
                    if (typeof tokenNameMap != 'undefined') {
                        if (tokenNameMap(tk, setName, true)) {
                            self.CurrentFilters().Options.push(tk);
                        }
                    } else {
                        self.CurrentFilters().Options.push(tk);
                    }
                }
                callback(true);
            },
            error: function (jqxhr, textStatus) {
                alert(jqxhr.responseText);
                callback(false);
            },
            complete: function () {
            }
        });
    };

    self.NextPageBlock = function (qexpr, last, callback) {
        if (self.IsQueryStateChanged())
            self.ResetPageState();
        if (self.CurrBlockIndex() < self.PageBlocks().length) {
            callback(true, false);
            return;
        }
        $.ajax({
            url: self.BaseUrl + "/NextPageBlock",
            type: "POST",
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({
                sourceId: dataSourceId,
                set: JSON.stringify({
                    set: setName,
                    pageBlockSize: self.PageBlockSize(),
                    pageSize: self.PageSize_(),
                    setFilter: self.SetFilter
                }),
                qexpr: JSON.stringify(qexpr),
                prevlast: last == null ? null : JSON.stringify(last)
            }),
            beforeSend: function () {
            },
            success: function (content) {
                var data = JSON.parse(content.NextPageBlockResult);
                self.EntityCount(data.TotalEntities);
                self.PageCount(data.TotalPages);
                if (data.Pages.length == 0) {
                    var lpb = self.LastPageBlock();
                    if (lpb != null) {
                        lpb.IsLastBlock(true);
                        var lp = lpb.LastPage();
                        if (lp != null) {
                            lp.IsLastPage(true);
                            self.CurrBlockIndex(self.CurrBlockIndex() - 1);
                        }
                    } else {
                        self.PagesWindow.removeAll();
                    }
                }
                else {
                    var idx0 = 0;
                    for (var i = 0; i < self.CurrBlockIndex() ; i++) {
                        idx0 += self.PageBlocks()[i].BlockCount;
                    }
                    var pb = new NotificationPageBlock(idx0, data);
                    pb.BlockIndex = self.PageBlocks().length;
                    self.PageBlocks.push(pb);
                    self.PagesWindow.removeAll();
                    for (var i = 0; i < pb.Pages().length; i++) {
                        self.PagesWindow.push(pb.Pages()[i]);
                    }
                }
                self.IsQueryStateChanged(false);
                callback(true, true);
            },
            error: function (jqxhr, textStatus) {
                alert(jqxhr.responseText);
                callback(false, false);
            },
            complete: function () {
            }
        });
    };
}

function tokenSortCmp(a, b) {
    if (a.SortOrder == b.SortOrder) {
        if (a.DisplayAs > b.DisplayAs)
            return 1;
        else if (a.DisplayAs < b.DisplayAs)
            return -1;
        else
            return 0;
    } else {
        return b.SortOrder - a.SortOrder;
    }
}

function tokenFilterCmp(a, b) {
    if (a.FilterOrder == b.FilterOrder) {
        if (a.DisplayAs > b.DisplayAs)
            return 1;
        else if (a.DisplayAs < b.DisplayAs)
            return -1;
        else
            return 0;
    } else {
        return b.FilterOrder - a.FilterOrder;
    }
}
function initsortinput(s) {
    if (loadingPage) {
        return;
    }
    var bobj = $("#queryExec");
    enableQuery(false);
    $("#filterOpts").hide();
    var iobj = $("#sortOpts");
    var deleting = false;
    iobj.removeAttr("disabled");
    iobj.css("cursor", "");
    iobj.keydown(function (event) {
        deleting = event.which == 8;
        if (event.which == 13) {
            event.preventDefault();
        }
        else if (event.which == 8 && s.SorterPath().length > 0 && this.value == "") {
            s.SorterPath.pop();
            s.CurrentSorters(s.SortersStack.pop());
            $(this).autocomplete("close");
            var tk = s.SorterPath().length > 0 ? s.SorterPath()[s.SorterPath().length - 1] : null;
            if (tk == null || tk.TkName != "asc" && tk.TkName != "desc") {
                enableQuery(false);
                $("#filterOpts").hide();
            } else {
                enableQuery(true);
                $("#filterOpts").show();
            }
        }
    });
    iobj.autocomplete({
        minLength: 0,
        delay: 0,
        autoFocus: true,
        source: function (request, response) {
            if (!s.CurrentSorters() || !s.CurrentSorters().Options)
                return;
            var opts = s.CurrentSorters().Options;
            var arr = opts.filter(function (val) {
                return val.DisplayAs.toLowerCase().indexOf(request.term.toLowerCase()) == 0;
            }).sort(tokenSortCmp);
            if (arr.length != 1 || deleting) {
                response($.map(arr, function (item) {
                    return { label: item.DisplayAs == "this" ? item.TkName : item.DisplayAs, value: item.DisplayAs };
                }));
                if (arr.length > 1 && !deleting) {
                    //iobj.val(arr[0].DisplayAs);
                }
            } else {
                iobj.autocomplete("close");
                var tk = arr[0];
                s.SorterPath.push(tk);
                iobj.val("");
                iobj.attr("disabled", "disabled");
                iobj.css("cursor", "progress");
                if (tk == null || tk.TkName != "asc" && tk.TkName != "desc") {
                    enableQuery(false);
                    $("#filterOpts").hide();
                } else {
                    enableQuery(true);
                    $("#filterOpts").show();
                    s.IsQueryStateChanged(true);
                }
                s.GetNextSorterOps(function (ok) {
                    if (ok) {
                        iobj.removeAttr("disabled");
                        if (s.CurrentSorters().Options.length == 2 && s.CurrentSorters().Options[0].IsExternal) {
                            s.CurrentSorters().Options[0].ImgSrc = "glyphicon glyphicon-chevron-up"; //appRoot + 'Images/control_up.png';
                            s.CurrentSorters().Options[1].ImgSrc = "glyphicon glyphicon-chevron-down"; //appRoot + 'Images/control_down.png';
                        }
                        if (s.CurrentSorters().CanBeClosed && s.CurrentSorters().CurrentExpr() != null && s.CurrentSorters().CurrentExpr() != "") {
                            $('.sortlabel').qtip({
                                overwrite: true,
                                content: s.CurrentSorters().CurrentExpr(),
                                position: {
                                    my: 'left bottom',
                                    at: 'top left',
                                    target: $('.sortlabel')
                                },
                                show: {
                                    event: 'mouseover',
                                    ready: false,
                                    solo: true
                                },
                                hide: 'mouseout'
                            });
                        }
                    }
                    iobj.focus();
                    iobj.css("cursor", "");
                });
                return false;
            }
        },
        select: function (event, ui) {
            var tk = null;
            var opts = s.CurrentSorters().Options;
            for (var i = 0; i < opts.length; i++) {
                if (opts[i].DisplayAs == ui.item.value) {
                    tk = opts[i];
                    break;
                }
            }
            if (tk != null) {
                s.SorterPath.push(tk);
                iobj.val("");
                iobj.attr("disabled", "disabled");
                iobj.css("cursor", "progress");
                if (tk == null || tk.TkName != "asc" && tk.TkName != "desc") {
                    enableQuery(false);
                    $("#filterOpts").hide();
                } else {
                    enableQuery(true);
                    $("#filterOpts").show();
                    s.IsQueryStateChanged(true);
                }
                s.GetNextSorterOps(function (ok) {
                    if (ok) {
                        iobj.removeAttr("disabled");
                        if (s.CurrentSorters().Options.length == 2 && s.CurrentSorters().Options[0].IsExternal) {
                            s.CurrentSorters().Options[0].ImgSrc = "glyphicon glyphicon-chevron-up"; //appRoot + 'Images/control_up.png';
                            s.CurrentSorters().Options[1].ImgSrc = "glyphicon glyphicon-chevron-down"; //appRoot + 'Images/control_down.png';
                        }
                        if (s.CurrentSorters().CanBeClosed && s.CurrentSorters().CurrentExpr() != null && s.CurrentSorters().CurrentExpr() != "") {
                            $('.sortlabel').qtip({
                                overwrite: true,
                                content: s.CurrentSorters().CurrentExpr(),
                                position: {
                                    my: 'left bottom',
                                    at: 'top left',
                                    target: $('.sortlabel')
                                },
                                show: {
                                    event: 'mouseover',
                                    ready: false,
                                    solo: true
                                },
                                hide: 'mouseout'
                            });
                        }
                    }
                    iobj.focus();
                    iobj.css("cursor", "");
                });
                return false;
            }
        }
    });
}

function initfilterinput(s) {
    if (loadingPage) {
        return;
    }
    var bobj = $("#queryExec");
    enableQuery(false);
    var iobj = $("#filterOpts");
    var deleting = false;
    iobj.showingHint = false;
    iobj.removeAttr("disabled");
    iobj.css("cursor", "");
    iobj.keydown(function (event) {
        deleting = event.which == 8;
        if (event.which == 13) {
            event.preventDefault();
            if (this.value != "" && s.CurrentFilters().Options.length == 0) {
                var tk;
                if (s.CurrentFilters().QuoteVal) {
                    var strv = "";
                    var scnt = 0;
                    for (var i = 0; i < this.value.length; i++) {
                        var c = this.value[i];
                        if (c == '"') {
                            for (j = 0; j < scnt; j++)
                                strv += '\\';
                            if (scnt % 2 == 0)
                                strv += '\\';
                            strv += c;
                            scnt = 0;
                        } else if (c == '\\') {
                            scnt++;
                        } else {
                            for (j = 0; j < scnt; j++)
                                strv += '\\';
                            if (scnt % 2 != 0)
                                strv += '\\';
                            strv += c;
                            scnt = 0;
                        }
                    }
                    tk = new QToken('"' + strv + '"');
                    tk.TkClass = "filternode StringValue";
                } else {
                    tk = new QToken(this.value);
                    tk.TkClass = "filternode Value";
                }
                if (iobj.showingHint) {
                    $(iobj).qtip('toggle', false);
                    iobj.showingHint = false;
                }
                s.FilterPath.push(tk);
                iobj.val("");
                iobj.attr("disabled", "disabled");
                iobj.css("cursor", "progress");
                s.GetNextFilterOps(function (ok) {
                    if (ok) {
                        iobj.removeAttr("disabled");
                        var closed = s.CurrentFilters().CanBeClosed;
                        enableQuery(closed);
                        if (closed)
                            s.IsQueryStateChanged(true);
                        if (s.CurrentFilters().CanBeClosed && s.CurrentFilters().CurrentExpr() != null && s.CurrentFilters().CurrentExpr() != "") {
                            $('.filterlabel').qtip({
                                overwrite: true,
                                content: s.CurrentFilters().CurrentExpr(),
                                position: {
                                    my: 'left top',
                                    at: 'bottom right',
                                    target: $('.filterlabel')
                                },
                                show: {
                                    event: 'mouseover',
                                    ready: false,
                                    solo: true
                                },
                                hide: 'mouseout'
                            });
                        }
                    }
                    iobj.focus();
                    iobj.css("cursor", "");
                });
                return false;
            }
        }
        else if (event.which == 8 && s.FilterPath().length > 0 && this.value == "") {
            s.FilterPath.pop();
            s.CurrentFilters(s.FiltersStack.pop());
            var closed = s.CurrentFilters().CanBeClosed;
            s.FilterClosed(closed);
            enableQuery(closed);
            if (closed)
                s.IsQueryStateChanged(true);
        } else {
            if (s.CurrentFilters().Hint != null && s.CurrentFilters().Hint != "") {
                if (!iobj.showingHint) {
                    $(iobj).qtip({
                        overwrite: true,
                        content: s.CurrentFilters().Hint,
                        position: {
                            my: 'left bottom',
                            at: 'top left',
                            target: $(iobj)
                        },
                        show: {
                            event: "none",
                            ready: true,
                            solo: true
                        },
                        hide: "unfocus",
                        events: {
                            show: function () {
                                iobj.showingHint = true;
                            },
                            hide: function () {
                                iobj.showingHint = false;
                            }
                        }
                    });
                    $(iobj).qtip('toggle', true);
                    iobj.showingHint = true;
                }
            }
            else {
                if (iobj.showingHint) {
                    $(iobj).qtip('toggle', false);
                    iobj.showingHint = false;
                }
            }
        }
    });
    iobj.focus(function (event) {
        if (s.CurrentFilters() == null) {
            iobj.attr("disabled", "disabled");
            iobj.css("cursor", "progress");
            s.GetNextFilterOps(function (ok) {
                if (ok) {
                    iobj.removeAttr("disabled");
                    iobj.focus();
                }
                iobj.css("cursor", "");
            });
        }
    });
    iobj.autocomplete({
        minLength: 0,
        delay: 0,
        autoFocus: true,
        source: function (request, response) {
            var opts = s.CurrentFilters().Options;
            var arr = opts.filter(function (val) {
                return val.DisplayAs.toLowerCase().indexOf(request.term.toLowerCase()) == 0;
            }).sort(tokenFilterCmp);
            if (arr.length != 1 || deleting) {
                response($.map(arr, function (item) {
                    return { label: item.DisplayAs, value: item.DisplayAs };
                }));
            } else {
                iobj.autocomplete("close");
                var tk = arr[0];
                s.FilterPath.push(tk);
                iobj.val("");
                iobj.attr("disabled", "disabled");
                iobj.css("cursor", "progress");
                s.GetNextFilterOps(function (ok) {
                    if (ok) {
                        iobj.removeAttr("disabled");
                        var closed = s.CurrentFilters().CanBeClosed;
                        enableQuery(closed);
                        if (closed)
                            s.IsQueryStateChanged(true);
                        if (s.CurrentFilters().CanBeClosed && s.CurrentFilters().CurrentExpr() != null && s.CurrentFilters().CurrentExpr() != "") {
                            $('.filterlabel').qtip({
                                overwrite: true,
                                content: s.CurrentFilters().CurrentExpr(),
                                position: {
                                    my: 'left bottom',
                                    at: 'top left',
                                    target: $('.filterlabel')
                                },
                                show: {
                                    event: 'mouseover',
                                    ready: false,
                                    solo: true
                                },
                                hide: 'mouseout'
                            });
                        }
                    }
                    iobj.focus();
                    iobj.css("cursor", "");
                });
                return false;
            }
        },
        select: function (event, ui) {
            var tk = null;
            var opts = s.CurrentFilters().Options;
            for (var i = 0; i < opts.length; i++) {
                if (opts[i].DisplayAs == ui.item.value) {
                    tk = opts[i];
                    break;
                }
            }
            if (tk != null) {
                s.FilterPath.push(tk);
                iobj.val("");
                iobj.attr("disabled", "disabled");
                iobj.css("cursor", "progress");
                s.GetNextFilterOps(function (ok) {
                    if (ok) {
                        iobj.removeAttr("disabled");
                        var closed = s.CurrentFilters().CanBeClosed;
                        enableQuery(closed);
                        if (closed)
                            s.IsQueryStateChanged(true);
                        if (s.CurrentFilters().CanBeClosed && s.CurrentFilters().CurrentExpr() != null && s.CurrentFilters().CurrentExpr() != "") {
                            $('.filterlabel').qtip({
                                overwrite: true,
                                content: s.CurrentFilters().CurrentExpr(),
                                position: {
                                    my: 'left bottom',
                                    at: 'top left',
                                    target: $('.filterlabel')
                                },
                                show: {
                                    event: 'mouseover',
                                    ready: false,
                                    solo: true
                                },
                                hide: 'mouseout'
                            });
                        }
                    }
                    iobj.focus();
                    iobj.css("cursor", "");
                });
                return false;
            }
        }
    });
}

function getQueryExpr() {
    var sorters = [];
    var filters = [];
    for (var i = 0; i < notificationSet.SorterPath().length; i++)
        sorters.push(notificationSet.SorterPath()[i]);
    for (var i = 0; i < notificationSet.FilterPath().length; i++)
        filters.push(notificationSet.FilterPath()[i]);
    return new QueryExpresion(sorters, filters);
}

function _setWait(on) {
    if (on) {
        $(".content-wrapper").css({ "background-color": "rgba(230,230,230,100)" });
        $("html").addClass("waiting");
    } else {
        $(".content-wrapper").css({ "background-color": "rgba(255,255,255,0)" });
        $("html").removeClass("waiting");
    }
}

function setWait(on) {
    _setWait(on);
    enableQuery(!on);
}

var queryCompleted = false;

function enableQuery(enable) {
    queryCompleted = enable;
    if (enable) {
        $("#queryExec").removeClass("disabled");
    } else {
        $("#queryExec").addClass("disabled");
    }
}

function showlist(e) {
    if (!queryCompleted)
        return;
    var qexpr = getQueryExpr();
    if (notificationSet.IsQueryStateChanged()) {
        notificationSet.ResetPageState();
    }
    notificationSet.NextPageBlock(qexpr, null, function (ok, ch) {
        if (ch && notificationSet.CurrentPage() != null && !(typeof notificationSet.CurrentPage().Items === 'undefined')) {
            notificationSet.CurrentPage().Items.removeAll();
        }
        if (ok) {
            notificationSet.IsQueryInitialized(true);
            if (ch && notificationSet.PageBlocks().length > 0 && notificationSet.PageBlocks()[0].Pages().length > 0) {
                loadpage(0);
            }
        } else {

        }
    });
}

function prevPageBlock() {
    if (loadingPage) {
        return;
    }
    var idx = notificationSet.CurrBlockIndex();
    if (idx > 0) {
        notificationSet.CurrBlockIndex(idx - 1);
        notificationSet.PagesWindow.removeAll();
        var ipage = -1;
        for (var i = 0; i < notificationSet.PageBlocks()[idx - 1].Pages().length; i++) {
            var p = notificationSet.PageBlocks()[idx - 1].Pages()[i];
            notificationSet.PagesWindow.push(p);
            if (p.IsPageSelected()) {
                ipage = p.Index_();
            }
        }
        loadpage(ipage == -1 ? 0 : ipage);
    }
}

function nextPageBlock() {
    if (loadingPage) {
        return;
    }
    var idx = notificationSet.CurrBlockIndex();
    if (idx < notificationSet.PageBlocks().length - 1) {
        notificationSet.CurrBlockIndex(idx + 1);
        notificationSet.PagesWindow.removeAll();
        var ipage = -1;
        for (var i = 0; i < notificationSet.PageBlocks()[idx + 1].Pages().length; i++) {
            var p = notificationSet.PageBlocks()[idx + 1].Pages()[i];
            notificationSet.PagesWindow.push(p);
            if (p.IsPageSelected()) {
                ipage = p.Index_();
            }
        }
        loadpage(ipage == -1 ? 0 : ipage);
    } else {
        idx = notificationSet.PageBlocks().length - 1;
        var b = notificationSet.PageBlocks()[idx];
        if (!b.IsLastBlock()) {
            notificationSet.CurrBlockIndex(idx + 1);
            var p = b.LastPage();
            if (p == null) {
                return;
            }
            var qexpr = getQueryExpr();
            notificationSet.NextPageBlock(qexpr, p.LastItem(), function (ok, ch) {
                if (ok) {
                    if (notificationSet.PageBlocks().length > 0 && notificationSet.PageBlocks()[idx + 1].Pages().length > 0) {
                        loadpage(notificationSet.PageBlocks()[idx + 1].Pages()[0].Index_());
                    }
                }
            });
        }
    }
}

function loadpage(index) {
    if (loadingPage) {
        return;
    }
    loadingPage = true;
    var p = null;
    var p0 = null;
    var blk = notificationSet.PageBlocks()[notificationSet.CurrBlockIndex()];
    for (var i = 0; i < blk.Pages().length; i++) {
        var _p = blk.Pages()[i];
        if (_p.Index_() == index) {
            p = _p;
        } else if (_p.IsPageSelected()) {
            p0 = _p;
        }
    }
    setWait(true)
    if (p != null) {
        if (!p.IsDataLoaded()) {
            p.GetPageItems(notificationSet, function (ok) {
                if (ok) {
                    updateCurrPage(p, p0);
                    for (var i = 0; i < p.Items().length; i++) {
                        //
                        //p.Items()[i].TobeLoadMsg(serverMessage1);
                    }
                } else {

                }
                loadingPage = false;
                setWait(false)
            });
        } else {
            updateCurrPage(p, p0);
            loadingPage = false;
            setWait(false)
        }
    } else {
        loadingPage = false;
        setWait(false)
    }
}

function updateCurrPage(p, p0) {
    notificationSet.CurrentPage(p);
    p.IsPageSelected(true);
    if (p0 != null) {
        p0.IsPageSelected(false);
    }
}

function selectEntity(data, event) {
    for (var i = 0; i < notificationSet.CurrentPage().Items().length; i++) {
        var e = notificationSet.CurrentPage().Items()[i];
        if (e.IsEntitySelected() && e != data) {
            e.IsEntitySelected(false);
        }
    }
    notificationSet.CurrentSelectedUser(data);
    data.IsEntitySelected(true);
    notificationSet.CurrentPage().CurrentItem(data);
    //updateEntityDetails(data);
    event.stopPropagation();
    return false;
}

var notificationSet = null;
var appName = '';
var pageBlockSize = '10';
var pageSize = '10';

// end set view model
