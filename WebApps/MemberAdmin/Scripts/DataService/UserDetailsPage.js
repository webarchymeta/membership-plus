function ChannelType(id, name) {
    var self = this;
    self.id = id;
    self.name = name;
}

function Channel(data) {
    var self = this;
    self.data = data;
    self.addr = ko.observable(data.addr);
    self.comment = ko.observable(data.comment);
    self.selectedType = ko.observable(data.typeId);
    self.isModified = ko.computed(function () {
        return self.addr() != self.data.addr || self.comment() != self.data.comment;
    });
}

function UserDetails() {
    var self = this;
    self.channelTypes = [];
    self.Channels = ko.observableArray([]);
    self.newChannel = { typeId: 0, addr: ko.observable(''), comment: ko.observable('') };
    self.selectedType = ko.observable();
}

var appRoot = '';

function _addNewChannel(data, event, typeMsg, addrMsg) {
    var typeId = data.selectedType();
    var addr = data.newChannel.addr();
    if (typeId) {
        if (addr != '') {
            $.ajax({
                url: appRoot + "Account/AddChannel",
                type: "POST",
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({ typeId: typeId, address: addr, comment: data.newChannel.comment() }),
                success: function (content) {
                    if (!content.ok) {
                        alert(content.msg);
                    } else {
                        data.selectedType(null);
                        data.newChannel.comment('');
                        data.newChannel.addr('');
                        data.Channels.push(new Channel(content.data));
                    }
                },
                error: function (jqxhr, textStatus) {
                    alert(jqxhr.responseText);
                },
                complete: function () {
                }

            });
        } else {
            alert(addrMsg);
        }
    } else {
        alert(typeMsg);
    }
}

function _updateChannel(data, event) {
    $.ajax({
        url: appRoot + "Account/UpdateChannel",
        type: "POST",
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify({ id: data.data.id, address: data.addr(), comment: data.comment() }),
        success: function (content) {
            if (!content.ok) {
                alert(content.msg);
            } else {
                data.data.addr = data.addr();
                data.addr('');
                data.addr(data.data.addr);
                data.data.comment = data.comment();
                data.comment('');
                data.comment(data.data.comment);
            }
        },
        error: function (jqxhr, textStatus) {
            alert(jqxhr.responseText);
        },
        complete: function () {
        }
    });
}

function _deleteChannel(parent, data, event) {
    $.ajax({
        url: appRoot + "Account/DeleteChannel",
        type: "POST",
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify({ id: data.data.id }),
        success: function (content) {
            if (!content.ok) {
                alert(content.msg);
            } else {
                var cnt = parent.Channels().length;
                for (var i = 0; i < cnt; i++) {
                    var c = parent.Channels()[i];
                    if (c.data.id == data.data.id) {
                        parent.Channels.remove(c);
                        break;
                    }
                }
            }
        },
        error: function (jqxhr, textStatus) {
            alert(jqxhr.responseText);
        },
        complete: function () {
        }
    });
}