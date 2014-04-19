function RoomAdmin(data) {
    var self = this;
    self.id = data.id;
    self.name = data.name;
    self.role = data.role;
}

function ChatRoom(data, id) {
    var self = this;
    self.id = ko.observable(id);
    self.name = ko.observable('');
    self.descr = ko.observable(data.descr);
    self.active = ko.observable(data.active);
    self.subscribers = ko.observable(data.subscribers);
    self.admins = ko.observableArray();
    if (data.admins.length > 0) {
        for (var i = 0; i < data.admins.length; i++) {
            self.admins.push(new RoomAdmin(data.admins[i]));
        }
    }
}

var appRoot;

function ChatContext() {
    var self = this;
    self.Current = ko.observable();
    self.loadRoomDetails = function(par) {
        if (initializing) {
            return;
        }
        $.ajax({
            url: appRoot + "GroupChat/LoadRoomSummary?id=" + par.id,
            type: "GET",
            success: function (content) {
                var room = new ChatRoom(content);
                room.name(par.name);
                context.Current(room);
            },
            error: function (jqxhr, textStatus) {
                alert(jqxhr.responseText);
                callback(false);
            }
        });
    }
}

var context;
var initializing = true;

$(function () {
    context = new ChatContext();
    ko.applyBindings(context);
    initializing = false;
});

