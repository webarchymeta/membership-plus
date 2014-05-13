var modalInitialized = false;

function startPrivateChat(data, event) {
    var panel = $("#privateChatPopup");
    var userName = $("#userName");
    var content = $("#chatPage");
    userName.text(data.data.Username);
    if (!modalInitialized) {
        panel.modal({ keyboard: false, backdrop: "static" });
        panel.on("hidden.bs.modal", function () {
            content.contents().find("document").trigger('unload');
            content[0].src = 'about:blank';
        });
        modalInitialized = true;
    }
    panel.modal('show');
    content[0].src = appRoot + 'PrivateChat/ChatPopup?toId=' + data.data.ID; 
    event.stopPropagation();
    return false;
}

function startPrivateMessage(data, event) {
    var panel = $("#privateChatPopup");
    var userName = $("#userName");
    var content = $("#chatPage");
    userName.text(data.data.Username);
    if (!modalInitialized) {
        panel.modal({ keyboard: false, backdrop: "static" });
        panel.on("hidden.bs.modal", function () {
            content.contents().find("document").trigger('unload');
            content[0].src = 'about:blank';
        });
        modalInitialized = true;
    }
    panel.modal('show');
    content[0].src = appRoot + 'PrivateChat/MessagePopup?toId=' + data.data.ID;
    event.stopPropagation();
    return false;
}