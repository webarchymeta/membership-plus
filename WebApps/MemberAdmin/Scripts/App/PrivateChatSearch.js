var modalInitialized = false;

function startPrivateChat(data, event) {
    var panel = $("#privateChatPopup");
    var userName = $("#userName");
    var content = $("#chatPage");
    if (typeof data.data != 'undefined')
        userName.text(data.data.Username);
    else if (typeof data.from != 'undefined')
        userName.text(data.from.name);
    if (!modalInitialized) {
        panel.modal({ keyboard: false, backdrop: "static" });
        panel.on("hidden.bs.modal", function () {
            content.contents().find("document").trigger('unload');
            content[0].src = 'about:blank';
        });
        modalInitialized = true;
    }
    panel.modal('show');
    if (typeof data.data != 'undefined')
        content[0].src = appRoot + 'PrivateChat/ChatPopup?toId=' + data.data.ID;
    else if (typeof data.from != 'undefined')
        content[0].src = appRoot + 'PrivateChat/ChatPopup?toId=' + data.from.id;
    event.stopPropagation();
    return false;
}

function startPrivateMessage(data, event) {
    var panel = $("#privateChatPopup");
    var userName = $("#userName");
    var content = $("#chatPage");
    if (typeof data.data != 'undefined')
        userName.text(data.data.Username);
    else if (typeof data.from != 'undefined')
        userName.text(data.from.name);
    if (!modalInitialized) {
        panel.modal({ keyboard: false, backdrop: "static" });
        panel.on("hidden.bs.modal", function () {
            content.contents().find("document").trigger('unload');
            content[0].src = 'about:blank';
        });
        modalInitialized = true;
    }
    panel.modal('show');
    if (typeof data.data != 'undefined')
        content[0].src = appRoot + 'PrivateChat/MessagePopup?toId=' + data.data.ID;
    else if (typeof data.from != 'undefined')
        content[0].src = appRoot + 'PrivateChat/MessagePopup?toId=' + data.from.id;
    event.stopPropagation();
    return false;
}